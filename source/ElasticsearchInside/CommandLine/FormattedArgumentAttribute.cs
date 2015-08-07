using System;

namespace ElasticsearchInside.CommandLine
{
    public class FormattedArgumentAttribute : Attribute
    {
        private readonly string _argumentName;
        private readonly object _defaultValue;

        public FormattedArgumentAttribute(string argumentName, object defaultValue = null)
        {
            _argumentName = argumentName;
            _defaultValue = defaultValue;
        }

        public string ArgumentName
        {
            get { return _argumentName; }
        }

        public object DefaultValue
        {
            get { return _defaultValue; }
        }
    }
}