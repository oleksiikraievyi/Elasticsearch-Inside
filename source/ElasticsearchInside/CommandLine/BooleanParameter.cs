namespace ElasticsearchInside.CommandLine
{
    internal class BooleanParameter
    {
        private readonly bool _value;

        public BooleanParameter(bool value)
        {
            _value = value;
        }

        public static implicit operator BooleanParameter(bool value)
        {
            return new BooleanParameter(value);
        }

        public override string ToString()
        {
            return _value ? "true" : "false";
        }
    }
}
