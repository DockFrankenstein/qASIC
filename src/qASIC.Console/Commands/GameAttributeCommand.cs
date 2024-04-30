using qASIC.Console.Commands.Attributes;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qASIC.Console.Commands
{
    public class GameAttributeCommand : IGameCommand
    {
        public GameAttributeCommand() : this(string.Empty) { }
        public GameAttributeCommand(string commandName)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }

        public string[] Aliases { get; set; } = new string[0];

        public string Description { get; set; }

        public string DetailedDescription { get; set; }

        public List<Target> Targets { get; set; } = new List<Target>();

        public object Run(CommandArgs args)
        {
            var maxArgLimit = Targets
                .Select(x => x.maxArgsCount)
                .Max();

            var minArgLimit = Targets
                .Select(x => x.minArgsCount)
                .Min();

            args.CheckArgumentCount(minArgLimit, maxArgLimit);

            ConsoleArgument[] cmdArgs = args.args
                .Skip(1)
                .ToArray();

            var targets = Targets
                .Where(x => cmdArgs.Length >= x.minArgsCount && cmdArgs.Length <= x.maxArgsCount)
                .ToArray();

            List<Type>[] supportedArgTypes = new List<Type>[maxArgLimit]
                .Select(x => new List<Type>())
                .ToArray();

            foreach (var target in targets)
                for (int i = 0; i < target.argTypes.Length; i++)
                    supportedArgTypes[i].Add(target.argTypes[i]);

            for (int i = 0; i < cmdArgs.Length; i++)
                cmdArgs[i].parsedValues = cmdArgs[i].parsedValues
                    .Where(x => supportedArgTypes[i].Contains(x.GetType()) || x is string)
                    .ToArray();

            object returnValue = null;

            int closestMatchCorrectArgsCount = -1;
            Target? closestMatch = null;

            if (FindCommandAndTryRun(new List<object>()))
                return returnValue;

            throw new CommandParseException(closestMatch!.Value.argTypes[closestMatchCorrectArgsCount], args[closestMatchCorrectArgsCount + 1].arg);

            bool FindCommandAndTryRun(List<object> values, bool first = true)
            {
                if (values.Count < cmdArgs.Length)
                {
                    var index = values.Count;
                    values.Add(new object());
                    foreach (var value in cmdArgs[index].parsedValues)
                    {
                        values[index] = value;
                        if (FindCommandAndTryRun(values, false))
                            return true;

                        if (index == cmdArgs.Length - 1 && RunFromValues(values))
                            return true;
                    }
                }

                if (cmdArgs.Length == 0 && first && RunFromValues(new List<object>()))
                    return true;

                return false;
            }

            bool RunFromValues(List<object> values)
            {
                var valueTypes = values
                    .Select(x => x.GetType())
                    .ToArray();

                foreach (var target in targets)
                {
                    var finalValues = values;

                    var targetArgTypes = new Type[valueTypes.Length];
                    Array.Copy(target.argTypes, targetArgTypes, targetArgTypes.Length);

                    if (target.forwardCommandArgs)
                        finalValues.Insert(0, args);

                    var parameterCount = target.maxArgsCount;

                    if (target.forwardCommandArgs)
                        parameterCount++;

                    while (finalValues.Count() < parameterCount)
                        finalValues.Add(Type.Missing);

                    int argCount = 0;
                    for (; argCount < valueTypes.Length; argCount++)
                        if (valueTypes[argCount] != targetArgTypes[argCount])
                            break;

                    if (argCount > closestMatchCorrectArgsCount)
                    {
                        closestMatch = target;
                        closestMatchCorrectArgsCount = argCount;
                    }

                    if (argCount != valueTypes.Length)
                        continue;

                    returnValue = target.Invoke(finalValues.ToArray(), args);
                    return true;
                }

                return false;
            }
        }

        public struct Target
        {
            public Target(MemberInfo memberInfo)
            {
                this.memberInfo = memberInfo;
                attr = memberInfo.GetCustomAttribute<CommandAttribute>()!;
                targetAttr = memberInfo.GetCustomAttributes<CommandTargetAttribute>()
                    .ToArray();

                switch (memberInfo)
                {
                    case MethodInfo methodInfo:
                        var parameters = methodInfo.GetParameters();

                        forwardCommandArgs = parameters.Length > 0 && parameters[0].ParameterType == typeof(CommandArgs);

                        if (forwardCommandArgs)
                            parameters = parameters
                                .Skip(1)
                                .ToArray();

                        minArgsCount = parameters
                            .Where(x => !x.IsOptional)
                            .Count();

                        maxArgsCount = parameters.Length;

                        argTypes = parameters
                            .Select(x => x.ParameterType)
                            .ToArray();
                        break;
                    case FieldInfo fieldInfo:
                        forwardCommandArgs = false;
                        minArgsCount = 0;
                        maxArgsCount = 1;
                        argTypes = new Type[] { fieldInfo.FieldType! };
                        break;
                    case PropertyInfo propertyInfo:
                        forwardCommandArgs = false;
                        minArgsCount = 0;
                        maxArgsCount = 1;
                        argTypes = new Type[] { propertyInfo.PropertyType! };
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            public object Invoke(object[] values, CommandArgs args)
            {
                var targetType = memberInfo.DeclaringType!;
                var targets = targetAttr
                    .SelectMany(x => x.GetTargets(targetType))
                    .Where(x => x != null)
                    .Distinct();

                if (attr.UseRegisteredTargets)
                {
                    var regTargets = args.console.Targets
                        .Where(x => targetType.IsAssignableFrom(x.GetType()));

                    targets = targets
                        .Concat(regTargets);
                }

                var singleTarget = targets.Count() == 1;

                switch (memberInfo)
                {
                    case MethodInfo methodInfo:
                        if (methodInfo.IsStatic)
                            return methodInfo.Invoke(null, values);

                        foreach (var item in targets)
                        {
                            LogExecuteBegin(item);
                            var val = args.console.Execute(args.commandName, () => methodInfo.Invoke(item, values));

                            if (singleTarget)
                                return val;
                        }

                        return null;
                    case FieldInfo fieldInfo:
                        if (values.Length != 1) return null;

                        if (fieldInfo.IsStatic)
                        {
                            if (values[0] == Type.Missing)
                                return fieldInfo.GetValue(null); ;

                            fieldInfo.SetValue(null, values[0]);
                        }

                        foreach (var item in targets)
                        {
                            LogExecuteBegin(item);
                            var val = args.console.Execute(args.commandName, () =>
                            {
                                fieldInfo.SetValue(item, values[0]);
                                return null;
                            });

                            if (singleTarget)
                                return val;
                        }

                        return null;
                    case PropertyInfo propertyInfo:
                        if (values.Length != 1) return null;

                        if (propertyInfo.GetAccessors(true)[0].IsStatic)
                        {
                            if (values[0] == Type.Missing)
                                return propertyInfo.GetValue(null);

                            propertyInfo.SetValue(null, values[0]);
                        }

                        foreach (var item in targets)
                        {
                            LogExecuteBegin(item);
                            var val = args.console.Execute(args.commandName, () =>
                            {
                                propertyInfo.SetValue(item, values[0]);
                                return null;
                            });

                            if (singleTarget)
                                return val;
                        }

                        return null;
                    default:
                        throw new NotImplementedException();
                }

                void LogExecuteBegin(object target) =>
                    args.console.Log($"Executing command for target '{target ?? "NULL"}'");
            }

            public MemberInfo memberInfo;
            public CommandAttribute attr;
            public CommandTargetAttribute[] targetAttr;
            public Type[] argTypes;
            public int minArgsCount;
            public int maxArgsCount;
            /// <summary>Whenever target has <see cref="CommandArgs"/> as the first parameter</summary>
            public bool forwardCommandArgs;
        }
    }
}