using System.ComponentModel;
using System.Reflection;

namespace Task.Manager.Cli.Utils;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null) {
            return value.ToString();
        }
        
        DescriptionAttribute? attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
        if (attribute == null) {
            return value.ToString();
        }
        
        return attribute.Description;
    }
}
