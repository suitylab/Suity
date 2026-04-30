using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Rex.VirtualDom;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Manages reference tracking between editor objects, supporting building, finding, redirecting, and collecting references.
/// </summary>
internal sealed class ReferenceManagerBK : ReferenceManager
{
    /// <summary>
    /// Singleton instance of the reference manager.
    /// </summary>
    public static ReferenceManagerBK Instance = new();

    private readonly Dictionary<IReferenceHost, ReferenceBuildSync> _referencers = [];
    private readonly HashSet<IReferenceHost> _dirtyReferencers = [];

    private bool _updating;

    /// <summary>
    /// Lock object for update operations.
    /// </summary>
    public object _updateLock = new();

    private bool _disabled;

    private ReferenceManagerBK()
    { }

    /// <summary>
    /// Initializes the reference manager and registers it as the current instance.
    /// </summary>
    public void Initialize()
    {
        ReferenceManager.Current = this;

        EditorRexes.ReferenceManagerDisabled.AsRexListener().Subscribe(v =>
        {
            _disabled = v;

            if (_disabled)
            {
                IReferenceHost[] hosts;
                lock (_referencers)
                {
                    hosts = [.. _referencers.Keys];
                    _referencers.Clear();
                }

                lock (_dirtyReferencers)
                {
                    foreach (var host in hosts)
                    {
                        _dirtyReferencers.Add(host);
                    }
                }
            }
        });

        EditorRexes.RaiseUpdateReference.AddActionListener(() =>
        {
            if (!_disabled)
            {
                EditorUtility.AddDelayedAction(RaiseUpdateDelayedAction.Instance);
            }
        });
    }

    /// <inheritdoc/>
    public override void MarkDirty(IReferenceHost host)
    {
        if (host is null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        lock (_dirtyReferencers)
        {
            _dirtyReferencers.Add(host);
        }

        EditorServices.SystemLog.AddLog($"Ref mark dirty : {host}");

        EditorUtility.AddDelayedAction(UpdateDelayedAction.Instance);
    }

    /// <inheritdoc/>
    public override void Remove(IReferenceHost host)
    {
        if (host is null)
        {
            return;
        }

        lock (_dirtyReferencers)
        {
            _dirtyReferencers.Remove(host);
        }

        ReferenceBuildSync collection = null;
        lock (_referencers)
        {
            collection = _referencers.RemoveAndGet(host);
        }

        if (collection != null)
        {
            foreach (var guid in collection.Ids)
            {
                EditorObjectManager.Instance.GetEntry(guid)?.RemoveListener(host);
            }
        }

        EditorServices.SystemLog.AddLog($"Ref remove : {host}");
    }

    /// <inheritdoc/>
    public override void Update()
    {
        if (_disabled)
        {
            return;
        }

        bool updated = false;

        lock (_updateLock)
        {
            if (_updating)
            {
                return;
            }
            _updating = true;
        }

        try
        {
            IReferenceHost[] buildHosts = null;

            lock (_dirtyReferencers)
            {
                if (_dirtyReferencers.Count > 0)
                {
                    buildHosts = [.. _dirtyReferencers];
                    _dirtyReferencers.Clear();
                }
            }

            if (buildHosts != null)
            {
                updated = true;

                foreach (var host in buildHosts)
                {
                    ReferenceBuildSync buildSync = null;
                    lock (_referencers)
                    {
                        buildSync = _referencers.RemoveAndGet(host);
                    }

                    if (buildSync != null)
                    {
                        foreach (var guid in buildSync.Ids)
                        {
                            EditorObjectManager.Instance.GetEntry(guid)?.RemoveListener(host);
                        }

                        buildSync.Clear();
                    }
                    else
                    {
                        buildSync = new ReferenceBuildSync(host);
                    }

                    try
                    {
                        host.ReferenceSync(SyncPath.Empty, buildSync);
                    }
                    catch (Exception err)
                    {
                        err.LogError();
                    }

                    foreach (var guid in buildSync.Ids)
                    {
                        if (guid == Guid.Empty)
                        {
                            continue;
                        }

                        EditorObjectManager.Instance.EnsureEntry(guid).AddListener(host);
                    }

                    lock (_referencers)
                    {
                        _referencers.Add(host, buildSync);
                    }

                    EditorServices.SystemLog.AddLog($"Ref update : {host}");
                }
            }
        }
        finally
        {
            lock (_updateLock)
            {
                _updating = false;
            }
        }

        if (updated)
        {
            EditorUtility.AddDelayedAction(RaiseUpdateDelayedAction.Instance);
        }
    }

    /// <inheritdoc/>
    public override bool IsDirty => _dirtyReferencers.Count > 0;

    /// <summary>
    /// Finds all reference paths to a given ID by traversing all reference hosts deeply.
    /// </summary>
    /// <param name="id">The target GUID to find references for.</param>
    /// <returns>Collection of sync path report items.</returns>
    public IEnumerable<SyncPathReportItem> FindReferenceDeep(Guid id)
    {
        if (_disabled)
        {
            yield break;
        }

        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(id);
        if (entry is null)
        {
            yield break;
        }

        var hosts = entry.GetReferenceHosts();

        foreach (var host in hosts)
        {
            object owner = host;

            if (host is SyncReferenceHost refHost)
            {
                owner = refHost.Target;
            }

            var sync = new ReferenceFindSync(owner, id);
            host.ReferenceSync(SyncPath.Empty, sync);

            foreach (var result in sync.Results)
            {
                yield return result;
            }
        }
    }

    /// <summary>
    /// Finds all reference paths to a given ID within a specific host.
    /// </summary>
    /// <param name="host">The reference host to search within.</param>
    /// <param name="id">The target GUID to find references for.</param>
    /// <returns>Collection of sync path report items.</returns>
    public IEnumerable<SyncPathReportItem> FindReferenceDeep(IReferenceHost host, Guid id)
    {
        if (_disabled)
        {
            yield break;
        }

        if (host is null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        object owner = host;

        if (host is SyncReferenceHost refHost)
        {
            owner = refHost.Target;
        }

        var sync = new ReferenceFindSync(owner, id);
        host.ReferenceSync(SyncPath.Empty, sync);

        foreach (var result in sync.Results)
        {
            yield return result;
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<SyncPathReportItem> FindReference(Guid id)
    {
        if (_disabled)
        {
            yield break;
        }

        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(id);
        if (entry is null)
        {
            yield break;
        }

        IEnumerable<ReferenceBuildSync> collections = null;

        lock (_referencers)
        {
            collections = entry.GetReferenceHosts().Select(host => _referencers.GetValueSafe(host)).SkipNull();
        }

        if (collections is null)
        {
            yield break;
        }

        foreach (var collection in collections)
        {
            var host = collection.Host;
            foreach (var item in collection.GetReferenceItems(id))
            {
                yield return new SyncPathReportItem(host, item.Path, null, item.Message);
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<SyncPathReportItem> FindReference(IReferenceHost host, Guid id)
    {
        if (_disabled)
        {
            yield break;
        }

        if (host is null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        Update();

        ReferenceBuildSync collection = null;

        lock (_referencers)
        {
            collection = _referencers.GetValueSafe(host);
        }

        if (collection is null)
        {
            yield break;
        }

        foreach (var item in collection.GetReferenceItems(id))
        {
            yield return new SyncPathReportItem(host, item.Path, null, item.Message);
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<IReferenceHost> FindReferenceHosts(Guid id)
    {
        if (_disabled)
        {
            return [];
        }

        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(id);
        if (entry is null)
        {
            return [];
        }

        IEnumerable<ReferenceBuildSync> collections = null;

        lock (_referencers)
        {
            collections = entry.GetReferenceHosts().Select(host => _referencers.GetValueSafe(host)).SkipNull();
        }

        if (collections != null)
        {
            return collections.Select(o => o.Host);
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public override int GetReferenceCount(Guid id)
    {
        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(id);
        if (entry is null)
        {
            return 0;
        }

        lock (_referencers)
        {
            var collections = entry.GetReferenceHosts().Select(host => _referencers.GetValueSafe(host)).SkipNull();

            return collections.Sum(o => o.GetReferenceItemCount(id));
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> GetDependencies(IReferenceHost host)
    {
        if (_disabled)
        {
            return [];
        }

        Update();

        lock (_referencers)
        {
            var refs = _referencers.GetValueSafe(host);
            return refs?.Ids ?? [];
        }
    }

    /// <inheritdoc/>
    public override ICollection<Guid> CollectDependencies(object obj)
    {
        if (_disabled)
        {
            return EmptyCollection<Guid>.Empty;
        }

        HashSet<Guid> guids = [];

        ReferenceCollectSync sync = new((path, id) =>
        {
            guids.Add(id);
        });

        try
        {
            Visitor.Visit<IReference>(obj, (referencer, pathContext) =>
            {
                referencer.ReferenceSync(pathContext.GetPath(), sync);
            });
        }
        catch (Exception err)
        {
            err.LogWarning("Error collecting dependencies for object: " + obj?.ToString() ?? "null");
        }

        return guids;
    }

    /// <inheritdoc/>
    public override IDictionary<Guid, ReferenceProblemTypes> CollectProblemDependencies(object obj)
    {
        if (_disabled)
        {
            return EmptyDictionary<Guid, ReferenceProblemTypes>.Empty;
        }

        Dictionary<Guid, ReferenceProblemTypes> problems = null;

        var sync = new ReferenceCollectSync((path, id) =>
        {
            if (id != Guid.Empty)
            {
                var entry = EditorObjectManager.Instance.GetEntry(id);
                var entryObj = entry?.Target;

                if (entryObj is null)
                {
                    problems ??= [];

                    problems[id] = ReferenceProblemTypes.Missing;
                }
                else if (entry?.IdConflict == true)
                {
                    problems ??= [];

                    problems[id] = ReferenceProblemTypes.Conflict;
                }
            }
        });


        try
        {
            Visitor.Visit<IReference>(obj, (referencer, pathContext) =>
            {
                referencer.ReferenceSync(pathContext.GetPath(), sync);
            });
        }
        catch (Exception err)
        {
            err.LogWarning("Error collecting problems for object: " + obj?.ToString() ?? "null");
        }


        return problems as IDictionary<Guid, ReferenceProblemTypes> ?? EmptyDictionary<Guid, ReferenceProblemTypes>.Empty;
    }

    /// <inheritdoc/>
    public override IEnumerable<SyncPathReportItem> RedirectReference(Guid oldId, Guid newId)
    {
        if (_disabled)
        {
            yield break;
        }

        if (oldId == newId)
        {
            yield break;
        }

        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(oldId);
        if (entry is null)
        {
            yield break;
        }

        var hosts = entry.GetReferenceHosts();

        List<IReferenceHost> dirtyHosts = [];
        List<SyncPathReportItem> results = [];

        foreach (var host in hosts)
        {
            bool changed = false;

            var sync = new ReferenceRedirectSync(oldId, newId);
            sync.ChangeCallBack += (path, id, message) =>
            {
                changed = true;
                results.Add(new SyncPathReportItem(host, path, null, message));
            };

            host.ReferenceSync(SyncPath.Empty, sync);

            if (changed)
            {
                dirtyHosts.Add(host);
            }
        }

        foreach (var host in dirtyHosts)
        {
            if ((host as SyncReferenceHost)?.GetOrLoadTarget() is Document doc)
            {
                doc.MarkDirty(this);
                doc.Save();

                doc.View?.RefreshView();
            }
        }

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Changes all references from oldId to newId.
    /// </summary>
    /// <param name="oldId">The old GUID to replace.</param>
    /// <param name="newId">The new GUID to use.</param>
    [Obsolete]
    internal void ChangeId(Guid oldId, Guid newId)
    {
        Update();

        ObjectEntry entry = EditorObjectManager.Instance.GetEntry(oldId);
        if (entry is null)
        {
            return;
        }

        var hosts = entry.GetReferenceHosts();

        foreach (var host in hosts)
        {
            //object owner = host;

            //if (host is SyncReferenceHost refHost)
            //{
            //    owner = refHost.Target;
            //}

            var sync = new ReferenceRedirectSync(oldId, newId);
            host.ReferenceSync(SyncPath.Empty, sync);
        }
    }

    /// <summary>
    /// Changes references within a specific host from oldId to newId.
    /// </summary>
    /// <param name="host">The reference host to update.</param>
    /// <param name="oldId">The old GUID to replace.</param>
    /// <param name="newId">The new GUID to use.</param>
    [Obsolete]
    internal void ChangeId(IReferenceHost host, Guid oldId, Guid newId)
    {
        host.ReferenceSync(SyncPath.Empty, new ReferenceRedirectSync(oldId, newId));
    }

    /// <summary>
    /// Raises the ReferenceUpdated event internally.
    /// </summary>
    internal void InternalRaiseReferenceUpdated()
    {
        RaiseReferenceUpdated();
    }

    /// <summary>
    /// Delayed action that triggers reference manager update.
    /// </summary>
    private class UpdateDelayedAction : DelayedAction
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly UpdateDelayedAction Instance = new();

        /// <summary>
        /// Creates a new update delayed action with a delay of 2 frames.
        /// </summary>
        public UpdateDelayedAction()
            : base(2)
        {
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            ReferenceManager.Current.Update();
        }
    }

    /// <summary>
    /// Delayed action that raises the reference updated event.
    /// </summary>
    private class RaiseUpdateDelayedAction : DelayedAction
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly RaiseUpdateDelayedAction Instance = new();

        /// <inheritdoc/>
        public override void DoAction()
        {
            ReferenceManagerBK.Instance.InternalRaiseReferenceUpdated();
        }
    }
}
