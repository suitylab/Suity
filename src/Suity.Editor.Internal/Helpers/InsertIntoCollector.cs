using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Helpers;

/// <summary>
/// Collects types decorated with <see cref="InsertIntoAttribute"/> and organizes them by position.
/// Used for extensible plugin-style registration systems.
/// </summary>
public static class InsertIntoCollector
{
    /// <summary>
    /// Collects all types derived from <typeparamref name="T"/> that have an <see cref="InsertIntoAttribute"/> with a valid position.
    /// Creates instances and stores them in a dictionary keyed by position.
    /// </summary>
    /// <typeparam name="T">The base type to collect.</typeparam>
    /// <param name="toLower">If true, position keys are converted to lowercase. Default is false.</param>
    /// <returns>A dictionary mapping position strings to created instances of <typeparamref name="T"/>.</returns>
    public static Dictionary<string, T> Collect<T>(bool toLower = false)
    {
        Dictionary<string, T> dic = [];

        var types = typeof(T).GetDerivedTypes();
        foreach (var type in types)
        {
            var insertIntos = type.GetAttributesCached<InsertIntoAttribute>().Where(o => !string.IsNullOrEmpty(o.Position));
            foreach (var insertInto in insertIntos)
            {
                string pos = insertInto.Position;
                if (toLower)
                {
                    pos = pos.ToLowerInvariant();
                }

                if (dic.ContainsKey(insertInto.Position))
                {
                    Logs.LogWarning($"{typeof(T).GetTypeCSCodeName()} position conflit : {insertInto.Position}");
                    continue;
                }

                try
                {
                    T obj = (T)type.CreateInstanceOf();
                    if (obj == null)
                    {
                        Logs.LogError($"Create {typeof(T).GetTypeCSCodeName()} failed.");
                        continue;
                    }

                    dic.Add(insertInto.Position, obj);
                }
                catch (Exception err)
                {
                    err.LogError($"Create {typeof(T).GetTypeCSCodeName()} failed.");
                }
            }
        }

        return dic;
    }
}
