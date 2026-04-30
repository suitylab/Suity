using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Services;

/// <summary>
/// Manages asset activators that create assets from files based on their extensions.
/// </summary>
public sealed class AssetActivatorManager
{
    /// <summary>
    /// Singleton instance of the asset activator manager.
    /// </summary>
    public static readonly AssetActivatorManager Instance = new();

    private readonly Dictionary<string, AssetActivator> _activators = [];

    private bool _init;

    private readonly HashSet<string> _attachedExts = [Asset.MetaExtension];

    private AssetActivatorManager()
    {
        EditorRexes.EditorAwake.AddActionListener(Initialize);
    }

    /// <summary>
    /// Initializes the manager by scanning and registering all derived AssetActivator types.
    /// </summary>
    private void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        EditorServices.SystemLog.AddLog("AssetActivatorManager Initializing...");
        EditorServices.SystemLog.PushIndent();

        ScanForActivatorType();

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("AssetActivatorManager Initialized.");
    }

    /// <summary>
    /// Scans for all derived AssetActivator types and registers them.
    /// </summary>
    private void ScanForActivatorType()
    {
        foreach (Type activatorType in typeof(AssetActivator).GetDerivedTypes())
        {
            AssetActivator activator = (AssetActivator)activatorType.CreateInstanceOf();
            RegisterAssetActivator(activator);
        }
    }


    /// <summary>
    /// Gets the collection of file extensions that have attached assets.
    /// </summary>
    public IEnumerable<string> AttachedAssetExtensions => _attachedExts.Pass();

    /// <summary>
    /// Registers an asset activator with its supported file extensions.
    /// </summary>
    /// <param name="activator">The activator to register.</param>
    public void RegisterAssetActivator(AssetActivator activator)
    {
        if (activator is null)
        {
            throw new ArgumentNullException();
        }

        var exts = activator.GetExtensions();
        EditorServices.SystemLog.AddLog($"Register asset activator : {activator.GetType().Name} ext : {string.Join(",", exts)}");

        foreach (var ext in exts)
        {
            if (!_activators.ContainsKey(ext))
            {
                _activators.Add(ext, activator);
            }
            else
            {
                Logs.LogError($"Asset activator extension conflict : {ext}");
            }

            if (activator.IsAttached)
            {
                string extx = "." + ext.Trim('.');
                _attachedExts.Add(extx);
            }
        }

        activator._initialized = true;
    }

    /// <summary>
    /// Gets the asset activator for a given file extension.
    /// </summary>
    /// <param name="ext">The file extension.</param>
    /// <returns>The matching activator, or null if not found.</returns>
    [Obsolete("After adding the SAsset document format judgment mechanism, passing only ext cannot determine AssetActivator")]
    public AssetActivator GetAssetActivator(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext))
        {
            return null;
        }

        ext = ext.ToLowerInvariant().TrimStart('.');

        if (_activators.TryGetValue(ext, out AssetActivator activator))
        {
            return activator;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if a file name represents an asset file based on its extension.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file is a recognized asset file.</returns>
    public bool IsAssetFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;

        string ext = Path.GetExtension(fileName).TrimStart('.');

        return _activators.ContainsKey(ext);
    }
}
