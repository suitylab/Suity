using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.Values;

/// <summary>
/// Base controller class for SObject, providing data binding, validation, and GUI configuration capabilities.
/// </summary>
public abstract class SObjectController :
    IViewObject,
    IReference,
    ISupportAnalysis,
    IEditorObjectListener
{
    private SObject _target;

    public SObjectController()
    { }

    #region Type info

    /// <summary>
    /// Gets the input type definition of the target SObject.
    /// </summary>
    public TypeDefinition InputType => _target?.InputType;

    /// <summary>
    /// Gets a value indicating whether instance access mode is enabled. Default is false.
    /// </summary>
    public virtual bool InstanceAccess => false;

    /// <summary>
    /// Gets the target object type definition.
    /// </summary>
    public virtual TypeDefinition TargetObjectType => null;

    /// <summary>
    /// Gets the native DType for this controller.
    /// </summary>
    public DType NativeDType => DTypeManager.Instance.GetNativeDType(this.GetType());

    #endregion

    #region Start Stop Resume

    internal void Start(SObject target)
    {
        if (_target != null)
        {
            Release();
        }

        _target = target;

        try
        {
            Commit();
            OnStart();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }
    }

    internal void Resume(SObject target)
    {
        _target = target;
        foreach (var name in target.GetPropertyNames())
        {
            this.SetProperty(name, target.GetPropertyFormatted(name), SyncContext.Empty);
        }

        Commit();
    }

    internal void Release()
    {
        _target = null;

        try
        {
            OnRelease();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }
    }

    /// <summary>
    /// Override this method to implement custom start logic when the controller is attached to an SObject.
    /// </summary>
    protected virtual void OnStart()
    { }

    /// <summary>
    /// Override this method to implement custom release logic when the controller is detached from an SObject.
    /// </summary>
    protected virtual void OnRelease()
    { }

    #endregion

    #region Parenting

    /// <summary>
    /// Gets the target SObject being controlled.
    /// </summary>
    public SObject Target => _target;

    /// <summary>
    /// Ensures that a target SObject exists, creating one if necessary.
    /// </summary>
    /// <returns>The target SObject.</returns>
    public SObject EnsureTarget() => _target ??= new SObject(this);

    /// <summary>
    /// Finds the parent owner of the specified type in the SObject hierarchy.
    /// </summary>
    /// <typeparam name="T">The type of owner to find.</typeparam>
    /// <returns>The parent owner of the specified type, or null if not found.</returns>
    public T FindParentOwner<T>() where T : class
    {
        SContainer parent = _target.Parent;
        while (parent != null)
        {
            if (parent.Context is T t)
            {
                return t;
            }

            parent = parent.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a property by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property value.</returns>
    public object GetProperty(string name)
    {
        SinglePropertySync sync = SinglePropertySync.CreateGetter(name);
        OnSync(sync, SyncContext.Empty);

        return sync.Value;
    }

    /// <summary>
    /// Gets the root asset associated with this controller.
    /// </summary>
    /// <returns>The root asset, or null if not available.</returns>
    public Asset GetRootAsset() => (RootContext as IHasAsset)?.TargetAsset;

    /// <summary>
    /// Gets the asset filter for the root asset.
    /// </summary>
    /// <param name="instance">Whether to get instance filter.</param>
    /// <returns>The asset filter.</returns>
    public IAssetFilter GetRootAssetFilter(bool instance = false)
    {
        var asset = GetRootAsset();
        return asset != null ? asset.GetInstanceFilter(instance) : AssetFilters.Default;
    }

    /// <summary>
    /// Gets the root context object.
    /// </summary>
    public object RootContext => _target?.RootContext;

    /// <summary>
    /// Gets the root context of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of context to retrieve.</typeparam>
    /// <returns>The root context cast to the specified type, or null.</returns>
    public T GetRootContext<T>() where T : class => RootContext as T;

    #endregion

    #region Gui

    /// <summary>
    /// Configures the GUI for this controller asynchronously.
    /// </summary>
    /// <param name="title">The title for the configuration dialog.</param>
    public async Task GuiConfig(string title)
    {
        try
        {
            OnUpdateStatus();
            await OnGuiConfig(title);
        }
        catch (Exception ex)
        {
            ex.LogError();
        }
    }

    /// <summary>
    /// Handles text input for this controller.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    public virtual void InputText(string inputText)
    { }

    /// <summary>
    /// Override this method to implement custom GUI configuration.
    /// </summary>
    /// <param name="title">The title for the configuration dialog.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnGuiConfig(string title) => Task.CompletedTask;
    

    /// <summary>
    /// Creates a new SObject using GUI and optionally configures it.
    /// </summary>
    /// <param name="inputType">The input type definition for the new object.</param>
    /// <param name="title">The title for the dialog.</param>
    /// <returns>The created SObject, or null if creation was cancelled.</returns>
    protected virtual async Task<SObject> GuiCreateObject(TypeDefinition inputType, string title)
    {
        if (_target is null)
        {
            throw new NullReferenceException();
        }

        var obj = await inputType.GuiCreateObject(_target, title);
        if (obj != null)
        {
            await GuiConfigObject(obj, title);
        }

        return obj;
    }

    /// <summary>
    /// Configures an SObject using GUI.
    /// </summary>
    /// <param name="childObject">The SObject to configure.</param>
    /// <param name="title">The title for the dialog.</param>
    protected virtual async Task GuiConfigObject(SObject childObject, string title)
    {
        Commit();

        await childObject.GuiConfigObject(title);
    }

    #endregion

    #region Property & Status

    /// <summary>
    /// Commits all pending property changes to the target SObject.
    /// </summary>
    public void Commit()
    {
        if (_target != null)
        {
            try
            {
                OnUpdateStatus();
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }

            var sync = new GetAllPropertySync(false);

            try
            {
                OnSync(sync, SyncContext.Empty);
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }

            foreach (var pair in sync.Values)
            {
                _target.InternalSetProperty(pair.Key, pair.Value.Value);
            }
        }
    }

    /// <summary>
    /// Updates the status of the controller.
    /// </summary>
    public void UpdateStatus()
    {
        try
        {
            OnUpdateStatus();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
        }
    }

    internal void InternalSetProperty(Guid id, string name, object value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            SinglePropertySync sync = SinglePropertySync.CreateSetter(name, value);
            try
            {
                OnSync(sync, SyncContext.Empty);
            }
            catch (Exception err)
            {
                Logs.LogError(err);
            }

            // Verify and set value again after taking effect
            var newValue = GetProperty(name);
            _target?.InternalSetProperty(id, newValue);
        }
    }

    /// <summary>
    /// Override this method to implement custom status update logic.
    /// </summary>
    protected virtual void OnUpdateStatus()
    { }

    #endregion

    #region ISyncObject IViewObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        var wrapper = new DNativePropertySync(this, sync);
        OnSync(wrapper, context);

        if (sync.IsSetter())
        {
            try
            {
                OnUpdateStatus();
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }
    }

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        try
        {
            OnUpdateStatus();
            OnSetupView(setup);
        }
        catch (Exception ex)
        {
            ex.LogError();
        }
    }

    internal void InternalSync(IPropertySync sync, ISyncContext context) => OnSync(sync, context);

    /// <summary>
    /// Override this method to implement custom synchronization logic.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    /// <summary>
    /// Sets up the view for this controller.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    public void SetupView(IViewObjectSetup setup) => OnSetupView(setup);

    /// <summary>
    /// Override this method to implement custom view setup.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    #endregion

    #region IReference

    /// <summary>
    /// Synchronizes references for this controller.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync object.</param>
    public virtual void ReferenceSync(SyncPath path, IReferenceSync sync)
    { }

    #endregion

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    AnalysisResult ISupportAnalysis.Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems for this controller.
    /// </summary>
    /// <param name="problems">The analysis problem collection.</param>
    /// <param name="intent">The analysis intent.</param>
    public virtual void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    { }

    #endregion

    #region IEditorObjectListener

    /// <summary>
    /// Handles object update events.
    /// </summary>
    /// <param name="id">The object ID.</param>
    /// <param name="obj">The editor object.</param>
    /// <param name="args">The entry event arguments.</param>
    /// <param name="handled">Whether the event has been handled.</param>
    public virtual void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    { }

    #endregion

    #region class DNativePropertySync

    private class DNativePropertySync : MarshalByRefObject, IPropertySync
    {
        private readonly SObjectController _controller;
        private readonly IPropertySync _inner;

        public DNativePropertySync(SObjectController owner, IPropertySync inner)
        {
            _controller = owner;
            _inner = inner;
        }

        public SyncIntent Intent => _inner.Intent;
        public SyncMode Mode => _inner.Mode;
        public string Name => _inner.Name;
        public IEnumerable<string> Names => _inner.Names;
        public object Value => _inner.Value;

        public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
        {
            T result = _inner.Sync(name, obj, flag, defaultValue);
            _controller._target?.InternalSetProperty(name, result);

            return result;
        }
    }

    #endregion

    public static SObject CreateObject<T>()
        where T : SObjectController, new()
    {
        var controller = new T();
        return new SObject(controller);
    }
}