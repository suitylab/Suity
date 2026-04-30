using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Initializes linked asset activators for document formats at editor startup.
/// </summary>
internal sealed class LinkedAssetInitializer
{
    /// <summary>
    /// Singleton instance of the linked asset initializer.
    /// </summary>
    public static readonly LinkedAssetInitializer Instance = new();

    private bool _init;

    private readonly Dictionary<Type, LinkedAssetActivator> _activatorsByDocType = [];

    private LinkedAssetInitializer()
    {
        EditorRexes.EditorStart.AddActionListener(Initialize);
    }

    /// <summary>
    /// Initializes all linked asset activators by scanning document formats and registering them.
    /// </summary>
    private void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        EditorServices.SystemLog.AddLog("SAssetInitializer Initializing...");
        EditorServices.SystemLog.PushIndent();

        AssetActivatorManager.Instance.RegisterAssetActivator(SAssetActivator.Instance);

        // Due to the Alias mechanism introduced by DocumentFormat, formats may have duplicates
        // Here we need to deduplicate
        foreach (var factory in DocumentManager.Instance.GetDocumentFormats().Distinct())
        {
            var activator = new LinkedAssetActivator(factory);
            AssetActivatorManager.Instance.RegisterAssetActivator(activator);

            Type docType = factory.DocumentType;
            if (docType != null)
            {
                _activatorsByDocType[docType] = activator;
            }
        }

        foreach (Type docType in typeof(AssetDocument).GetDerivedTypes())
        {
            DocumentFormatAttribute docTypeAttr = docType.GetAttributeCached<DocumentFormatAttribute>();
            if (docTypeAttr is null)
            {
                continue;
            }
            if (string.IsNullOrEmpty(docTypeAttr.FormatName))
            {
                continue;
            }

            LinkedAssetActivator activator = _activatorsByDocType.GetValueSafe(docType);
            if (activator is null)
            {
                activator = new LinkedAssetActivator(new AttributedDocumentFormat(docType, docTypeAttr));

                foreach (var extension in docTypeAttr.Extensions)
                {
                    if (!string.IsNullOrEmpty(extension))
                    {
                        activator.AddExtension(extension);
                    }
                }
                AssetActivatorManager.Instance.RegisterAssetActivator(activator);
            }

            var nonStaticAttr = docType.GetAttributeCached<InstanceAssetAccessAttribute>();
            if (nonStaticAttr != null)
            {
                activator.InstanceMode = AssetInstanceMode.Instance;
            }
        }

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("LinkedAssetInitializer Initialized.");
    }
}

/// <summary>
/// Asset activator that creates assets from linked documents.
/// </summary>
internal class LinkedAssetActivator : AssetActivator
{
    private readonly DocumentFormat _documentFormat;

    private List<string> _exts;

    private bool? _isIdDocumented;

    /// <summary>
    /// Creates an empty linked asset activator.
    /// </summary>
    internal LinkedAssetActivator()
    {
    }

    /// <summary>
    /// Creates a linked asset activator for a specific document format.
    /// </summary>
    /// <param name="documentFormat">The document format to activate.</param>
    public LinkedAssetActivator(DocumentFormat documentFormat)
    {
        _documentFormat = documentFormat ?? throw new ArgumentNullException(nameof(documentFormat));
    }

    /// <inheritdoc/>
    public override Asset CreateAsset(string fileName, string assetKey)
    {
        var document = DocumentManager.Instance.OpenDocument(fileName);
        if (document is null)
        {
            return null;
        }

        if (document.State != DocumentState.Loaded)
        {
            Logs.LogWarning($"Document load failed in asset creation pipeline : {fileName}");
            return null;
        }

        var builder = (document.Content as AssetDocument)
            ?.AssetBuilder
            ?.WithLocalName(assetKey);

        if (builder != null)
        {
            EditorServices.FileAssetManager.LockedWithFileAsset(builder, StorageLocation.Create(fileName));
            var asset = builder.LockedResolveId();
            if (asset != null)
            {
                asset.ShowStorageProperty = document.Format?.CanShowAsProperty == true;
            }

            return asset;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override string[] GetExtensions()
    {
        return _documentFormat?.GetAdditionalExtensions() ?? _exts?.ToArray() ?? [];
    }
    
    /// <inheritdoc/>
    public override LoadingIterations Iteration => _documentFormat?.Iteration ?? LoadingIterations.Iteration1;

    /// <inheritdoc/>
    public override bool IsIdDocumented
    {
        get
        {
            if (_isIdDocumented is { } v)
            {
                return v;
            }

            var docType = _documentFormat?.DocumentType;
            if (docType is null)
            {
                return false;
            }

            v = typeof(SAssetDocument).IsAssignableFrom(docType);

            return v;
        }
    }

    /// <inheritdoc/>
    public override bool IsAttached => _documentFormat?.IsAttached ?? false;

    /// <summary>
    /// Adds a file extension supported by this activator.
    /// </summary>
    /// <param name="extension">The file extension to add.</param>
    internal void AddExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return;
        }

        (_exts ??= []).Add(extension);
    }
}

/// <summary>
/// Specialized activator for .sasset document files.
/// </summary>
internal sealed class SAssetActivator : LinkedAssetActivator
{
    /// <summary>
    /// Singleton instance of the SAsset activator.
    /// </summary>
    public static readonly SAssetActivator Instance = new();

    private SAssetActivator()
    {
        AddExtension("sasset");
    }

    /// <inheritdoc/>
    public override bool RequireInFileResolve => true;

    /// <inheritdoc/>
    public override bool IsIdDocumented => true;
}
