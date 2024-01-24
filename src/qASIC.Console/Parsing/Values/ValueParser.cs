namespace qASIC.Console.Parsing.Values
{
    public abstract class ValueParser
    {
        public ValueParser() { }

        public abstract Type ValueType { get; }

        public abstract bool TryParse(string s, out object? result);
    }

    public abstract class ValueParser<T> : ValueParser
    {
        public override Type ValueType => typeof(T);

        public override bool TryParse(string s, out object? result)
        {
            var value = TryParse(s, out T? parseResult);
            result = parseResult;
            return value;
        }

        public abstract bool TryParse(string s, out T? result);
    }
}