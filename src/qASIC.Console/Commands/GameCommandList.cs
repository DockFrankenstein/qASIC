using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace qASIC.Console.Commands
{
    public class GameCommandList : IEnumerable<IGameCommand>
    {
        private List<RegisteredCommand> Commands { get; set; } = new List<RegisteredCommand>();

        public Action<IEnumerable<IGameCommand>> OnCommandsAdded;

        /// <summary>Adds command to the list.</summary>
        /// <param name="command">Command to add.</param>
        /// <returns>Returns itself.</returns>
        public GameCommandList AddCommand(IGameCommand command) =>
            AddCommandRage(new IGameCommand[] { command });

        /// <summary>Adds commands to the list.</summary>
        /// <param name="command">Collection of commands to add.</param>
        /// <returns>Returns itself.</returns>
        public GameCommandList AddCommandRage(IEnumerable<IGameCommand> commands)
        {
            Commands.AddRange(commands.Select(x => new RegisteredCommand(x)));
            OnCommandsAdded?.Invoke(commands);
            return this;
        }

        /// <summary>Adds all built-in commands to the list.</summary>
        /// <returns>Returns itself.</returns>
        public GameCommandList AddBuildInCommands() =>
            AddCommandRage(new IGameCommand[]
            {
                new BuiltIn.Clear(),
                new BuiltIn.Echo(),
                new BuiltIn.Exit(),
                new BuiltIn.Hello(),
                new BuiltIn.Help(),
                new BuiltIn.Version(),
            });

        /// <summary>Finds and adds commands to the list that use <see cref="ConsoleCommandAttribute"/>.</summary>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindCommands() =>
            FindCommands<ConsoleCommandAttribute>();

        /// <summary>Finds and adds commands to the list that use the specified attribute.</summary>
        /// <typeparam name="T">Type of attribute used by target commands.</typeparam>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindCommands<T>() where T : Attribute =>
            FindCommands(typeof(T));

        /// <summary>Finds and adds commands to the list that use the specified attribute.</summary>
        /// <param name="type">Type of attribute used by target commands.</param>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindCommands(Type type)
        {
            var commandTypes = TypeFinder.FindClassesWithAttribute(type, BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => typeof(IGameCommand).IsAssignableFrom(x));

            var commands = TypeFinder.CreateConstructorsFromTypes<IGameCommand>(commandTypes)
                .Where(x => x != null);

            AddCommandRage(commands);

            return this;
        }

        /// <summary>Finds and adds methods, properties and fields marked with <see cref="CommandAttribute"/>.</summary>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindAttributeCommands() =>
            FindAttributeCommands<CommandAttribute>();

        /// <summary>Finds and adds methods, properties and fields marked with the specified attribute.</summary>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindAttributeCommands<T>() where T : CommandAttribute =>
            FindAttributeCommands(typeof(T));

        /// <summary>Finds and adds methods, properties and fields marked with the specified attribute.</summary>
        /// <returns>Returns itself.</returns>
        public GameCommandList FindAttributeCommands(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var methods = TypeFinder.FindMethodsWithAttribute(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var fields = TypeFinder.FindFieldsWithAttribute(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var properties = TypeFinder.FindPropertiesWithAttribute(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var targets = methods
                .Concat(fields)
                .Concat(properties);

            var addedCommands = new List<IGameCommand>();
            foreach (var member in targets)
            {
                var attr = (CommandAttribute)member.GetCustomAttribute(type);
                if (attr == null) continue;

                var commandName = attr.Name.ToLower();

                var commandExists = Commands
                    .Any(x => x.command.CommandName.ToLower() == commandName);

                var command = commandExists ?
                    (GameAttributeCommand)Commands.Where(x => x.command.CommandName == commandName).First().command :
                    null;

                command ??= new GameAttributeCommand()
                {
                    CommandName = commandName,
                };

                var memberTarget = GameAttributeCommand.Target.CreateFromMember(member);

                command.Targets.Add(memberTarget);

                if (commandExists) continue;

                addedCommands.Add(command);
                Commands.Add(new RegisteredCommand(command));
            }

            OnCommandsAdded?.Invoke(addedCommands);
            return this;
        }

        /// <summary>Tries to find command.</summary>
        /// <param name="commandName">Name of the command, doesn't need to be lowercase.</param>
        /// <param name="command">Found command.</param>
        /// <returns>Returns if it found a command.</returns>
        public bool TryGetCommand(string commandName, out IGameCommand command)
        {
            commandName = commandName?.ToLower();

            var targets = Commands
                .Where(x => x.names.Contains(commandName))
                .Select(x => x.command);

            command = targets.FirstOrDefault();
            return command != null;
        }

        public IEnumerator<IGameCommand> GetEnumerator() =>
            Commands
            .Select(x => x.command)
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public IGameCommand this[int index]
        {
            get => Commands[index].command;
        }

        public int Length =>
            Commands.Count;

        class RegisteredCommand
        {
            public RegisteredCommand(IGameCommand command)
            {
                this.command = command;

                names = command.Aliases
                    .Prepend(command.CommandName)
                    .Select(x => x.ToLower())
                    .ToArray();
            }

            public string[] names;
            public IGameCommand command;
        }
    }
}