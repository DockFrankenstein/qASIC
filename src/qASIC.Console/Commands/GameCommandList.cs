using System.Collections;
using System.Reflection;

namespace qASIC.Console.Commands
{
    public class GameCommandList : IEnumerable<IGameCommand>
    {
        private List<RegisteredCommand> Commands { get; set; } = new List<RegisteredCommand>();

        /// <summary>Adds command to the list.</summary>
        /// <param name="command">Command to add.</param>
        public GameCommandList AddCommand(IGameCommand command)
        {
            var registeredCommand = new RegisteredCommand(command);
            Commands.Add(registeredCommand);
            return this;
        }

        public GameCommandList FindBuiltInCommands() =>
            FindCommands<BuildInConsoleCommandAttribute>();

        /// <summary>Finds and adds commands to the list that use <see cref="StandardConsoleCommandAttribute"/>.</summary>
        public GameCommandList FindCommands() =>
            FindCommands<StandardConsoleCommandAttribute>();

        /// <summary>Finds and adds commands to the list that use the specified attribute.</summary>
        /// <typeparam name="T">Type of attribute used by target commands.</typeparam>
        public GameCommandList FindCommands<T>() where T : Attribute =>
            FindCommands(typeof(T));

        /// <summary>Finds and adds commands to the list that use the specified attribute.</summary>
        /// <param name="type">Type of attribute used by target commands.</param>
        public GameCommandList FindCommands(Type type)
        {
            var commandTypes = TypeFinder.FindAllClassesWithAttribute(type, BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => typeof(IGameCommand).IsAssignableFrom(x));

            var commands = TypeFinder.CreateConstructorsFromTypes<IGameCommand>(commandTypes);

            foreach (var command in commands)
                if (command != null)
                    AddCommand(command);

            return this;
        }

        public GameCommandList FindAttributeCommands() =>
            FindAttributeCommands<CommandAttribute>();

        public GameCommandList FindAttributeCommands<T>() where T : CommandAttribute =>
            FindAttributeCommands(typeof(T));

        public GameCommandList FindAttributeCommands(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var methods = TypeFinder.FindMethodsAttributes(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var fields = TypeFinder.FindFieldAttributes(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var properties = TypeFinder.FindPropertyAttributes(type, bindingFlags)
                .Select(x => x as MemberInfo);

            var targets = methods
                .Concat(fields)
                .Concat(properties);

            foreach (var method in targets)
            {
                var attr = (CommandAttribute?)method.GetCustomAttribute(type);
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

                var methodTarget = new GameAttributeCommand.Target(method);

                command.Targets.Add(methodTarget);

                if (!commandExists)
                    AddCommand(command);
            }

            return this;
        }

        /// <summary>Tries to find command.</summary>
        /// <param name="commandName">Name of the command, doesn't need to be lowercase.</param>
        /// <param name="command">Found command.</param>
        /// <returns>Returns if it found a command.</returns>
        public bool TryGetCommand(string commandName, out IGameCommand? command)
        {
            commandName = commandName.ToLower();

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