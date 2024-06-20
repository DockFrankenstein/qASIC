using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using qASIC.Console.Commands.Prompts;

namespace qASIC.Console
{
    public struct CommandArgs : IEnumerable<ConsoleArgument>, IEnumerable
    {
        public string inputString;
        public string commandName;
        public ConsoleArgument[] args;
        public GameConsole console;
        public CommandPrompt prompt;

        public ConsoleArgument this[int index]
        {
            get => args[index];
            set => args[index] = value;
        }

        public readonly int Length => args.Length;

        public void CheckArgumentCount(int count) =>
            CheckArgumentCount(count, count);

        public void CheckArgumentCount(int min, int max)
        {
            var argsCount = Length - 1;
            bool valid = min <= argsCount && argsCount <= max;

            if (!valid)
                throw new CommandArgsCountException(argsCount, min, max);
        }

        public IEnumerator<ConsoleArgument> GetEnumerator() =>
            args
            .AsEnumerable()
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            args.GetEnumerator();
    }

    public struct ConsoleArgument
    {
        public ConsoleArgument(string arg) : this(arg, new object[0]) { }

        public ConsoleArgument(string arg, object[] parsedValues)
        {
            this.arg = arg;
            this.parsedValues = parsedValues;
        }

        public string arg;
        public object[] parsedValues;

        public static explicit operator string(ConsoleArgument arg) =>
            arg.arg.ToString();

        public T GetValue<T>() =>
            (T)GetValue(typeof(T));

        public object GetValue(Type type)
        {
            var result = TryGetValue(type, out var obj);
            if (!result) throw new CommandParseException(type, arg);
            return obj!;
        }

        public bool TryGetValue<T>(out T value)
        {
            var result = TryGetValue(typeof(T), out var obj);
            value = (T)obj;
            return result;
        }

        public bool TryGetValue(Type type, out object value)
        {
            foreach (var item in parsedValues)
            {
                var itemType = item.GetType();
                if (!type.IsAssignableFrom(itemType)) continue;
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public bool CanGetValue<T>() =>
            CanGetValue(typeof(T));

        public bool CanGetValue(Type type) =>
            TryGetValue(type, out _);

        public override string ToString() =>
            arg;
    }
}