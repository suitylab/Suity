using Suity.Synchonizing;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Suity.Views.Named;

/// <summary>
/// Provides extension methods for working with named items, nodes, and collections.
/// </summary>
public static class NamedExtensions
{
    /// <summary>
    /// Creates a synchronized named list for the specified value type.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the list, must be a class implementing <see cref="ISyncObject"/>.</typeparam>
    /// <param name="nameField">The field name used for identifying items.</param>
    /// <returns>A new <see cref="INamedSyncList{TValue}"/> instance.</returns>
    public static INamedSyncList<TValue> CreateNamedSyncList<TValue>(string nameField) where TValue : class, ISyncObject
        => NamedExternal._external.CreateNamedSyncList<TValue>(nameField);

    /// <summary>
    /// Safely gets an item at the specified index from a named node, returning null if out of range.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The <see cref="NamedItem"/> at the index, or null if out of range.</returns>
    public static NamedItem GetItemAtSafe(this INamedNode node, int index)
    {
        if (index >= 0 && index < node.Count)
        {
            return node.GetItemAt(index);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Safely gets a field at the specified index from a field list, returning null if out of range.
    /// </summary>
    /// <param name="list">The field list.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The <see cref="NamedField"/> at the index, or null if out of range.</returns>
    public static NamedField GetItemAtSafe(this NamedFieldList list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Safely gets an item at the specified index from a root collection, returning null if out of range.
    /// </summary>
    /// <param name="list">The root collection.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The <see cref="NamedItem"/> at the index, or null if out of range.</returns>
    public static NamedItem GetItemAtSafe(this NamedRootCollection list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            return list.GetItemAt(index);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Safely gets an item at the specified index from a named sync list, returning null if out of range.
    /// </summary>
    /// <typeparam name="TValue">The type of items in the list.</typeparam>
    /// <param name="list">The named sync list.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The item at the index, or null if out of range.</returns>
    public static TValue GetItemAtSafe<TValue>(this INamedSyncList<TValue> list, int index)
        where TValue : class, ISyncObject
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a group by name from the direct children of a named node.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="groupName">The name of the group to find.</param>
    /// <returns>The first matching <see cref="NamedGroup"/>, or null if not found.</returns>
    public static NamedGroup GetGroup(this INamedNode node, string groupName)
        => node.Items.OfType<NamedGroup>().FirstOrDefault(x => x.GroupName == groupName);

    /// <summary>
    /// Gets a group by traversing a path of group names from a named node.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="groupPath">The array of group names representing the path.</param>
    /// <returns>The <see cref="NamedGroup"/> at the end of the path, or null if not found.</returns>
    public static NamedGroup GetGroup(this INamedNode node, string[] groupPath)
    {
        if (node is null)
        {
            return null;
        }

        if (groupPath is null || groupPath.Length == 0)
        {
            return null;
        }

        return groupPath.Aggregate(node, (current, name) => current.GetGroup(name)) as NamedGroup;

    }

    /// <summary>
    /// Gets all groups with the specified name from a named node.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="groupName">The name of the groups to find.</param>
    /// <returns>An array of matching <see cref="NamedGroup"/> instances.</returns>
    [Obsolete]
    public static NamedGroup[] GetGroups(this INamedNode node, string groupName)
        => node.Items.OfType<NamedGroup>().Where(x => x.GroupName == groupName).ToArray();

    /// <summary>
    /// Ensures a group path exists, creating groups as needed using the specified type.
    /// </summary>
    /// <typeparam name="TGroup">The type of group to create, must extend <see cref="NamedGroup"/> and have a parameterless constructor.</typeparam>
    /// <param name="node">The named node.</param>
    /// <param name="groupPath">The path of groups to ensure.</param>
    /// <returns>The <see cref="INamedNode"/> at the end of the path.</returns>
    [Obsolete]
    public static INamedNode EnsureGroup<TGroup>(this INamedNode node, string groupPath)
        where TGroup : NamedGroup, new()
    {
        return EnsureGroupByPath(node, groupPath, () => new TGroup());
    }

    /// <summary>
    /// Gets a node by traversing a group path string.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="groupPath">The path string, using '/' or space as separators.</param>
    /// <returns>The <see cref="INamedNode"/> at the end of the path, or null if any group in the path is not found.</returns>
    public static INamedNode GetGroupByPath(this INamedNode node, string groupPath)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return node;
        }

        var groups = groupPath.Split(['/', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (groups.Length == 0)
        {
            return node;
        }

        for (int j = 0; j < groups.Length; j++)
        {
            string groupName = groups[j];
            var group = node.GetGroup(groupName);
            if (group is null)
            {
                return null;
            }
            node = group;
        }

        return node;
    }

    /// <summary>
    /// Ensures a group path exists, creating groups as needed using the specified factory.
    /// </summary>
    /// <param name="node">The named node.</param>
    /// <param name="groupPath">The path string, using '/' or space as separators.</param>
    /// <param name="factory">A factory function to create new groups.</param>
    /// <returns>The <see cref="INamedNode"/> at the end of the path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when node is null.</exception>
    /// <exception cref="NullReferenceException">Thrown when factory returns null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the group is already in a list.</exception>
    public static INamedNode EnsureGroupByPath(this INamedNode node, string groupPath, Func<NamedGroup> factory)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (string.IsNullOrWhiteSpace(groupPath))
        {
            return node;
        }

        var groups = groupPath.Split(['/', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (groups.Length == 0)
        {
            return node;
        }

        for (int j = 0; j < groups.Length; j++)
        {
            string groupName = groups[j];
            var group = node.GetGroup(groupName);
            if (group is null)
            {
                group = factory();
                if (group is null)
                {
                    throw new NullReferenceException("factory return null.");
                }
                if (group.ParentList is not null)
                {
                    throw new InvalidOperationException("group already in a list.");
                }
                
                group.GroupName = groupName;
                node.AddItem(group);
            }
            node = group;
        }

        return node;
    }
}
