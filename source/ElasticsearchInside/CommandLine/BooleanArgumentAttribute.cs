using System;

namespace ElasticsearchInside.CommandLine
{
    internal class BooleanArgumentAttribute : Attribute
    {
        private readonly string _format;
        private readonly bool _defaultValue;

        public BooleanArgumentAttribute(string format, bool defaultValue = false)
        {
            _format = format;
            _defaultValue = defaultValue;                             
        }

        public string Format
        {
            get { return _format; }
        }

        public bool DefaultValue
        {
            get { return _defaultValue; }
        }
    }
}