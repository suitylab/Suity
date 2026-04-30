using Suity.Reflecting;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for working with enums.
/// </summary>
public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Enum, string> _keys = new();

    [Obsolete]
    public static string ToKey(this Enum value)
    {
        return _keys.GetOrAdd(value, s => $"{s.GetType().FullName}.{s}");
    }

    public static string ToDataId(this Enum value)
    {
        return _keys.GetOrAdd(value, s => $"{s.GetType().FullName}.{s}");
    }

    public static string ToDisplayText(this Enum value)
    {
        Type enumType = value.GetType();
        FieldInfo fieldInfo = enumType.GetField(value.ToString());

        var attr = fieldInfo?.GetAttributeCached<DisplayTextAttribute>();

        return attr?.DisplayText ?? value.ToString();
    }

    public static bool TryParseEnumValue(this string key, out Enum value)
    {
        do
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                break;
            }

            int index = key.LastIndexOf('.');
            if (index < 0)
            {
                break;
            }

            string enumTypeName = key.Substring(0, index);
            string enumName = key.Substring(++index);

            Type enumType = enumTypeName.ResolveType();
            if (enumType is null || !enumType.IsEnum)
            {
                break;
            }
            
            try
            {
                var obj = Enum.Parse(enumType, enumName, false);
                value = (Enum)obj;
                return true;
            }
            catch (Exception)
            {
                break;
            }
        } while (false);

        value = default;

        return false;
    }
}