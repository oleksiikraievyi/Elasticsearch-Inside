using System.Linq;
using System.Reflection;
using System.Text;

namespace ElasticsearchInside.CommandLine
{
    public class CommandLineBuilder
    {
        public string Build<T>(T entity)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public);

            var stringBuilder = new StringBuilder();

            foreach (var propertyInfo in properties)
            {
                var args = propertyInfo.GetCustomAttributes(typeof(FormattedArgumentAttribute), true).OfType<FormattedArgumentAttribute>().FirstOrDefault();

                if (args != null)
                {
                    var value = propertyInfo.GetValue(entity) ?? args.DefaultValue;
                    stringBuilder.AppendFormat(" " + args.ArgumentName, value);
                }

                var argumentAttribute = propertyInfo.GetCustomAttributes(typeof(BooleanArgumentAttribute), true).OfType<BooleanArgumentAttribute>().FirstOrDefault();

                if (argumentAttribute != null && (argumentAttribute.DefaultValue || ((bool)propertyInfo.GetValue(entity))))
                    stringBuilder.AppendFormat(" {0}", argumentAttribute.Format);

            }

            return stringBuilder.ToString();

        }
    }
}
