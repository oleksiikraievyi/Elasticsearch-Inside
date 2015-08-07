namespace ElasticsearchInside.CommandLine
{
    internal class OnOffParameter
    {
        private readonly bool _value;

        public OnOffParameter(bool value)
        {
            _value = value;
        }

        public static implicit operator OnOffParameter(bool value)
        {
            return new OnOffParameter(value);
        }

        public override string ToString()
        {
            return _value ? "on" : "off";
        }
    }
}
