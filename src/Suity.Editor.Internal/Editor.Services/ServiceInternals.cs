using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Views.Named;
using Suity.Views.PathTree;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Marks a class as having internal initialization priority.
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class InternalPriorityAttribute : Attribute
{
}

/// <summary>
/// Provides access to internal editor service initialization and licensing.
/// </summary>
public static class ServiceInternals
{
    /// <summary>
    /// The internal license service used for capability checks.
    /// </summary>
    internal static LicenseService _license = EmptyLicenseService.Empty;

    private static bool _init;

    /// <summary>
    /// Initializes all internal editor subsystems. This method is idempotent.
    /// </summary>
    public static void InitializeInternalSystems()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        EditorObjectManagerBK.Instance.Initialize();
        AssetManagerBK.Instance.Initialize();
        DTypeManagerBK.Instance.Initialize();
        ValueManagerBK.Instance.Initialize();
        SItemExternalBK.Instance.Initialize();
        SValueExternalBK.Instance.Initialize();
        NamedExternalBK.Instance.Initialize();
        WorkSpacesExternalBK.Instance.Initialize();
        SyncExportExternalBK.Instance.Initialize();
        TypesExternalBK.Instance.Intialize();

        DocumentManagerBK.Instance.Initialize();
        ReferenceManagerBK.Instance.Initialize();
        AnalysisServiceBK.Instance.Initialize();

        PathNodeCollection._factory = node => new PathNodeCollectionBK(node);
        RootDirectoryNode._exFactory = node => new RootDirectoryNodeExBK(node);
    }

    /// <summary>
    /// Gets the internal license service.
    /// </summary>
    public static LicenseService License => _license;
}

/// <summary>
/// Interface for components that participate in internal editor initialization.
/// </summary>
public interface IInternalEditorInitialize
{
    /// <summary>
    /// Gets the priority of this initializer. Lower values are initialized first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Performs initialization logic for this component.
    /// </summary>
    void Initialize();
}
