using qASIC.Core.Serialization.Serializers;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;

namespace qASIC.Serialization.Serializers
{
    public class ConfigSerializer
    {
        public const string INDENT = "  ";
        private const BindingFlags DEFAULT_FLAGS = BindingFlags.Public;

        private static HashSet<Type> ExcludedTypes = new HashSet<Type>()
        {
            typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), 
            typeof(Action<,,,>), typeof(Action<,,,,>), typeof(Action<,,,,,>), 
            typeof(Action<,,,,,,>), typeof(Action<,,,,,,,>), typeof(Action<,,,,,,,,>), 
            typeof(Action<,,,,,,,,,>), typeof(Action<,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,>), 
            typeof(Action<,,,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,,,,>), 
            typeof(Action<,,,,,,,,,,,,,,,>),

            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>),
            typeof(Func<,,,,>), typeof(Func<,,,,,>), typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>), typeof(Func<,,,,,,,,>), typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,,>),
        };

        public BindingFlags Flags { get; set; } = DEFAULT_FLAGS;

        #region Serialization
        /// <summary>Serializes an object to a text string.</summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>The serialized string.</returns>
        public string Serialize(object obj)
        {
            if (!obj.GetType().IsSerializable)
                return string.Empty;

            var val = CreateConfigForObject(obj);
            return Serialize(val);
        }

        /// <summary>Creates a config value for an object.</summary>
        /// <param name="obj">Object that will be used for creating the config value.</param>
        /// <returns>A config value that represents the object's data.</returns>
        ConfigValue CreateConfigForObject(object obj)
        {
            if (obj == null)
                return new ConfigValue();

            var type = obj.GetType();

            if (obj is IDictionary dict)
            {
                ConfigValue dictVal = new ConfigValue();
                foreach (DictionaryEntry item in dict)
                    dictVal.Add(item.Key.ToString(), CreateConfigForObject(item.Value));

                return dictVal;
            }

            if (obj is IEnumerable<object> enumerable)
            {
                if (IsSimple(type.GetElementType() ?? type.GetGenericArguments().Single()))
                    return new ConfigValue(enumerable.Select(x => x?.ToString() ?? string.Empty).ToArray());

                var enumVal = new ConfigValue();
                int index = 0;
                foreach (var item in enumerable)
                {
                    enumVal.Add(index.ToString(), CreateConfigForObject(item));
                    index++;
                }

                return enumVal;
            }

            var members = GetMembers(type);

            if (IsSimple(type))
                return new ConfigValue(obj.ToString());

            var val = new ConfigValue();
            foreach (var item in members)
            {
                string name;
                object memberValue;
                switch (item)
                {
                    case FieldInfo field:
                        name = field.Name;
                        memberValue = field.GetValue(obj);
                        break;
                    case PropertyInfo property:
                        if (!property.CanRead || !property.CanWrite)
                            continue;

                        name = property.Name;
                        memberValue = property.GetValue(obj);
                        break;
                    default:
                        continue;
                }

                val.Add(name, CreateConfigForObject(memberValue));
            }

            return val;
        }

        /// <summary>Serializes a config value to a text string.</summary>
        /// <param name="configValue">Config Value to serialize.</param>
        /// <returns>The serialized string.</returns>
        public string Serialize(ConfigValue configValue)
        {
            if (configValue == null)
                return string.Empty;

            if (configValue.Count == 0)
                return $"{configValue.GetFinalCommentString()}{configValue.Value}";

            return Serialize(configValue, string.Empty).TrimStart('\n');
        }

        string Serialize(ConfigValue configValue, string indent)
        {
            if (configValue == null)
                return string.Empty;

            if (configValue.Count == 0)
                return FormatValueForExport(configValue.Value);

            StringBuilder txt = new StringBuilder();

            foreach (var item in configValue)
            {
                if (item.Value == null)
                {
                    txt.Append($"\n{indent}{FormatValueForExport(item.Key)}");
                    continue;
                }

                if (item.Value.ArrayValue.Length > 0)
                {
                    txt.Append($"\n{indent}{item.Key}:\n{indent}{INDENT}{string.Join($"\n{indent}{INDENT}", item.Value.ArrayValue)}");
                    continue;
                }

                txt.Append($"\n{item.Value.GetFinalCommentString(indent)}{indent}{item.Key}:{Serialize(item.Value, $"{indent}{INDENT}")}");
            }

            return txt.ToString();
        }
        #endregion

        #region Deserialization
        /// <summary>Deserializes an object from a text string.</summary>
        /// <typeparam name="T">Type of the resulting object.</typeparam>
        /// <param name="txt">The formatted text containing values.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string txt) =>
            (T)Deserialize(txt, typeof(T));

        /// <summary>Deserializes an object from a text string.</summary>
        /// <param name="txt">The formatted text containing values.</param>
        /// <param name="type">Type of the resulting object.</param>
        /// <returns>The deserialized object.</returns>
        public object Deserialize(string txt, Type type)
        {
            var configValue = Deserialize(txt);
            object obj = TypeFinder.CreateConstructorFromType(type);
            OverrideMemberFromConfig(configValue, ref obj, type);
            return obj;
        }

        /// <summary>Overwrites an object using deserialized values from a text string.</summary>
        /// <param name="txt">The formatted text containing values.</param>
        /// <param name="obj">The deserialized object.</param>
        public void DeserializeOverwrite(string txt, object obj)
        {
            var configValue = Deserialize(txt);
            OverrideMemberFromConfig(configValue, ref obj, obj.GetType());
        }

        void OverrideMemberFromConfig(ConfigValue configValue, ref object obj, Type type)
        {
            if (obj is IDictionary dict)
            {
                var dictArgs = dict.GetType().GetGenericArguments();
                var keyType = dictArgs[0];
                var valType = dictArgs[1];

                foreach (var item in configValue)
                {
                    try
                    {
                        var key = Convert.ChangeType(item.Key, keyType);
                        object val = null;
                        OverrideMemberFromConfig(item.Value, ref val, valType);

                        if (dict.Contains(key))
                        {
                            dict[key] = val;
                            continue;
                        }

                        dict.Add(key, val);
                    }
                    catch { }
                }

                return;
            }

            if (obj is IEnumerable<object> enumerable)
            {
                var elType = type.GetElementType() ?? type.GetGenericArguments().Single();
                var isSimple = IsSimple(elType);

                if (isSimple)
                {
                    var simpleItems = configValue.ArrayValue
                        .SelectMany(x =>
                        {
                            try
                            {
                                object val = Convert.ChangeType(x, elType);
                                return new object[] { val };
                            }
                            catch { }
                            return new object[0];
                        });

                    try
                    {
                        obj = Convert.ChangeType(simpleItems, type);
                    }
                    catch { }

                    return;
                }

                var items = configValue
                    .Select(x =>
                    {
                        object val = null;
                        OverrideMemberFromConfig(x.Value, ref val, elType);
                        return val;
                    });

                try
                {
                    obj = Convert.ChangeType(items, type);
                }
                catch { }

                return;
            }

            if (IsSimple(type) || type.IsAssignableFrom(typeof(object)))
            {
                obj = Convert.ChangeType(configValue.Value, type);
                return;
            }

            var members = GetMembers(type);

            foreach (var item in members)
            {
                if (!configValue.Contains(item.Name))
                    continue;

                var itemConfigValue = configValue[item.Name];
                Type itemType;
                object itemValue;

                switch (item)
                {
                    case FieldInfo field:
                        itemType = field.FieldType;
                        itemValue = obj != null ?
                            field.GetValue(obj) :
                            null;
                        break;
                    case PropertyInfo property:
                        if (!property.CanWrite)
                            continue;

                        itemType = property.PropertyType;

                        itemValue = (obj != null && property.CanRead) ?
                            property.GetValue(obj) :
                            null;
                        break;
                    default:
                        continue;
                }

                OverrideMemberFromConfig(itemConfigValue, ref itemValue, itemType);

                switch (item)
                {
                    case FieldInfo field:
                        field.SetValue(obj, itemValue);
                        break;
                    case PropertyInfo property:
                        property.SetValue(obj, itemValue);
                        break;
                }
            }
        }

        /// <summary>Deserializes a text string.</summary>
        /// <param name="txt">The formatted text containing values.</param>
        /// <returns>The deserialized value.</returns>
        public ConfigValue Deserialize(string txt)
        {
            //txt = Regex.Replace(txt, "(?=#)(?!##.*)(?<!#).*?(.*)", string.Empty)
            txt = string.Join("#", txt.Split(new string[] { "##" }, StringSplitOptions.None)
                .Select(x => Regex.Replace(txt, "(?=#).*?(.*)", string.Empty)));

            var lines = txt
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Prepend(string.Empty);

            return Deserialize(ref lines, 0, true);
        }

        ConfigValue Deserialize(ref IEnumerable<string> lines, int indent, bool first = false)
        {
            var elements = GetLineElements(lines.First());
            lines = lines.Skip(1);

            if (!first)
            {
                if (elements.Length == 1)
                    return null;

                if (!string.IsNullOrWhiteSpace(elements[1]) ||
                    (lines.Count() > 0 && GetIndentCount(lines.First()) != indent))
                    return new ConfigValue(FormatValueForImport(elements[1]));
            }

            var val = new ConfigValue();
            List<string> arrayValues = new List<string>();

            while (lines.Count() > 0)
            {
                var currentLine = lines.First();
                var lineIndent = GetIndentCount(currentLine);
                currentLine = RemoveIndent(currentLine);

                //If line's indent is higher
                //This means that there was some formatting error, so
                //we just ignore the line
                if (lineIndent > indent)
                {
                    lines = lines.Skip(1);
                    continue;
                }

                //If line's indent is lower
                //This means that it's out of this config value's scope
                if (lineIndent < indent)
                {
                    break;
                }

                var itemName = GetLineElements(currentLine).First();
                var itemValue = Deserialize(ref lines, indent + 1);

                switch (itemValue == null)
                {
                    case true:
                        arrayValues.Add(FormatValueForImport(itemName));
                        break;
                    case false:
                        val.Add(itemName, itemValue);
                        break;
                }
            }

            val.ArrayValue = arrayValues.ToArray();
            return val;
        }
        #endregion

        #region Utility
        bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        MemberInfo[] GetMembers(Type type) =>
            type.GetMembers(Flags | BindingFlags.Instance)
            .Concat(TypeFinder.FindPropertiesWithAttribute<SerializableAttribute>())
            .Concat(TypeFinder.FindFieldsWithAttribute<SerializableAttribute>())
            .Distinct()
            .Except(type.GetDefaultMembers())
            .Where(x => (x.MemberType.HasFlag(MemberTypes.Field) || x.MemberType.HasFlag(MemberTypes.Property)) &&
                !x.Name.StartsWith("<") &&
                x.DeclaringType.IsSerializable &&
                x.GetCustomAttribute(typeof(NonSerializedAttribute)) == null &&
                (!(x is PropertyInfo property) || (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)) &&
                !IsTypeExcluded(x))
            .ToArray();

        bool IsTypeExcluded(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo field:
                    if (ExcludedTypes.Contains(field.FieldType)) return true;
                    if (!field.FieldType.IsGenericType) return false;

                    return ExcludedTypes.Contains(field.FieldType.GetGenericTypeDefinition());
                case PropertyInfo property:
                    if (ExcludedTypes.Contains(property.PropertyType)) return true;
                    if (!property.PropertyType.IsGenericType) return false;

                    return ExcludedTypes.Contains(property.PropertyType.GetGenericTypeDefinition());
                default:
                    return true;
            }
        }

        int GetIndentCount(string txt)
        {
            int i = 0;
            while (txt.StartsWith(INDENT))
            {
                i++;
                txt = txt.Substring(INDENT.Length, txt.Length - INDENT.Length);
            }

            return i;
        }

        string[] GetLineElements(string s)
        {
            var elements = Regex.Split(s, ":(?!:)(?<!:.)");

            if (elements.Length <= 2)
                return elements;

            return new string[]
            {
                elements[0],
                string.Join(":", elements.Skip(1)),
            };
        }

        string RemoveIndent(string s) =>
            s.TrimStart();

        string FormatValueForExport(string s) =>
            s
            //.Replace(":", "::")
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("#", "##");

        string FormatValueForImport(string s)
        {
            s = Regex.Replace(s, "\\n(?<!\\..)", "\n");
            return s
                //.Replace("::", ":")
                .Replace("\\\\", "\\");
        }
        #endregion
    }
}