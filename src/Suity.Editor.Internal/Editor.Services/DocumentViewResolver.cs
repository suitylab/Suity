using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Resolves document view types based on document type mappings and attributes.
/// </summary>
internal sealed class DocumentViewResolver
{
    /// <summary>
    /// Singleton instance of the document view resolver.
    /// </summary>
    public static readonly DocumentViewResolver Instance = new();

    private static readonly UniqueMultiDictionary<Type, Type> _viewTypes = new();
    private static readonly UniqueMultiDictionary<Type, Type> _defaultViewTypes = new();
    private static bool _init;

    private DocumentViewResolver()
    {
        EditorRexes.EditorAwake.AddActionListener(Initialize);
    }

    /// <summary>
    /// Initializes the resolver by scanning all IDocumentView implementations and registering them.
    /// </summary>
    private void Initialize()
    {
        EditorServices.SystemLog.AddLog($"{nameof(DocumentViewResolver)} Initializing...");
        EditorServices.SystemLog.PushIndent();

        _viewTypes.Clear();

        // Inspect all IDocumentView implementations and find suitable views.
        foreach (Type viewType in typeof(IDocumentView).GetDerivedTypes())
        {
            var attrs = viewType.GetAttributesCached<DocumentViewUsageAttribute>().ToArray();

            foreach (var attr in attrs) 
            {
                if (attr.DocumentType != null && typeof(Document).IsAssignableFrom(attr.DocumentType))
                {
                    if (viewType.HasAttributeCached<DefaultDocumentFormatAttribute>())
                    {
                        EditorServices.SystemLog.AddLog($"Add default document view type : {viewType.Name} for {attr.DocumentType.Name}");
                        _defaultViewTypes.Add(attr.DocumentType, viewType);
                    }
                    else
                    {
                        EditorServices.SystemLog.AddLog($"Add document view type : {viewType.Name} for {attr.DocumentType.Name}");
                        _viewTypes.Add(attr.DocumentType, viewType);
                    }
                }
                else if (!string.IsNullOrEmpty(attr.FormatName))
                {
                    var factory = DocumentManager.Instance.GetDocumentFormat(attr.FormatName);
                    if (factory?.DocumentType != null && typeof(Document).IsAssignableFrom(factory.DocumentType))
                    {
                        if (viewType.HasAttributeCached<DefaultDocumentFormatAttribute>())
                        {
                            EditorServices.SystemLog.AddLog($"Add default document view type : {viewType.Name} for {attr.DocumentType.Name}");
                            _defaultViewTypes.Add(attr.DocumentType, viewType);
                        }
                        else
                        {
                            EditorServices.SystemLog.AddLog($"Add document view type : {viewType.Name} for {attr.DocumentType.Name}");
                            _viewTypes.Add(attr.DocumentType, viewType);
                        }
                    }
                }
            }
        }

        _init = true;

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"{nameof(DocumentViewResolver)} Initialized.");
    }

    /// <summary>
    /// Creates a view instance for the specified document type.
    /// </summary>
    /// <param name="objectType">The document type to create a view for.</param>
    /// <returns>The created view, or null if no matching view type is found.</returns>
    internal IDocumentView CreateView(Type objectType)
    {
        if (!_init)
        {
            Initialize();
        }

        Type viewType = ResolveViewType(objectType);

        if (viewType != null)
        {
            return (IDocumentView)viewType.CreateInstanceOf();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves the view type for a given document type, walking up the inheritance chain.
    /// </summary>
    /// <param name="objectType">The document type.</param>
    /// <param name="resolveDefault">Whether to fall back to default view types if no specific match is found.</param>
    /// <returns>The resolved view type, or null.</returns>
    public Type ResolveViewType(Type objectType, bool resolveDefault = true)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = objectType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_viewTypes[cType]);
            if (viewType != null)
            {
                return viewType;
            }

            cType = cType.BaseType;
        }

        if (resolveDefault)
        {
            return ResolveDefaultViewType(objectType);
        }

        return null;
    }

    /// <summary>
    /// Resolves the default view type for a given document type.
    /// </summary>
    /// <param name="objectType">The document type.</param>
    /// <returns>The default view type, or null.</returns>
    public Type ResolveDefaultViewType(Type objectType)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = objectType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_defaultViewTypes[cType]);
            if (viewType != null)
            {
                return viewType;
            }

            cType = cType.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Resolves a single view type from a collection, preferring types marked with RequestOverrideAttribute.
    /// </summary>
    /// <param name="types">The candidate view types.</param>
    /// <returns>The selected view type.</returns>
    private Type ResolveMultipleType(IEnumerable<Type> types)
    {
        if (types.CountOne())
        {
            return types.First();
        }

        Type overrideType = types.FirstOrDefault(o => o.HasAttributeCached<RequestOverrideAttribute>());
        return overrideType ?? types.FirstOrDefault();
    }

    /// <summary>
    /// Logs all registered view type mappings for debugging purposes.
    /// </summary>
    internal void LogViewTypes()
    {
        Logs.LogDebug("View types");
        foreach (var docType in _viewTypes.Keys)
        {
            Logs.LogDebug($"--Document type: {docType.Name}");
            foreach (var viewType in _viewTypes[docType])
            {
                Logs.LogDebug($"  --View type: {viewType.Name}");
            }
        }

        Logs.LogDebug("Default view types");
        foreach (var docType in _defaultViewTypes.Keys)
        {
            Logs.LogDebug($"--Document type: {docType.Name}");
            foreach (var viewType in _defaultViewTypes[docType])
            {
                Logs.LogDebug($"  --View type: {viewType.Name}");
            }
        }
    }
}
