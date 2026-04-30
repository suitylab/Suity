using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor;

/// <summary>
/// Internal singleton implementation of <see cref="StorageManager"/> that handles storage provider
/// registration, file existence checks, and storage item retrieval for both physical files
/// and custom storage providers (paths starting with "//").
/// </summary>
internal class StorageManagerBK : StorageManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="StorageManagerBK"/>.
    /// </summary>
    public static readonly StorageManagerBK Instance = new();

    private readonly Dictionary<string, IStorageProvider> _providers = [];
    private bool _init;

    private StorageManagerBK()
    {
        StorageManager.Current = this;
        EditorRexes.EditorBeforeAwake.AddActionListener(Initialize);
    }

    /// <summary>
    /// Initializes the storage manager by scanning and registering all available storage providers.
    /// </summary>
    private void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        ScanForStorageProvider();
    }

    /// <summary>
    /// Scans for all types implementing <see cref="IStorageProvider"/> and registers them.
    /// </summary>
    private void ScanForStorageProvider()
    {
        foreach (Type resolverType in typeof(IStorageProvider).GetDerivedTypes())
        {
            try
            {
                IStorageProvider resolver = (IStorageProvider)resolverType.CreateInstanceOf();
                RegisterStorageProvider(resolver);
            }
            catch (Exception err)
            {
                err.LogError($"Failed to create IDocumentStreamResolver : {resolverType.FullName}");
            }
        }
    }

    /// <summary>
    /// Registers a storage provider by its name.
    /// </summary>
    /// <param name="provider">The storage provider to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provider name is missing or already registered.</exception>
    private void RegisterStorageProvider(IStorageProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (string.IsNullOrEmpty(provider.Name))
        {
            throw new InvalidOperationException($"{nameof(IStorageProvider)} name is missing.");
        }

        if (_providers.ContainsKey(provider.Name))
        {
            throw new InvalidOperationException($"{nameof(IStorageProvider)} name exists :{provider.Name}");
        }

        _providers.Add(provider.Name, provider);
    }

    /// <inheritdoc/>
    public override bool FileExists(string fullPath)
    {
        if (TryParseProvider(fullPath, out string providerName, out string location))
        {
            return GetProvider(providerName)?.Exists(location) == true;
        }
        else
        {
            return File.Exists(fullPath);
        }
    }

    /// <inheritdoc/>
    public override bool FileExists(StorageLocation location)
    {
        if (location is null)
        {
            return false;
        }

        if (location.StorageType != null)
        {
            return GetProvider(location.StorageType)?.Exists(location.Location) == true;
        }
        else if (location.PhysicFileName != null)
        {
            return File.Exists(location.PhysicFileName);
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override IStorageItem GetStorageItem(string fullPath)
    {
        if (TryParseProvider(fullPath, out string name, out string location))
        {
            var provider = GetProvider(name)
                ?? throw new NullReferenceException($"{nameof(IStorageProvider)} not found : {name}");

            return provider.GetStorageItem(location);
        }
        else if (File.Exists(fullPath))
        {
            return new FileStorageItem(fullPath);
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override IStorageItem GetStorageItem(StorageLocation location)
    {
        if (location is null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        if (!string.IsNullOrEmpty(location.StorageType))
        {
            var provider = GetProvider(location.StorageType)
                ?? throw new NullReferenceException($"{nameof(IStorageProvider)} not found : {location.StorageType}");

            return provider.GetStorageItem(location.Location);
        }
        else if (!string.IsNullOrEmpty(location.PhysicFileName))
        {
            return new FileStorageItem(location.PhysicFileName);
        }
        else
        {
            throw new InvalidOperationException($"Get stream failed : {location}");
        }
    }

    /// <summary>
    /// Parses a custom storage provider path (e.g., "//providerName/location") into its provider name and location components.
    /// </summary>
    /// <param name="path">The path to parse.</param>
    /// <param name="providerName">When this method returns, contains the provider name if parsing succeeded; otherwise, null.</param>
    /// <param name="location">When this method returns, contains the location within the provider if parsing succeeded; otherwise, null.</param>
    /// <returns>True if the path was successfully parsed as a custom provider path; otherwise, false.</returns>
    public override bool TryParseProvider(string path, out string providerName, out string location)
    {
        providerName = null;
        location = null;

        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!path.StartsWith("//"))
        {
            return false;
        }

        path = path.RemoveFromFirst("//");
        int index = path.IndexOf('/');
        if (index <= 0)
        {
            return false;
        }

        providerName = path[..index];
        location = path.RemoveFromFirst(providerName.Length + 1);

        return true;
    }

    /// <inheritdoc/>
    public override IStorageProvider GetProvider(string name)
    {
        return _providers.GetValueSafe(name);
    }

    /// <inheritdoc/>
    public override bool IsInCustomStorage(string path)
    {
        return path?.StartsWith("//") == true;
    }
}