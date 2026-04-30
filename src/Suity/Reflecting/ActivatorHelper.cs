using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Suity.Reflecting;

/// <summary>
/// Provides helper methods for creating instances of types.
/// </summary>
public static class ActivatorHelper
{
    private delegate object ObjectActivator();

    private static readonly ObjectActivator nullObjectActivator = () => null;
    private static readonly Dictionary<Type, ObjectActivator> createInstanceMethodCache = [];

    /// <summary>
    /// Creates an instance of a Type. Attempts to use the Types default empty constructor, but will
    /// return an uninitialized object in case no constructor is available.
    /// </summary>
    /// <param name="type">The Type to create an instance of.</param>
    /// <returns>An instance of the Type. Null, if instanciation wasn't possible.</returns>
    // This feature is not available on il2cpp
    //[System.Diagnostics.DebuggerStepThrough]
    public static object CreateInstanceOf(this Type type)
    {
        ObjectActivator activator = null;
        lock (createInstanceMethodCache)
        {
            if (createInstanceMethodCache.TryGetValue(type, out activator))
            {
                return activator();
            }
        }

        // Filter out non-instantiatable Types
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
        {
            activator = nullObjectActivator;
        }
        // If the caller wants a string, just return an empty one
        else if (type == typeof(string))
        {
            activator = () => "";
        }
        // If the caller wants an array, create an empty one
        else if (typeof(Array).IsAssignableFrom(type) && type.GetArrayRank() == 1)
        {
            activator = () => Array.CreateInstance(type.GetElementType(), 0);
        }
        else
        {
            try
            {
                // Attempt to invoke the Type default empty constructor
                ConstructorInfo emptyConstructor = type.GetConstructor(BindingFlagsPreset.BindInstanceAll, null, Type.EmptyTypes, null);
                if (emptyConstructor != null)
                {
                    var constructorLambda = Expression.Lambda<ObjectActivator>(Expression.New(emptyConstructor));
                    activator = constructorLambda.Compile();
                }
                // If there is no such constructor available, provide an uninitialized object
                else
                {
                    activator = () => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                }
            }
            catch (Exception)
            {
                activator = nullObjectActivator;
            }
        }

        // Test whether our activation method really works, and mind it for later
        object firstResult;

        try
        {
            firstResult = activator();
        }
        catch (Exception)
        {
            //// If we fail to initialize the Type due to a problem in its static constructor, it's likely a user problem. Let him know.
            //if (e is TypeInitializationException)
            //{
            //    Logs.LogError(
            //        string.Format("Failed to initialize Type {0}: {1}",
            //        TypeInfoString.Type(type),
            //        TypeInfoString.Exception(e.InnerException)));
            //}

            activator = nullObjectActivator;
            firstResult = null;

            throw;
        }

        lock (createInstanceMethodCache) 
        {
            createInstanceMethodCache[type] = activator;
        }

        return firstResult;
    }
}