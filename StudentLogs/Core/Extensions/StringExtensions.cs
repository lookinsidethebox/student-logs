using Core.Attributes;

namespace Core.Extensions
{
    public static class StringExtensions
	{
        public static string GetStringValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attrs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attrs.Length > 0 ? attrs[0].StringValue : null;
        }
    }
}
