using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for describing and identifying types.
/// </summary>
public static class TypeDescribeHelper
{
    /// <summary>
    /// Returns the short version of an Assembly name.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetShortAssemblyName(this Assembly assembly)
    {
        return assembly.FullName.Split(',')[0];
    }

    /// <summary>
    /// Returns the short version of an Assembly name.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static string GetShortAssemblyName(this AssemblyName assemblyName)
    {
        return assemblyName.FullName.Split(',')[0];
    }

    /// <summary>
    /// Returns the short version of an Assembly name.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static string GetShortAssemblyName(string assemblyName)
    {
        return assemblyName.Split(',')[0];
    }

    /// <summary>
    /// Returns a Types keyword, its "short" name. Just the types "base", no generic
    /// type parameters or array specifications.
    /// </summary>
    /// <param name="T">The Type to describe</param>
    /// <returns></returns>
    public static string GetTypeKeyword(this Type T)
    {
        return T.Name.Split(['`'], StringSplitOptions.RemoveEmptyEntries)[0].Replace('+', '.');
    }

    /// <summary>
    /// Returns a string describing a certain Type.
    /// </summary>
    /// <param name="T">The Type to describe</param>
    /// <returns></returns>
    public static string GetTypeCSCodeName(this Type T, bool shortName = false)
    {
        var typeStr = new StringBuilder();

        if (T.IsGenericParameter)
        {
            return T.Name;
        }

        if (T.IsArray)
        {
            typeStr.Append(GetTypeCSCodeName(T.GetElementType(), shortName));
            typeStr.Append('[');
            typeStr.Append(',', T.GetArrayRank() - 1);
            typeStr.Append(']');
        }
        else
        {
            Type[] genArgs = T.IsGenericType ? T.GetGenericArguments() : null;

            if (T.IsNested)
            {
                Type declType = T.DeclaringType;
                if (declType.IsGenericTypeDefinition)
                {
                    Array.Resize(ref genArgs, declType.GetGenericArguments().Length);
                    declType = declType.MakeGenericType(genArgs);
                    genArgs = T.GetGenericArguments().Skip(genArgs.Length).ToArray();
                }

                string parentName = GetTypeCSCodeName(declType, shortName);

                string[] nestedNameToken = shortName ? T.Name.Split('+') : T.FullName.Split('+');
                string nestedName = nestedNameToken[nestedNameToken.Length - 1];

                int genTypeSepIndex = nestedName.IndexOf("[[", StringComparison.Ordinal);
                if (genTypeSepIndex != -1) nestedName = nestedName.Substring(0, genTypeSepIndex);
                genTypeSepIndex = nestedName.IndexOf('`');
                if (genTypeSepIndex != -1) nestedName = nestedName.Substring(0, genTypeSepIndex);

                typeStr.Append(parentName);
                typeStr.Append('.');
                typeStr.Append(nestedName);
            }
            else
            {
                if (shortName)
                {
                    typeStr.Append(T.Name.Split(['`'], StringSplitOptions.RemoveEmptyEntries)[0].Replace('+', '.'));
                }
                else
                {
                    typeStr.Append(T.FullName.Split(['`'], StringSplitOptions.RemoveEmptyEntries)[0].Replace('+', '.'));
                }
            }

            if (genArgs?.Length > 0)
            {
                if (T.IsGenericTypeDefinition)
                {
                    typeStr.Append('<');
                    typeStr.Append(',', genArgs.Length - 1);
                    typeStr.Append('>');
                }
                else if (T.IsGenericType)
                {
                    typeStr.Append('<');
                    for (int i = 0; i < genArgs.Length; i++)
                    {
                        typeStr.Append(GetTypeCSCodeName(genArgs[i], shortName));
                        if (i < genArgs.Length - 1)
                            typeStr.Append(',');
                    }
                    typeStr.Append('>');
                }
            }
        }

        return typeStr.Replace('+', '.').ToString();
    }

    /// <summary>
    /// Returns a string describing a certain Type.
    /// </summary>
    /// <param name="T">The Type to describe</param>
    /// <returns></returns>
    public static string GetTypeId(this Type T)
    {
        return T.FullName != null ? Regex.Replace(T.FullName, @"(, [^\]\[]*)", "") : T.Name;
    }

    /// <summary>
    /// Returns a string describing a certain Member of a Type.
    /// </summary>
    /// <param name="member">The Member to describe.</param>
    /// <returns></returns>
    public static string GetMemberId(this MemberInfo member)
    {
        if (member is Type) return "T:" + GetTypeId(member as Type);
        string declType = member.DeclaringType.GetTypeId();

        FieldInfo field = member as FieldInfo;
        if (field != null) return "F:" + declType + ':' + field.Name;

        EventInfo ev = member as EventInfo;
        if (ev != null) return "E:" + declType + ':' + ev.Name;

        PropertyInfo property = member as PropertyInfo;
        if (property != null)
        {
            ParameterInfo[] parameters = property.GetIndexParameters();
            if (parameters.Length == 0)
                return "P:" + declType + ':' + property.Name;
            else
                return "P:" + declType + ':' + property.Name + '(' + parameters.ToString(p => p.ParameterType.GetTypeId(), ",") + ')';
        }

        MethodInfo method = member as MethodInfo;
        if (method != null)
        {
            ParameterInfo[] parameters = method.GetParameters();
            Type[] genArgs = method.GetGenericArguments();

            string result = "M:" + declType + ':' + method.Name;

            if (genArgs.Length != 0)
            {
                if (method.IsGenericMethodDefinition)
                    result += "``" + genArgs.Length.ToString(CultureInfo.InvariantCulture);
                else
                    result += "``" + genArgs.Length.ToString(CultureInfo.InvariantCulture) + '[' + genArgs.ToString(t => "[" + t.GetTypeId() + "]", ",") + ']';
            }
            if (parameters.Length != 0)
                result += '(' + parameters.ToString(p => p.ParameterType.GetTypeId(), ",") + ')';

            return result;
        }

        ConstructorInfo ctor = member as ConstructorInfo;
        if (ctor != null)
        {
            ParameterInfo[] parameters = ctor.GetParameters();

            string result = "C:" + declType + ':' + (ctor.IsStatic ? "s" : "i");

            if (parameters.Length != 0)
                result += '(' + parameters.ToString(p => p.ParameterType.GetTypeId(), ", ") + ')';

            return result;
        }

        throw new NotSupportedException(string.Format("Member Type '{0} not supported", member.GetType().GetTypeCSCodeName(true)));
    }

    public static bool GetIsNumeric(this Type type)
    {
        return Type.GetTypeCode(type).GetIsNumeric();
    }

    public static bool GetIsNumeric(this TypeCode typeCode) => typeCode switch
    {
        TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => true,
        _ => false,
    };

    public static bool GetIsInteger(this TypeCode typeCode) => typeCode switch
    {
        TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => true,
        _ => false,
    };

    public static bool GetHasSign(this TypeCode typeCode) => typeCode switch
    {
        TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => true,
        _ => false,
    };
}