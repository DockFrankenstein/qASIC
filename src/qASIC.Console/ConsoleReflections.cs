using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace qASIC.Console
{
    internal class ConsoleReflections : qEnviromentInitializer
    {
        public static Dictionary<string, LogColorAttribute> ColorAttributeMethods { get; private set; } = new Dictionary<string, LogColorAttribute>();
        public static Dictionary<string, LogColorAttribute> ColorAttributeDeclaringTypes { get; private set; } = new Dictionary<string, LogColorAttribute>();

        public static Dictionary<string, LogPrefixAttribute> PrefixAttributeMethods { get; private set; } = new Dictionary<string, LogPrefixAttribute>();
        public static Dictionary<string, LogPrefixAttribute> PrefixAttributeDeclaringTypes { get; private set; } = new Dictionary<string, LogPrefixAttribute>();

        public override void Initialize()
        {
            ColorAttributeMethods = TypeFinder.FindMethodsWithAttribute<LogColorAttribute>()
                .ToDictionary(x => CreateMethodId(x), x => x.GetCustomAttribute<LogColorAttribute>()!);

            ColorAttributeDeclaringTypes = TypeFinder.FindClassesWithAttribute<LogColorAttribute>()
                .ToDictionary(x => CreateTypeId(x), x => x.GetCustomAttribute<LogColorAttribute>()!);

            PrefixAttributeMethods = TypeFinder.FindMethodsWithAttribute<LogPrefixAttribute>()
                .ToDictionary(x => CreateMethodId(x), x => x.GetCustomAttribute<LogPrefixAttribute>()!);

            PrefixAttributeDeclaringTypes = TypeFinder.FindClassesWithAttribute<LogPrefixAttribute>()
                .ToDictionary(x => CreateTypeId(x), x => x.GetCustomAttribute<LogPrefixAttribute>()!);
        }

        public static string CreateMethodId(MethodBase method) =>
            method != null ?
            $"{method.DeclaringType?.FullName}/{method}" :
            string.Empty;

        public static string CreateTypeId(Type type) =>
            type.FullName ?? string.Empty;
    }
}