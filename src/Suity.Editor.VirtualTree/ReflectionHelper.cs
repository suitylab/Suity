using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Provides helper methods for reflection-based operations in the virtual tree.
/// </summary>
internal static class ReflectionHelper
{
    /// <summary>
    /// Creates an instance of the specified type.
    /// </summary>
    /// <param name="instanceType">The type to instantiate.</param>
    /// <param name="noConstructor">Whether to skip calling the constructor.</param>
    /// <returns>The created instance, or null if creation failed.</returns>
    public static object CreateInstanceOf(this Type instanceType, bool noConstructor = false)
    {
        try
        {
            if (instanceType == typeof(string))
                return "";
            else if (typeof(Array).IsAssignableFrom(instanceType) && instanceType.GetArrayRank() == 1)
                return Array.CreateInstance(instanceType.GetElementType(), 0);
            else if (noConstructor)
                return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(instanceType);
            else
                return Activator.CreateInstance(instanceType, true);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the default instance of a Type. Equals <c>default(T)</c>, but works for Reflection.
    /// </summary>
    /// <param name="instanceType">The Type to create a default instance of.</param>
    /// <returns></returns>
    public static object GetDefaultInstanceOf(this Type instanceType)
    {
        if (instanceType.IsValueType)
            return Activator.CreateInstance(instanceType, true);
        else
            return null;
    }

    /// <summary>
    /// Checks whether a type derives from a specified base type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="baseType">The potential base type.</param>
    /// <returns>True if the type derives from the base type, false otherwise.</returns>
    public static bool IsDerivedFrom(this Type type, Type baseType)
    {
        do
        {
            if (type.BaseType == baseType)
                return true;

            type = type.BaseType;
        } while (type != null);

        return false;
    }

    /// <summary>
    /// Finds the most specific common base class among a collection of types.
    /// </summary>
    /// <param name="types">The collection of types to analyze.</param>
    /// <returns>The common base class, or null if none found.</returns>
    public static Type GetCommonBaseClass(this IEnumerable<Type> types)
    {
        Type commonBase = null;
        foreach (Type type in types)
        {
            if (commonBase is null)
            {
                commonBase = type;
                continue;
            }
            while (commonBase != null && !commonBase.IsAssignableFrom(type))
            {
                commonBase = commonBase.BaseType ?? typeof(object);
                if (commonBase == typeof(object)) return commonBase;
            }
        }

        return commonBase;
    }

    /// <summary>
    /// Gets the C# code representation of a type name.
    /// </summary>
    /// <param name="T">The type to get the name for.</param>
    /// <param name="shortName">Whether to return a short name without namespace.</param>
    /// <returns>The C# code representation of the type name.</returns>
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
                string nestedName = nestedNameToken[^1];

                int genTypeSepIndex = nestedName.IndexOf("[[");
                if (genTypeSepIndex != -1) nestedName = nestedName[..genTypeSepIndex];
                genTypeSepIndex = nestedName.IndexOf('`');
                if (genTypeSepIndex != -1) nestedName = nestedName[..genTypeSepIndex];

                typeStr.Append(parentName);
                typeStr.Append('.');
                typeStr.Append(nestedName);
            }
            else
            {
                if (shortName)
                    typeStr.Append(T.Name.Split(['`'], StringSplitOptions.RemoveEmptyEntries)[0].Replace('+', '.'));
                else
                    typeStr.Append(T.FullName.Split(['`'], StringSplitOptions.RemoveEmptyEntries)[0].Replace('+', '.'));
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
    /// Finds all concrete types that derive from the specified abstract type across all loaded assemblies.
    /// </summary>
    /// <param name="abstractType">The abstract or base type to search for.</param>
    /// <returns>An array of concrete types that inherit from or implement the abstract type.</returns>
    public static Type[] FindConcreteTypes(Type abstractType)
    {
        return AppDomain.CurrentDomain.GetAssemblies().
            Where(a => !a.IsDynamic).
            SelectMany(a => 
            {
                try
                {
                    return a.GetExportedTypes();
                }
                catch (Exception err)
                {
                    err.LogError($"Get assembly types failed : {a.GetShortAssemblyName()}");

                    return [];
                }                    
            }).
            Where(t => !t.IsAbstract && !t.IsInterface && abstractType.IsAssignableFrom(t)).
            ToArray();
    }

    /// <summary>
    /// Gets the element type of an IList or array type.
    /// </summary>
    /// <param name="listType">The list or array type.</param>
    /// <returns>The element type, or object if not determinable.</returns>
    public static Type GetIListElementType(Type listType)
    {
        Type ilistInterface = null;
        if (listType.HasElementType)
            return listType.GetElementType();
        else if ((ilistInterface = listType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))) != null)
            return ilistInterface.GetGenericArguments()[0];
        else if (listType.IsGenericType)
            return listType.GetGenericArguments()[0];
        else
            return typeof(object);
    }

    /// <summary>
    /// Sets a property value on multiple target objects, iterating through values.
    /// </summary>
    /// <param name="property">The property to set.</param>
    /// <param name="targetObjects">The target objects to set the property on.</param>
    /// <param name="values">The values to assign.</param>
    /// <param name="index">The index parameter for indexed properties.</param>
    public static void DefaultPropertySetter(PropertyInfo property, IEnumerable<object> targetObjects, IEnumerable<object> values, int index)
    {
        IEnumerator<object> valuesEnum = values.GetEnumerator();
        object curValue = null;

        if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
        foreach (object target in targetObjects)
        {
            if (target != null) property.SetValue(target, curValue, [index]);
            if (valuesEnum.MoveNext()) curValue = valuesEnum.Current;
        }
    }

    /// <summary>
    /// Determines the dynamic type based on the actual types of values.
    /// </summary>
    /// <param name="staticType">The static fallback type.</param>
    /// <param name="values">The values to determine types from.</param>
    /// <returns>The most specific shared type.</returns>
    public static Type ReflectDynamicType(Type staticType, IEnumerable<object> values)
    {
        return ReflectDynamicType(staticType, values.Where(v => v != null).Select(v => v.GetType()));
    }

    /// <summary>
    /// Determines the most specific shared Type of all the specified Types.
    /// The specified static Type will be used as a shared fallback, if no other
    /// common root is found.
    /// </summary>
    /// <param name="staticType">The static fallback type.</param>
    /// <param name="dynamicTypes">The collection of dynamic types to analyze.</param>
    /// <returns>The most specific shared type.</returns>
    public static Type ReflectDynamicType(Type staticType, IEnumerable<Type> dynamicTypes)
    {
        if (staticType.IsSealed) return staticType;
        if (!staticType.IsClass && !staticType.IsInterface) return staticType;

        Type commonBaseType = dynamicTypes.GetCommonBaseClass();
        if (staticType.IsDerivedFrom(commonBaseType) || (staticType.IsInterface && commonBaseType == typeof(object)))
            return staticType;
        else
            return commonBaseType;
    }

    /// <summary>
    /// Determines the most specific shared Type between a static type and a common base type.
    /// </summary>
    /// <param name="staticType">The static fallback type.</param>
    /// <param name="commonBaseType">The common base type to compare against.</param>
    /// <returns>The most appropriate type to use.</returns>
    public static Type ReflectDynamicType(Type staticType, Type commonBaseType)
    {
        if (staticType.IsSealed) return staticType;
        if (!staticType.IsClass && !staticType.IsInterface) return staticType;

        if (staticType.IsDerivedFrom(commonBaseType) || (staticType.IsInterface && commonBaseType == typeof(object)))
            return staticType;
        else
            return commonBaseType;
    }
}