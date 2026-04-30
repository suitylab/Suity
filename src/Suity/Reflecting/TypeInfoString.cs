using Suity.Helpers;
using System;
using System.Linq;
using System.Reflection;

namespace Suity.Reflecting;

/// <summary>
/// Provides methods for creating string representations of type information.
/// </summary>
public static class TypeInfoString
{
    /// <summary>
    /// Retrieves the current stack frame.
    /// </summary>
    /// <param name="skipFrames">The number of frames to skip. This function itsself is omitted by default.</param>
    /// <returns>The caller's stack frame.</returns>
    public static System.Diagnostics.StackFrame CurrentStackFrame(int skipFrames = 0)
    {
        return new System.Diagnostics.StackTrace(skipFrames + 1).GetFrame(0);
    }

    /// <summary>
    /// Returns the name of the caller method.
    /// </summary>
    /// <param name="skipFrames">The number of frames to skip. This function itsself is omitted by default.</param>
    /// <param name="includeDeclaringType">If true, the methods declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string CurrentMethod(int skipFrames = 0, bool includeDeclaringType = true)
    {
        return MethodInfo(CurrentStackFrame(skipFrames + 1).GetMethod(), includeDeclaringType);
    }

    /// <summary>
    /// Returns the name of the caller methods declaring type.
    /// </summary>
    /// <param name="skipFrames">The number of frames to skip. This function itsself is omitted by default.</param>
    /// <returns></returns>
    public static string CurrentType(int skipFrames = 0)
    {
        return Type(CurrentStackFrame(skipFrames + 1).GetMethod().DeclaringType);
    }

    /// <summary>
    /// Returns a string that can be used for representing a <see cref="System.Reflection.Assembly"/> in log entries.
    /// </summary>
    /// <param name="asm"></param>
    /// <returns></returns>
    public static string Assembly(Assembly asm)
    {
        return asm.GetShortAssemblyName();
    }

    /// <summary>
    /// Returns a string that can be used for representing a <see cref="System.Type"/> in log entries.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string Type(Type type)
    {
        if (type != null)
        {
            return type.GetTypeCSCodeName(true);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns a string that can be used for representing a method in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the methods declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string MethodInfo(MethodInfo info, bool includeDeclaringType = true)
    {
        string declTypeName = Type(info.DeclaringType);
        string returnTypeName = Type(info.ReturnType);
        string[] paramNames = info.GetParameters().Select(p => Type(p.ParameterType)).ToArray();
        string[] genArgNames = null;

        try
        {
            genArgNames = info.GetGenericArguments().Select(Type).ToArray();
        }
        catch (Exception)
        {
            genArgNames = [];
        }

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{4} {0}{1}{3}({2})",
            includeDeclaringType ? declTypeName + "." : "",
            info.Name,
            paramNames.ToString(", "),
            genArgNames.Length > 0 ? "<" + genArgNames.ToString(", ") + ">" : "",
            returnTypeName);
    }

    /// <summary>
    /// Returns a string that can be used for representing a method or constructor in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the methods or constructors declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string MethodInfo(MethodBase info, bool includeDeclaringType = true)
    {
        if (info is MethodInfo)
            return MethodInfo(info as MethodInfo);
        else if (info is ConstructorInfo)
            return ConstructorInfo(info as ConstructorInfo);
        else if (info != null)
            return info.ToString();
        else
            return "null";
    }

    /// <summary>
    /// Returns a string that can be used for representing a constructor in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the constructors declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string ConstructorInfo(ConstructorInfo info, bool includeDeclaringType = true)
    {
        string declTypeName = Type(info.DeclaringType);
        string[] paramNames = info.GetParameters().Select(p => Type(p.ParameterType)).ToArray();

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0}{1}({2})",
            includeDeclaringType ? declTypeName + "." : "",
            info.DeclaringType.Name,
            paramNames.ToString(", "));
    }

    /// <summary>
    /// Returns a string that can be used for representing a property in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the properties declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string PropertyInfo(PropertyInfo info, bool includeDeclaringType = true)
    {
        string declTypeName = Type(info.DeclaringType);
        string propTypeName = Type(info.PropertyType);
        string[] paramNames = info.GetIndexParameters().Select(p => Type(p.ParameterType)).ToArray();

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} {1}{2}{3}",
            propTypeName,
            includeDeclaringType ? declTypeName + "." : "",
            info.Name,
            paramNames.Any() ? "[" + paramNames.ToString(", ") + "]" : "");
    }

    /// <summary>
    /// Returns a string that can be used for representing a field in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the fields declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string FieldInfo(FieldInfo info, bool includeDeclaringType = true)
    {
        string declTypeName = Type(info.DeclaringType);
        string fieldTypeName = Type(info.FieldType);

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} {1}{2}",
            fieldTypeName,
            includeDeclaringType ? declTypeName + "." : "",
            info.Name);
    }

    /// <summary>
    /// Returns a string that can be used for representing an event in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the events declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string EventInfo(EventInfo info, bool includeDeclaringType = true)
    {
        string declTypeName = Type(info.DeclaringType);
        string fieldTypeName = Type(info.EventHandlerType);

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} {1}{2}",
            fieldTypeName,
            includeDeclaringType ? declTypeName + "." : "",
            info.Name);
    }

    /// <summary>
    /// Returns a string that can be used for representing a(ny) member in log entries.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="includeDeclaringType">If true, the members declaring type is included in the returned name.</param>
    /// <returns></returns>
    public static string MemberInfo(MemberInfo info, bool includeDeclaringType = true)
    {
        if (info is MethodInfo)
            return MethodInfo(info as MethodInfo, includeDeclaringType);
        else if (info is ConstructorInfo)
            return ConstructorInfo(info as ConstructorInfo, includeDeclaringType);
        else if (info is PropertyInfo)
            return PropertyInfo(info as PropertyInfo, includeDeclaringType);
        else if (info is FieldInfo)
            return FieldInfo(info as FieldInfo, includeDeclaringType);
        else if (info is EventInfo)
            return EventInfo(info as EventInfo, includeDeclaringType);
        else if (info is Type)
            return Type(info as Type);
        else if (info != null)
            return info.ToString();
        else
            return "null";
    }

    /// <summary>
    /// Returns a string that can be used for representing an exception in log entries.
    /// It usually does not include the full call stack and is significantly shorter than
    /// an <see cref="System.Exception">Exceptions</see> ToString method.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static string Exception(Exception e, bool callStack = true)
    {
        if (e is null)
        {
            return null;
        }

        string eName = Type(e.GetType());
        string eSite = e.TargetSite != null ? MemberInfo(e.TargetSite) : null;

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0}{1}: {2}{4}CallStack:{4}{3}",
            eName,
            eSite != null ? " at " + eSite : "",
            e.Message,
            e.StackTrace,
            System.Environment.NewLine);
    }
}