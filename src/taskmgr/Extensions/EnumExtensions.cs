using System.Reflection;
using Task.Manager.Gui.Controls;

namespace Task.Manager.Extensions;

public static class EnumExtensions
{
    private static T GetCustomAttribute<T>(FieldInfo fieldInfo) where T : Attribute
    {
        var attribute = fieldInfo.GetCustomAttribute<T>();

        if (attribute is null) {
            throw new InvalidOperationException($"Field {fieldInfo.Name} has no {nameof(Attribute)}");
        }
        
        return attribute;
    }
    
    private static FieldInfo GetFieldInfo(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null) {
            throw new InvalidOperationException($"Cannot get field {value} from type {value.GetType()}");
        }
        
        return fieldInfo;
    }
    
    public static string GetProperty(this Enum value)
    {
        var fieldInfo = GetFieldInfo(value);
        var attribute = GetCustomAttribute<ProcessControl.ColumnPropertyAttribute>(fieldInfo);
        return attribute.Property;
    }

    public static string GetTitle(this Enum value)
    {
        var fieldInfo = GetFieldInfo(value);
        var attribute = GetCustomAttribute<ProcessControl.ColumnTitleAttribute>(fieldInfo);
        return attribute.Title;
    }
}
