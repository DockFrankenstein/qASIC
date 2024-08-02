using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace qASIC.Console
{
    internal static class ConsoleReflections
    {
        private static Dictionary<string, LogColorAttribute> _colorAttributeMethods = null;
        public static Dictionary<string, LogColorAttribute> ColorAttributeMethods
        {
            get
            {
                if (_colorAttributeMethods == null)
                    _colorAttributeMethods = TypeFinder.FindMethodsWithAttribute<LogColorAttribute>(FLAGS)
                        .ToDictionary(x => CreateMethodId(x), x => x.GetCustomAttribute<LogColorAttribute>()); ;

                return _colorAttributeMethods;
            }
        }

        private static Dictionary<string, LogColorAttribute> _colorAttributeDeclaringTypes = null;
        public static Dictionary<string, LogColorAttribute> ColorAttributeDeclaringTypes
        { 
            get
            {
                if (_colorAttributeDeclaringTypes == null)
                    _colorAttributeDeclaringTypes = TypeFinder.FindClassesWithAttribute<LogColorAttribute>(FLAGS)
                        .ToDictionary(x => CreateTypeId(x), x => x.GetCustomAttribute<LogColorAttribute>());

                return _colorAttributeDeclaringTypes;
            }
        }

        private static Dictionary<string, LogPrefixAttribute> _prefixAttributeMethods = null;
        public static Dictionary<string, LogPrefixAttribute> PrefixAttributeMethods 
        { 
            get
            {
                if (_prefixAttributeMethods == null)
                    _prefixAttributeMethods = TypeFinder.FindMethodsWithAttribute<LogPrefixAttribute>(FLAGS)
                        .ToDictionary(x => CreateMethodId(x), x => x.GetCustomAttribute<LogPrefixAttribute>());

                return _prefixAttributeMethods;
            }
        }

        private static Dictionary<string, LogPrefixAttribute> _prefixAttributeDeclaringTypes = null;
        public static Dictionary<string, LogPrefixAttribute> PrefixAttributeDeclaringTypes 
        { 
            get
            {
                if (_prefixAttributeDeclaringTypes == null)
                    _prefixAttributeDeclaringTypes = TypeFinder.FindClassesWithAttribute<LogPrefixAttribute>(FLAGS)
                        .ToDictionary(x => CreateTypeId(x), x => x.GetCustomAttribute<LogPrefixAttribute>());

                return _prefixAttributeDeclaringTypes;
            }
        }

        public static void Initialize()
        {
            _ = ColorAttributeMethods;
            _ = ColorAttributeDeclaringTypes;
            _ = PrefixAttributeMethods;
            _ = PrefixAttributeDeclaringTypes;
        }

        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static string CreateMethodId(MethodBase method) =>
            method != null ?
            $"{method.DeclaringType?.FullName}/{method}" :
            string.Empty;

        public static string CreateTypeId(Type type) =>
            type.FullName ?? string.Empty;
    }
}