using System;

namespace qASIC.Parsing
{
    public abstract class ValueParser
    {
        public ValueParser() { }

        public static ValueParser[] CreateStandardParserArray() =>
            new ValueParser[]
            {
                new IntParser(),
                new UIntParser(),
                new FloatParser(),
                new DoubleParser(),
                new DecimalParser(),
                new LongParser(),
                new UlongParser(),
                new ByteParser(),
                new SByteParser(),
                new ShortParser(),
                new UShortParser(),
                new BoolParser(),
                new StringParser(),
            };

        public abstract Type ValueType { get; }

        public abstract bool TryParse(string s, out object result);
    }

    public abstract class ValueParser<T> : ValueParser
    {
        public override Type ValueType => typeof(T);

        public override bool TryParse(string s, out object result)
        {
            var value = TryParse(s, out T parseResult);
            result = parseResult;
            return value;
        }

        public abstract bool TryParse(string s, out T result);
    }
}