namespace qASIC
{
    //For things that have to be initialized for an entire project that
    //wouldn't make sense to keep in qInstance
    public static class qEnviroment
    {
        public static bool Initialized { get; private set; }

        public static void Initialize()
        {
            //Initialization can only happen once
            if (Initialized)
                return;

            Initialized = true;

            var types = TypeFinder.FindAllTypes<qEnviromentInitializer>();
            var initializers = TypeFinder.CreateConstructorsFromTypes<qEnviromentInitializer>(types);

            foreach (var item in initializers)
                item?.Initialize();
        }
    }
}