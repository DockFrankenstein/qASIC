using System.Reflection;

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
        public static IEnumerable<Type> FindAllClassesWithAttribute<T>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where T : Attribute =>
            FindAllClassesWithAttribute(typeof(T), bindingFlags);

        public static IEnumerable<Type> FindAllClassesWithAttribute(Type type, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .Where(x => x.GetCustomAttributes(type, false).Count() > 0);

        //Methods
        public static IEnumerable<MethodInfo> FindMethodsAttributes<T>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where T : Attribute =>
            FindMethodsAttributes(typeof(T), bindingFlags);

        public static IEnumerable<MethodInfo> FindMethodsAttributes(Type type, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods(bindingFlags))
                .Where(x => x.GetCustomAttributes(type, false).FirstOrDefault() != null);

        //Fields
        public static IEnumerable<FieldInfo> FindFieldAttributesInClass<TClass, TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where TClass : class
            where TAttribute : Attribute =>
            FindFieldAttributesInClass(typeof(TClass), typeof(TAttribute), bindingFlags);

        public static IEnumerable<FieldInfo> FindFieldAttributesInClass(Type classType, Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            classType.GetFields(bindingFlags)
                .Where(x => x.GetCustomAttributes(attributeType, false).FirstOrDefault() != null);

        public static IEnumerable<FieldInfo> FindFieldAttributes<TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            FindFieldAttributes(typeof(TAttribute), bindingFlags);

        public static IEnumerable<FieldInfo> FindFieldAttributes(Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => FindFieldAttributesInClass(x, attributeType, bindingFlags));

        //Properties
        public static IEnumerable<PropertyInfo> FindPropertyAttributesInClass<TClass, TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS)
            where TClass : class
            where TAttribute : Attribute =>
                FindPropertyAttributesInClass(typeof(TClass), typeof(TAttribute), bindingFlags);

        public static IEnumerable<PropertyInfo> FindPropertyAttributesInClass(Type classType, Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            classType.GetProperties(bindingFlags)
                .Where(x => x.GetCustomAttributes(attributeType, false).FirstOrDefault() != null);

        public static IEnumerable<PropertyInfo> FindPropertyAttributes<TAttribute>(BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            FindPropertyAttributes(typeof(TAttribute), bindingFlags);

        public static IEnumerable<PropertyInfo> FindPropertyAttributes(Type attributeType, BindingFlags bindingFlags = _DEFAULT_FLAGS) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => FindPropertyAttributesInClass(x, attributeType, bindingFlags));
        #endregion

        public static object? CreateConstructorFromType(Type type) =>
            CreateConstructorFromType(type, null);

        public static object? CreateConstructorFromType(Type type, params object[]? parameters)
        {
            if (type == null)
                return null;

            ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null || constructor.IsAbstract) return null;
            return constructor.Invoke(parameters);
        }

        public static IEnumerable<T?> CreateConstructorsFromTypes<T>(IEnumerable<Type> types) =>
            types.SelectMany(x =>
            {
                if (x == null)
                    return new T?[] { default };

                ConstructorInfo? constructor = x.GetConstructor(Type.EmptyTypes);
                if (constructor == null || constructor.IsAbstract) return new T[0];
                return new T[] { (T)constructor.Invoke(null) };
            });
    }
}
