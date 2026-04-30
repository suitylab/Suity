using Suity.Collections;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Internal implementation of <see cref="WorkSpacesExternal"/> that scans and manages workspace controller types.
/// </summary>
internal class WorkSpacesExternalBK : WorkSpacesExternal
{
    /// <summary>
    /// The singleton instance of this class.
    /// </summary>
    public static readonly WorkSpacesExternalBK Instance = new();

    private readonly Dictionary<string, WorkSpaceControllerInfo> _ctrlInfos = [];
    private readonly Dictionary<Type, WorkSpaceControllerInfo> _ctrlInfosByType = [];

    private WorkSpacesExternalBK()
    {
    }

    /// <summary>
    /// Initializes the external workspace subsystem and registers a listener to scan controllers on editor awake.
    /// </summary>
    public void Initialize()
    {
        WorkSpacesExternal._external = this;

        EditorRexes.EditorAwake.AddActionListener(() => ScanControllers());
    }

    /// <summary>
    /// Scans all derived types of <see cref="WorkSpaceController"/> and registers their controller info.
    /// Skips abstract types and logs errors for controllers with empty names or duplicate registrations.
    /// </summary>
    private void ScanControllers()
    {
        foreach (Type type in typeof(WorkSpaceController).GetDerivedTypes())
        {
            if (type.IsAbstract)
            {
                continue;
            }

            WorkSpaceControllerUsageAttribute attr = type.GetAttributeCached<WorkSpaceControllerUsageAttribute>();
            WorkSpaceControllerInfo info;

            if (attr != null)
            {
                if (string.IsNullOrEmpty(attr.Name))
                {
                    //throw new NullReferenceException("WorkSpaceController key is emtpy");
                    Logs.LogError("WorkSpaceController key is emtpy : " + type.Name);
                    continue;
                }

                info = new WorkSpaceControllerInfo(type, attr);
            }
            else
            {
                info = new WorkSpaceControllerInfo(type);
            }

            if (_ctrlInfos.ContainsKey(info.Name))
            {
                Logs.LogError($"Add {type.FullName} failed, controller name exists : {info.Name}.");
                continue;
            }

            _ctrlInfos.Add(info.Name, info);
            _ctrlInfosByType.Add(type, info);
        }
    }

    /// <inheritdoc/>
    public override WorkSpaceControllerInfo[] ControllerInfos => _ctrlInfos.Values.ToArray();

    /// <inheritdoc/>
    public override WorkSpaceControllerInfo GetControllerInfo(string name) => _ctrlInfos.GetValueSafe(name);

    /// <inheritdoc/>
    public override WorkSpaceControllerInfo GetControllerInfo(Type type) => _ctrlInfosByType.GetValueSafe(type);

    /// <inheritdoc/>
    public override WorkSpaceControllerInfo GetControllerInfo<T>() => _ctrlInfosByType.GetValueSafe(typeof(T));
}