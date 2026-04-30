using Suity.Collections;
using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Back-end implementation of the analysis service that performs object analysis,
/// problem collection, and result generation.
/// </summary>
internal sealed class AnalysisServiceBK : AnalysisService
{
    /// <summary>
    /// Represents a single analysis task queued for processing.
    /// </summary>
    private class AnalyzeTask
    {
        /// <summary>
        /// The object to be analyzed.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Options controlling the analysis behavior.
        /// </summary>
        public AnalysisOption Option { get; }

        /// <summary>
        /// Callback invoked after analysis completes.
        /// </summary>
        public Action CallBack { get; }

        /// <summary>
        /// Creates a new analysis task.
        /// </summary>
        /// <param name="item">The object to analyze.</param>
        /// <param name="option">Analysis options.</param>
        /// <param name="callBack">Callback after completion.</param>
        public AnalyzeTask(object item, AnalysisOption option = null, Action callBack = null)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Option = option;
            CallBack = callBack;
        }
    }

    private const string MsgThreadProblem = "Thread problem encountered during analysis.";

    private static readonly AnalysisOption DefaultOption = new();

    private static readonly ConcurrentPool<HashSet<ISupportAnalysis>> _hashPool = new(() => []);
    private static readonly ConcurrentPool<List<string>> _listPool = new(() => []);

    /// <summary>
    /// Singleton instance of the analysis service.
    /// </summary>
    public static readonly AnalysisServiceBK Instance = new();

    private readonly ConcurrentQueue<AnalyzeTask> _queue = new();
    private readonly object _taskSyncRoot = new();

    private AnalysisServiceBK()
    { }

    /// <summary>
    /// Initializes the analysis service and registers it as the current instance.
    /// </summary>
    public void Initialize()
    {
        AnalysisService.Current = this;

        EditorRexes.EditorStart.AddActionListener(ProblemCollectorManager.Instance.Initialize);
    }

    /// <inheritdoc/>
    public override void QueueAnalyze(object obj, AnalysisOption option = null, Action callBack = null)
    {
        if (obj is null)
        {
            return;
        }

        EditorUtility.AddDelayedAction(new AnalyzeDelayAction(obj, option, callBack));
    }

    /// <inheritdoc/>
    public override Task Analyze(object obj, AnalysisOption option = null, Action callBack = null)
    {
        if (obj is null)
        {
            return Task.CompletedTask;
        }

        option ??= DefaultOption;

        _queue.Enqueue(new AnalyzeTask(obj, option, callBack));

        return Task.Run(ProcessAnalyzeTask);
    }

    /// <inheritdoc/>
    public override Task Analyze(ISupportAnalysis item, AnalysisOption option = null, Action callBack = null)
    {
        object obj = item;

        return Analyze(obj, option, callBack);
    }

    /// <summary>
    /// Processes queued analysis tasks sequentially.
    /// </summary>
    private void ProcessAnalyzeTask()
    {
        while (_queue.TryDequeue(out var task))
        {
            var obj = task.Item;
            if (obj is null)
            {
                continue;
            }

            var dupCheck = _hashPool.Acquire();

            try
            {
                if (obj is ISupportAnalysis supportAnalysis)
                {
                    DoAnalyze(supportAnalysis, task.Option, dupCheck);
                }
                else
                {
                    foreach (var item in Member.GetMembers<ISupportAnalysis>(obj, true))
                    {
                        DoAnalyze(item, task.Option, dupCheck);
                    }
                }

                QueuedAction.Do(() =>
                {
                    try
                    {
                        task.CallBack?.Invoke();
                    }
                    catch (Exception callBackErr)
                    {
                        callBackErr.LogError();
                    }
                });
            }
            catch (Exception err)
            {
                err.LogWarning();
            }
            finally
            {
                dupCheck.Clear();
                _hashPool.Release(dupCheck);
            }
        }
    }

    /// <summary>
    /// Performs the actual analysis on a single item, collecting problems and child items recursively.
    /// </summary>
    /// <param name="item">The item to analyze.</param>
    /// <param name="option">Analysis options.</param>
    /// <param name="analyzed">Set of already-analyzed items to prevent cycles.</param>
    private void DoAnalyze(ISupportAnalysis item, AnalysisOption option, HashSet<ISupportAnalysis> analyzed)
    {
        if (item is null)
        {
            return;
        }

        if (!analyzed.Add(item))
        {
            return;
        }

        option ??= DefaultOption;

        //string innerPath = path != null ? $"{path}>{item.GetType().Name}" : null;
        if (EditorPlugin.RuntimeLogging)
        {
            EditorServices.SystemLog.AddLog($"Analyzing : {item.GetType().Name} : {item}");
        }
        //Logs.LogDebug($"Analyzing : {innerPath} ({EditorUtility.GetDisplayString(item)})");

        AnalysisResult result = item.Analysis;
        if (result != null)
        {
            result.Clear(option);
        }
        else
        {
            result = new AnalysisResult(item);
            // Changed to write last
            //item.AnalysisResult = result;
        }

        HashSet<ISupportAnalysis> childItems = _hashPool.Acquire();
        try
        {
            if (option.CollectProblem)
            {
                item.CollectProblem(result.Problems, option?.Intent ?? AnalysisIntents.Normal);
                if (result.Problems.Count > 0)
                {
                    result.IncrementStatus(result.Problems.Status);
                }
            }


            InternalCollect(item, result, option);
            childItems.AddRange(Member.GetMembers<ISupportAnalysis>(item, true));
            
            // Try to avoid multi-threading issues
            foreach (var childItem in childItems)
            {
                DoAnalyze(childItem, option, analyzed);

                if (option.CollectProblem)
                {
                    var childResult = childItem.Analysis;
                    if (childResult?.Status > TextStatus.Info)
                    {
                        var childProblem = AnalysisProblem.FromAnalysis(childItem);
                        if (childProblem != null)
                        {
                            result.Problems.Add(childProblem);
                        }

                        result.IncrementStatus(childResult.Status);
                    }
                }
            }
        }
        catch (Exception err)
        {
            err.LogWarning();
        }
        finally
        {
            childItems.Clear();
            _hashPool.Release(childItems);
        }

        GenerateResultText(item, result, option);

        item.Analysis = result;
    }

    /// <inheritdoc/>
    public override void CollecctProblems(object obj, AnalysisProblem problems, AnalysisIntents intent)
    {
        if (obj is null)
        {
            return;
        }

        try
        {
            var collector = ProblemCollectorManager.Instance.GetCollector(obj.GetType());
            collector?.CollectProblem(obj, problems, intent);
        }
        catch (Exception err)
        {
            err.LogError();
        }
    }

    /// <inheritdoc/>
    public override void ShowProblems(AnalysisResult result)
    {
        EditorUtility.ClearLogView();

        if (result is null)
        {
            return;
        }

        var report = result.Problems.BuildLoggerReport();
        Logs.AddLog(result.Status.ToLogMessageType(), report);

        //foreach (var problem in result.Problems.GetItems())
        //{
        //    Logs.AddLog(problem.Status.GetLogMessageType(), problem);
        //}

        if (result.IdConflictCount > 0 && result.Id != Guid.Empty)
        {
            var entry = EditorObjectManager.Instance.GetEntry(result.Id);
            if (entry != null)
            {
                foreach (var target in entry.Targets)
                {
                    string message = $"Id conflict:{target.FullName}";
                    Logs.AddLog(LogMessageType.Warning, new ObjectLogCoreItem(message, target));
                }
            }
        }

        if (result.AssetKeyConflictCount > 0 && !string.IsNullOrEmpty(result.AssetKey))
        {
            var entry = AssetManager.Instance.GetAssetEntry(result.AssetKey);
            if (entry != null)
            {
                foreach (var target in entry.Targets)
                {
                    string message = $"AssetKey conflict:{target.FullName}";
                    Logs.AddLog(LogMessageType.Warning, new ObjectLogCoreItem(message, target));
                }
            }
        }

        if (result.ReferenceMissingCount > 0)
        {
            Logs.LogError($"{result.ReferenceMissingCount} reference(s) missing");
        }

        if (result.ReferenceConflictCount > 0)
        {
            Logs.LogError($"{result.ReferenceConflictCount} reference conflict(s)");
        }

        EditorUtility.ShowLogView();
    }

    /// <inheritdoc/>
    public override void ShowProblems(AnalysisProblem problems)
    {
        EditorUtility.ClearLogView();

        if (problems is null)
        {
            return;
        }

        var report = problems.BuildLoggerReport();
        Logs.AddLog(problems.Status.ToLogMessageType(), report);

        //foreach (var problem in problems.GetItems())
        //{
        //    Logs.AddLog(problem.Status.GetLogMessageType(), problem);
        //}

        EditorUtility.ShowLogView();
    }

    /// <summary>
    /// Collects internal analysis data such as references, conflicts, dependencies, and render targets.
    /// </summary>
    /// <param name="item">The item being analyzed.</param>
    /// <param name="result">The analysis result to populate.</param>
    /// <param name="option">Analysis options controlling what to collect.</param>
    private void InternalCollect(object item, AnalysisResult result, AnalysisOption option)
    {
        Guid id = Guid.Empty;

        if (item is IHasId idContext)
        {
            id = idContext.Id;
        }
        //else if (item is SObject sObject)
        //{
        //    id = sObject.ObjectType.TargetId;
        //}
        //else if (item is SItem sItem)
        //{
        //    id = sItem.InputType.TargetId;
        //}

        result.Id = id;

        if (id != Guid.Empty)
        {
            if (option.CollectMember)
            {
                if (item is IMemberContainer container)
                {
                    result.MemberCount = container.MemberCount;
                }
                else
                {
                    result.MemberCount = -1;
                }
            }

            if (option.CollectReference)
            {
                if (!ReferenceManager.Current.IsDirty)
                {
                    result.ReferenceCount = ReferenceManager.Current.GetReferenceCount(id);
                }
                else
                {
                    result.ReferenceCount = -1;
                }

                result.ResourceUseCount = EditorServices.MonitorService?.Current?.GetResourceCount(id) ?? 0;
                if (result.ResourceUseCount > 0)
                {
                    result.IncrementStatus(TextStatus.ResourceUse);
                }
            }

            if (option.CollectConflict)
            {
                EditorObject obj = EditorObjectManager.Instance.GetObject(id);
                if (obj?.IdConflict ?? false)
                {
                    result.IdConflictCount = obj.Entry.TargetCount - 1;
                    result.IncrementStatus(TextStatus.Warning);
                }
                else
                {
                    result.IdConflictCount = 0;
                }

                if (obj is Asset asset)
                {
                    result.AssetKey = asset.AssetKey;

                    if (asset.AssetKeyConflict)
                    {
                        result.AssetKeyConflictCount = asset.AssetEntry.TargetCount - 1;
                        result.IncrementStatus(TextStatus.Warning);
                    }
                    else
                    {
                        result.AssetKeyConflictCount = 0;
                    }

                    result.MultipleFullTypeNameCount = asset.MultipleFullTypeNames?.Count ?? 0;
                }
                else
                {
                    result.AssetKeyConflictCount = 0;
                }
            }
        }

        if (option.CollectExternalDependencies)
        {
            result.DependencyObjects.Clear();
            result.DependencyFileAssets.Clear();

            var asset = AssetManager.Instance.GetAsset(id);
            var owner = asset?.GetStorageObject(true);
            var fullPath = asset?.GetStorageLocation();
            if (owner != null && fullPath != null)
            {
                var objs = ReferenceManager.Current.CollectDependencies(owner)
                    .Select(idR => EditorObjectManager.Instance.GetObject(idR))
                    .SkipNull()
                    .Distinct();

                var fileAssets = objs
                    .Select(o => o.GetFileAsset())
                    .SkipNull()
                    .Where(o => o.Id != id)
                    .Select(o => o.Id)
                    .Distinct();

                result.DependencyObjects.AddRange(objs.Select(o => o.Id));
                result.DependencyFileAssets.AddRange(fileAssets);
            }
        }

        if (option.CollectProblemDependencies)
        {
            try
            {
                var problems = ReferenceManager.Current.CollectProblemDependencies(item);

                foreach (var problem in problems)
                {
                    switch (problem.Value)
                    {
                        case ReferenceProblemTypes.Missing:
                            {
                                string missingName = GlobalIdResolver.Current.RevertResolve(problem.Key);
                                if (!string.IsNullOrWhiteSpace(missingName))
                                {
                                    result.Problems.Add(new AnalysisProblem(TextStatus.Error, $"Reference missing:{missingName}({problem.Key})"));
                                }
                                else
                                {
                                    result.Problems.Add(new AnalysisProblem(TextStatus.Error, $"Reference missing:({problem.Key})"));
                                }

                                result.ReferenceMissingCount++;
                                break;
                            }
                        case ReferenceProblemTypes.Conflict:
                            {
                                var obj = EditorObjectManager.Instance.GetObject(problem.Key);
                                result.Problems.Add(new AnalysisProblem(TextStatus.Error, $"Reference conflict:{obj}"));
                                result.ReferenceConflictCount++;
                            }
                            break;

                        default:
                            break;
                    }
                }

                if (result.ReferenceMissingCount > 0)
                {
                    result.IncrementStatus(TextStatus.Warning);
                }

                if (result.ReferenceConflictCount > 0)
                {
                    result.IncrementStatus(TextStatus.Warning);
                }
            }
            catch (Exception err)
            {
                err.LogWarning();
            }
        }

        if (option.CollectRenderTargets && id != Guid.Empty)
        {
            ICodeRenderInfoService buildInfo = EditorServices.CodeRenderInfo;

            try
            {
                foreach (var target in buildInfo.GetAffectedRenderTargets(id).SkipNull())
                {
                    result.RenderTargets.Add(target);
                }
            }
            catch (Exception err)
            {
                err.LogWarning(MsgThreadProblem);
            }

            result.RenderTargets.Sort((a, b) =>
            {
                if (a is null) return -1;
                if (b is null) return 1;

                Guid idA = a.RenderItemId;
                Guid idB = b.RenderItemId;

                if (idA != idB)
                {
                    if (idA == id)
                    {
                        return -1;
                    }

                    if (idB == id)
                    {
                        return 1;
                    }
                }

                string pathA = a.FileName?.PhysicRelativePath ?? string.Empty;
                string pathB = b.FileName?.PhysicRelativePath ?? string.Empty;

                return pathA.CompareTo(pathB);
            });

            //foreach (var target in result.RenderTargets.Where(o => o.RenderItemId == id))
            //{
            //    result.UserCodeCount += target.UserCodeCount;
            //}

            result.UserCodeCount = buildInfo.GetUserCodeCount(id);
        }
    }

    /// <summary>
    /// Generates a human-readable summary text from the analysis result.
    /// </summary>
    /// <param name="item">The analyzed item.</param>
    /// <param name="result">The analysis result containing data to summarize.</param>
    /// <param name="option">Analysis options.</param>
    private void GenerateResultText(ISupportAnalysis item, AnalysisResult result, AnalysisOption option)
    {
        if (!option.GenerateResultText)
        {
            result.AnalysisText = string.Empty;
            return;
        }

        List<string> list = _listPool.Acquire();
        list.Clear();

        if (result.ReferenceCount > 0)
        {
            list.Add($"{result.ReferenceCount} reference(s)");
        }
        else if (result.Id != Guid.Empty)
        {
            if (result.ReferenceCount == 0)
            {
                list.Add("0 references");
            }
            else
            {
                list.Add("? references");
            }
        }

        if (result.UserCodeCount > 0)
        {
            list.Add($"{result.UserCodeCount} code segment(s)");
        }

        if (result.MemberCount > 0)
        {
            list.Add($"{result.MemberCount} member(s)");
        }

        if (result.ResourceUseCount > 0)
        {
            list.Add($"{result.ResourceUseCount} time(s) used");
        }

        if (result.MultipleFullTypeNameCount > 1)
        {
            list.Add($"{result.MultipleFullTypeNameCount} name sharing(s)");
        }

        if (result.DependencyFileAssets.Count > 0)
        {
            list.Add($"{result.DependencyFileAssets.Count} file dependency(ies)");
        }


        if (result.IdConflictCount > 0)
        {
            list.Add($"{result.IdConflictCount} Id conflict(s)");
        }

        if (result.AssetKeyConflictCount > 0)
        {
            list.Add($"{result.AssetKeyConflictCount} AssetKey conflict(s)");
        }

        if (result.ReferenceMissingCount > 0)
        {
            list.Add($"{result.ReferenceMissingCount} reference(s) missing");
        }

        if (result.ReferenceConflictCount > 0)
        {
            list.Add($"{result.ReferenceConflictCount} reference conflict(s)");
        }

        if (result.Problems.Count > 0)
        {
            list.Add($"{result.Problems.Count} problem(s)");
        }

        result.AnalysisText = string.Join(" | ", list);

        list.Clear();
        _listPool.Release(list);
    }

    #region AnalyzeDelayAction

    /// <summary>
    /// Delayed action wrapper for triggering analysis operations.
    /// </summary>
    private class AnalyzeDelayAction : DelayedAction<object>
    {
        private readonly AnalysisOption _option;
        private readonly Action _callBack;

        /// <summary>
        /// Creates a new delayed analysis action.
        /// </summary>
        /// <param name="obj">The object to analyze.</param>
        /// <param name="option">Analysis options.</param>
        /// <param name="callBack">Callback after completion.</param>
        public AnalyzeDelayAction(object obj, AnalysisOption option = null, Action callBack = null) : base(obj)
        {
            _option = option;
            _callBack = callBack;
        }

        /// <inheritdoc/>
        public override void DoAction()
        {
            object o = base.Value;

            EditorServices.SystemLog.AddLog($"{nameof(AnalysisService)} DoAnalyze : {o}...");
            EditorServices.AnalysisService.Analyze(o, _option, HandleOK);
            
        }

        /// <summary>
        /// Invoked when analysis completes successfully.
        /// </summary>
        private void HandleOK()
        {
            EditorServices.SystemLog.AddLog($"{nameof(AnalysisService)} DoAnalyze OK");
            _callBack?.Invoke();
        }
    }

    #endregion
}

/// <summary>
/// Manages problem collectors for different object types, resolving the appropriate
/// collector through type hierarchy mapping.
/// </summary>
internal class ProblemCollectorManager
{
    /// <summary>
    /// Singleton instance of the problem collector manager.
    /// </summary>
    public static readonly ProblemCollectorManager Instance = new ProblemCollectorManager();

    private readonly TypeMappingChain _typeMapping = new();
    private readonly Dictionary<Type, ProblemCollector> _collectorsByTarget = [];
    private readonly Dictionary<Type, ProblemCollector> _collectorsBySelf = [];

    /// <summary>
    /// Initializes the manager by scanning and registering all derived ProblemCollector types.
    /// </summary>
    public void Initialize()
    {
        var types = typeof(ProblemCollector<>).GetDerivedTypes().ToList();
        types.Sort((a, b) =>
        {
            if (a.HasAttributeCached<InternalPriorityAttribute>())
            {
                return -1;
            }

            if (b.HasAttributeCached<InternalPriorityAttribute>())
            {
                return 1;
            }

            return 0;
        });

        foreach (var collectorType in types)
        {
            try
            {
                Type targetType = collectorType.BaseType.GetGenericArguments()[0];
                _typeMapping.TryAddMap(targetType, collectorType);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Resolves and returns the appropriate problem collector for the given type.
    /// </summary>
    /// <param name="type">The type to find a collector for.</param>
    /// <returns>The resolved problem collector, or null if none found.</returns>
    public ProblemCollector GetCollector(Type type)
    {
        if (type is null)
        {
            return null;
        }

        if (_collectorsByTarget.TryGetValue(type, out var collector))
        {
            return collector;
        }

        Type[] types = _typeMapping.ResolveTypeChain(type);
        if (types.Length == 0)
        {
            return null;
        }

        try
        {
            List<ProblemCollector> collectors = [];
            foreach (Type ctype in types)
            {
                collector = _collectorsBySelf.GetOrAdd(ctype, t => (ProblemCollector)Activator.CreateInstance(t));
                collectors.Add(collector);
            }

            for (int i = 0; i < collectors.Count - 1; i++)
            {
                collectors[i].Base = collectors[i + 1];
            }

            _collectorsByTarget[type] = collectors[0];

            return collectors[0];
        }
        catch (Exception err)
        {
            err.LogError();

            return null;
        }
    }
}
