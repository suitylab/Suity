using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using Suity.Helpers;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Flows;

/// <summary>
/// Manages connector alias mappings for flow nodes, resolving legacy names and resource IDs.
/// </summary>
internal static class FlowConnectorAliasManager
{
    /// <summary>
    /// Stores alias mappings for a specific flow node type.
    /// </summary>
    private class FlowNodeTypeItem
    {
        /// <summary>
        /// Dictionary mapping alias names to real connector names.
        /// </summary>
        public Dictionary<string, string> Alias { get; } = [];
    }

    private static readonly Dictionary<Type, FlowNodeTypeItem> _types = [];

    private static bool _init;

    /// <summary>
    /// Initializes the alias manager by scanning all flow node types for <see cref="FlowConnectorAliasAttribute"/>.
    /// </summary>
    internal static void Initialize()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        foreach (var type in typeof(FlowNode).GetDerivedTypes())
        {
            if (type.IsAbstract)
            {
                continue;
            }

            foreach (var attr in type.GetAttributesCached<FlowConnectorAliasAttribute>())
            {
                AddAlias(type, attr.AliasName, attr.RealName);
            }
        }
    }

    /// <summary>
    /// Adds an alias mapping for a specific flow node type.
    /// </summary>
    /// <param name="type">The flow node type.</param>
    /// <param name="aliasName">The alias name for the connector.</param>
    /// <param name="name">The real connector name.</param>
    internal static void AddAlias(Type type, string aliasName, string name)
    {
        if (string.IsNullOrWhiteSpace(aliasName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var item = _types.GetOrAdd(type, t => new FlowNodeTypeItem());

        item.Alias[aliasName] = name;
    }

    /// <summary>
    /// Resolves a connector name from an alias, DField, or global ID.
    /// </summary>
    /// <param name="type">The flow node type.</param>
    /// <param name="aliasName">The alias or reference name to resolve.</param>
    /// <param name="renamed">True if the name was resolved via alias mapping.</param>
    /// <param name="resolved">True if the name was resolved via DField or global ID lookup.</param>
    /// <returns>The resolved connector name, or null if not found.</returns>
    internal static string ResolveName(Type type, string aliasName, out bool renamed, out bool resolved)
    {
        renamed = false;
        resolved = false;

        if (_types.TryGetValue(type, out var item))
        {
            string originName = item.Alias.GetValueSafe(aliasName);
            if (!string.IsNullOrWhiteSpace(originName))
            {
                renamed = true;

                return originName;
            }
        }

        // Resolve DField
        var dField = DTypeManager.Instance.ResolveField(aliasName);
        if (dField != null)
        {
            resolved = true;
            return dField.Id.ToString();
        }

        // Handle resource import issue
        if (GlobalIdResolver.TryResolve(aliasName, out Guid id))
        {
            resolved = true;

            return id.ToString();
        }

        return null;
    }

    /// <summary>
    /// Validates and fixes all links in a flow document by resolving connector aliases and detecting missing nodes/connectors.
    /// </summary>
    /// <param name="doc">The flow document to check.</param>
    internal static void CheckDocument(FlowDocument doc, DocumentLoadingIntent intent = DocumentLoadingIntent.Normal)
    {
        if (doc is null || doc.Entry is null)
        {
            return;
        }

        if (doc.Entry.State != DocumentState.Loaded)
        {
            return;
        }

        if (Project.Current.Status == ProjectStatus.Starting)
        {
            // Skipping scanning links during project startup to avoid issues with resource loading order. Links will be checked again after the project is fully opened.
            return;
        }

        if (intent != DocumentLoadingIntent.Normal)
        {
            // Skipping scanning links for non-normal loading intents (e.g., project startup, import, reload) to avoid issues with resource loading order. Links will be checked again after the project is fully opened or reloaded.
            return;
        }


        foreach (var link in doc.Links.Links)
        {
            CheckLink(doc, link);
        }
    }

    private static void CheckLink(FlowDocument doc, NodeLink link)
    {
        var fromNode = doc.GetFlowNode(link.FromNode);
        if (fromNode != null)
        {
            fromNode.FlushQueuedConnection();

            var connector = fromNode.GetConnector(link.FromConnector, false);
            if (connector is null)
            {
                var originName = ResolveName(fromNode.GetType(), link.FromConnector, out bool renamed, out bool resolved);
                if (originName != null)
                {
                    string oldName = link.FromConnector;
                    link.FromConnector = originName;

                    if (renamed)
                    {
                        Logs.LogInfo($"Connector renamed for node {fromNode.Name}: {oldName} -> {originName} doc:{doc.FileName}");
                    }
                    else if (resolved)
                    {
                        // Path -> ResourceId
                    }

                    // Try to get again
                    connector = fromNode.GetConnector(link.FromConnector, false);
                    if (connector is null)
                    {
                        doc.LogWarning($"Connector missing for node {fromNode.Name}: {link.FromConnector} doc:{doc.FileName}");
                    }
                }
                else
                {
                    doc.LogWarning($"Connector missing for node {fromNode.Name}: {link.FromConnector} doc:{doc.FileName}");
                }
            }
        }
        else
        {
            doc.LogError($"Node missing for connection line: {link.FromNode} doc:{doc.FileName}");
        }

        var toNode = doc.GetFlowNode(link.ToNode);
        if (toNode != null)
        {
            toNode.FlushQueuedConnection();

            var connector = toNode.GetConnector(link.ToConnector, false);
            if (connector is null)
            {
                var originName = ResolveName(toNode.GetType(), link.ToConnector, out bool renamed, out bool resolved);
                if (originName != null)
                {
                    string oldName = link.ToConnector;
                    link.ToConnector = originName;

                    if (renamed)
                    {
                        Logs.LogInfo($"Connector renamed for node {toNode.Name}: {oldName} -> {originName} doc:{doc.FileName}");
                    }
                    else if (resolved)
                    {
                        // Path -> ResourceId
                    }

                    // Try to get again
                    connector = toNode.GetConnector(link.ToConnector, false);
                    if (connector is null)
                    {
                        doc.LogWarning($"Connector missing for node {toNode.Name}: {link.ToConnector} doc:{doc.FileName}");
                    }
                }
                else
                {
                    doc.LogWarning($"Connector missing for node {toNode.Name}: {link.ToConnector} doc:{doc.FileName}");
                }
            }
        }
        else
        {
            doc.LogError($"Node missing for connection line: {link.ToNode}");
        }
    }
}