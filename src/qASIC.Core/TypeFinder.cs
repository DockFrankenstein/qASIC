using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qASIC
{
    public static class TypeFinder
    {
        private const BindingFlags _DEFAULT_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        #region Types
        public static IEnumerable<Type> FindAllTypes<T>()
        {
            var type = typeof(T);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t));
        }
        #endregion

        #region Attributes
        //Classes
        public static IEnumerable<Type> FindClassesWithAttribute<T>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where T : Attribute =>
            FindClassesWithAttribute(typeof(T), bindingFlags);

        public static IEnumerable<Type> FindClassesWithAttribute(Type type, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .Where(x => x.GetCustomAttributes(type, false).Count() > 0);

        //Methods
        public static IEnumerable<MethodInfo> FindMethodsWithAttributeInClass<TClass, TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where TClass : class
            where TAttribute : Attribute =>
            FindMethodsWithAttributeInClass(typeof(TClass), typeof(TAttribute), bindingFlags);

        public static IEnumerable<MethodInfo> FindMethodsWithAttributeInClass(Type classType, Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            classType.GetMethods(bindingFlags)
                .Where(x => x.GetCustomAttributes(attributeType, false).Count() > 0);

        public static IEnumerable<MethodInfo> FindMethodsWithAttribute<T>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where T : Attribute =>
            FindMethodsWithAttribute(typeof(T), bindingFlags);

        public static IEnumerable<MethodInfo> FindMethodsWithAttribute(Type type, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods(bindingFlags))
                .Where(x => x.GetCustomAttributes(type, false).Count() > 0);

        //Fields
        public static IEnumerable<FieldInfo> FindFieldsWithAttributeInClass<TClass, TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where TClass : class
            where TAttribute : Attribute =>
            FindFieldsWithAttributeInClass(typeof(TClass), typeof(TAttribute), bindingFlags);

        public static IEnumerable<FieldInfo> FindFieldsWithAttributeInClass(Type classType, Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            classType.GetFields(bindingFlags)
                .Where(x => x.GetCustomAttributes(attributeType, false).Count() > 0);

        public static IEnumerable<FieldInfo> FindFieldsWithAttribute<TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            FindFieldsWithAttribute(typeof(TAttribute), bindingFlags);

        public static IEnumerable<FieldInfo> FindFieldsWithAttribute(Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => FindFieldsWithAttributeInClass(x, attributeType, bindingFlags));

        //Properties
        public static IEnumerable<PropertyInfo> FindPropertiesWithAttributeInClass<TClass, TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where TClass : class
            where TAttribute : Attribute =>
                FindPropertiesWithAttributeInClass(typeof(TClass), typeof(TAttribute), bindingFlags);

        public static IEnumerable<PropertyInfo> FindPropertiesWithAttributeInClass(Type classType, Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            classType.GetProperties(bindingFlags)
                .Where(x => x.GetCustomAttributes(attributeType, false).Count() > 0);

        public static IEnumerable<PropertyInfo> FindPropertiesWithAttribute<TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            FindPropertiesWithAttribute(typeof(TAttribute), bindingFlags);

        public static IEnumerable<PropertyInfo> FindPropertiesWithAttribute(Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => FindPropertiesWithAttributeInClass(x, attributeType, bindingFlags));
        #endregion

        public static object CreateConstructorFromType(Type type) =>
            CreateConstructorFromType(type, null);

        public static object CreateConstructorFromType(Type type, params object[] parameters)
        {
            if (type == null)
                return null;

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null || constructor.IsAbstract) return null;
            return constructor.Invoke(parameters);
        }

        public static IEnumerable<T> CreateConstructorsFromTypes<T>(IEnumerable<Type> types) =>
            types.SelectMany(x =>
            {
                if (x == null)
                    return new T[] { default };

                ConstructorInfo constructor = x.GetConstructor(Type.EmptyTypes);
                if (constructor == null || constructor.IsAbstract) return new T[0];
                return new T[] { (T)constructor.Invoke(null) };
            });
    }
}
