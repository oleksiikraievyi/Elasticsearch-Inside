namespace ElasticsearchInside.CommandLine
{
    public class YesNoParameter
    {
        private readonly bool _value;

        public YesNoParameter(bool value)
        {
            _value = value;
        }

        public static implicit operator YesNoParameter(bool value)
        {
            return new YesNoParameter(value);
        }

        public override string ToString()
        {
            return _value ? "yes" : "no";
        }
    }
}
