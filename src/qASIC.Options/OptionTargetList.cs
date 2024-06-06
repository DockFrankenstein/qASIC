using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace qASIC.Options
{
    /// <summary>List of found option targets that were marked with an <see cref="OptionAttribute"/>.</summary>
    public class OptionTargetList : IEnumerable<KeyValuePair<string, OptionTargetList.Target>>
    {
        public OptionTargetList()
        {

        }

        public OptionTargetList(OptionTargetList other)
        {
            Targets = new Dictionary<string, List<Target>>(other.Targets);
            Flags = other.Flags;
        }

        public BindingFlags Flags { get; set; } = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Dictionary<string, List<Target>> Targets { get; set; } = new Dictionary<string, List<Target>>();

        public List<Target> this[string name]
        {
            get
            {
                name = OptionsManager.FormatKeyString(name);

                if (!Targets.ContainsKey(name))
                    Targets.Add(name, new List<Target>());

                return Targets[name];
            }
            set
            {
                name = OptionsManager.FormatKeyString(name);

                if (Targets.ContainsKey(name))
                {
                    Targets.Add(name, value);
                    return;
                }

                Targets[name] = value;
            }
        }

        public OptionTargetList FindOptions() =>
            FindOptions<OptionAttribute>();

        public OptionTargetList FindOptions<TOption>() where TOption : OptionAttribute
        {
            var methods = TypeFinder.FindMethodsWithAttribute<TOption>(Flags);
            var properties = TypeFinder.FindPropertiesWithAttribute<TOption>(Flags);
            var fields = TypeFinder.FindFieldsWithAttribute<TOption>(Flags);

            foreach (var item in methods)
            {
                var attr = item.GetCustomAttribute<TOption>();
                this[OptionsManager.FormatKeyString(attr.Name ?? item.Name)].Add(new MethodTarget(item)
                {
                    HasDefaultValue = attr.HasDefaultValue,
                    DefaultValue = attr.DefaultValue,
                });
            }

            foreach (var item in properties)
            {
                var attr = item.GetCustomAttribute<TOption>();
                this[OptionsManager.FormatKeyString(attr.Name ?? item.Name)].Add(new PropertyTarget(item)
                {
                    HasDefaultValue = attr.HasDefaultValue,
                    DefaultValue = attr.DefaultValue,
                });
            }

            foreach (var item in fields)
            {
                var attr = item.GetCustomAttribute<TOption>();
                this[OptionsManager.FormatKeyString(attr.Name ?? item.Name)].Add(new FieldTarget(item)
                {
                    HasDefaultValue = attr.HasDefaultValue,
                    DefaultValue = attr.DefaultValue,
                });
            }

            return this;
        }

        public bool TryGetValue(qRegisteredObjects registeredObjects, string name, out object value)
        {
            name = OptionsManager.FormatKeyString(name);
            value = null;

            var items = this[name];

            foreach (var item in items)
            {
                try
                {
                    if (!item.CanGetValue) continue;

                    if (item.IsStatic)
                    {
                        value = item.GetValue(null);
                        return true;
                    }

                    var targets = registeredObjects.Where(x => x?.GetType() == item.DeclaringType);

                    foreach (var obj in targets)
                    {
                        var val = item.GetValue(obj);
                        value = val;
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }

        public bool TryGetDefalutValue(string name, out object value)
        {
            name = OptionsManager.FormatKeyString(name);
            value = null;

            var items = this[name];

            foreach (var item in items)
            {
                if (!item.HasDefaultValue) continue;

                value = item.DefaultValue;
                return true;
            }

            return false;
        }

        public void Set(qRegisteredObjects registeredObjects, string name, object value)
        {
            name = OptionsManager.FormatKeyString(name);

            var items = this[name];

            var args = new ChangeOptionArgs()
            {
                value = value,
            };

            foreach (var item in items)
            {
                if (item.IsStatic)
                {
                    try
                    {
                        item.SetValue(null, args);
                    }
                    catch { }

                    continue;
                }

                var targets = registeredObjects.Where(x => x?.GetType() == item.DeclaringType);

                foreach (var obj in targets)
                {
                    try
                    {
                        item.SetValue(obj, args);
                    }
                    catch { }
                }
            }
        }

        public IEnumerator<KeyValuePair<string, Target>> GetEnumerator() =>
            Targets
            .SelectMany(x => x.Value.Select(y => new KeyValuePair<string, Target>(x.Key, y)))
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public abstract class Target
        {
            public abstract bool IsStatic { get; }
            public abstract Type ValueType { get; }
            public abstract Type DeclaringType { get; }

            public bool HasDefaultValue { get; set; }
            public object DefaultValue { get; set; }
            
            public abstract void SetValue(object obj, ChangeOptionArgs args);

            public virtual bool CanGetValue => true;

            public virtual object GetValue(object obj) =>
                null;
        }
        
        public class MethodTarget : Target
        {
            public MethodTarget(MethodInfo method)
            {
                Method = method;
                parameters = method.GetParameters();
            }

            public MethodInfo Method { get; private set; }

            ParameterInfo[] parameters;

            public override bool IsStatic => Method.IsStatic;
            public override bool CanGetValue => false;

            public override Type ValueType
            {
                get
                {
                    if (parameters.Length == 0)
                        return null;

                    if (parameters.Length == 1)
                        return parameters[0].ParameterType;

                    return parameters[1].ParameterType;
                }
            }

            public override Type DeclaringType => Method.DeclaringType;

            public override void SetValue(object obj, ChangeOptionArgs args)
            {
                switch (parameters.Length)
                {
                    case 0:
                        Method.Invoke(obj, parameters);
                        break;
                    case 1:
                        Method.Invoke(obj, new object[] { parameters[0].ParameterType == typeof(ChangeOptionArgs) ?
                            args :
                            args.value });
                        break;
                    case 2:
                        Method.Invoke(obj, new object[]
                        {
                            args,
                            args.value,
                        });
                        break;
                }
            }
        }

        public class PropertyTarget : Target
        {
            public PropertyTarget(PropertyInfo property)
            {
                Property = property;
            }

            public PropertyInfo Property { get; private set; }

            public override bool IsStatic => Property.GetAccessors(nonPublic: true)[0].IsStatic;
            public override bool CanGetValue => Property.CanRead;

            public override Type ValueType => Property.PropertyType;
            public override Type DeclaringType => Property.DeclaringType;

            public override object GetValue(object obj) =>
                Property.GetValue(obj);

            public override void SetValue(object obj, ChangeOptionArgs args) =>
                Property.SetValue(obj, args.value);
        }

        public class FieldTarget : Target
        {
            public FieldTarget(FieldInfo field)
            {
                Field = field;
            }

            public FieldInfo Field { get; private set; }

            public override bool IsStatic => Field.IsStatic;

            public override Type ValueType => Field.FieldType;
            public override Type DeclaringType => Field.DeclaringType;

            public override object GetValue(object obj) =>
                Field.GetValue(obj);

            public override void SetValue(object obj, ChangeOptionArgs args) =>
                Field.SetValue(obj, args.value);
        }
    }
}
