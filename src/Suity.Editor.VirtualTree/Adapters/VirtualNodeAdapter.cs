using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.VirtualTree.Adapters;

public abstract class VirtualNodeAdapter : MarshalByRefObject, ISyncContext
{
    private VirtualNode _owner;

    internal virtual void SetupNode(VirtualNode ownerNode)
    {
        _owner = ownerNode ?? throw new ArgumentNullException(nameof(ownerNode));
    }

    public VirtualNode Owner => _owner;

    public object GetValue() => _owner?.DisplayedValue;

    public T GetParentEditor<T>() where T : class
    {
        return _owner != null ? AdapterHelper.GetAdapterEditor(_owner.Parent) as T : null;
    }

    public T GetParentValue<T>()
    {
        VirtualNode parent = _owner?.Parent;
        object value = parent?.DisplayedValue;

        return value is T tValue ? tValue : default;
    }

    public object ParentValue
    {
        get
        {
            VirtualNode parent = _owner?.Parent;

            return parent?.DisplayedValue;
        }
    }

    public virtual object GetService(Type serviceType)
    {
        if (serviceType.IsAssignableFrom(this.GetType()))
        {
            return this;
        }

        var model = FindModel();
        var provider = model?.ServiceProvider;

        return provider?.GetService(serviceType);
    }

    internal VirtualTreeModel FindModel()
    {
        return _owner?.FindModel();
    }

    #region ISyncContext

    object ISyncContext.Parent => ParentValue;

    object IServiceProvider.GetService(Type serviceType)
    {
        return GetService(serviceType);
    }

    #endregion

    #region Virtual

    protected internal virtual void OnAdded()
    { }

    protected internal virtual void OnRemoved()
    { }

    public virtual string Text
    {
        get
        {
            ITextDisplay ext = GetValue() as ITextDisplay;
            return ext?.DisplayText;
        }
        set
        {
            ITextEdit ext = GetValue() as ITextEdit;
            ext?.SetText(value, this);
        }
    }

    public virtual string Description
    {
        get
        {
            IDescriptionDisplay ext = GetValue() as IDescriptionDisplay;

            return ext?.Description;
        }
    }

    public virtual string PreviewText
    {
        get
        {
            IPreviewDisplay ext = GetValue() as IPreviewDisplay;

            return ext?.PreviewText;
        }
        set
        {
            IPreviewEdit ext = GetValue() as IPreviewEdit;
            ext?.SetPreviewText(value, this);
        }
    }

    public virtual Image Icon
    {
        get
        {
            return GetValue() is ITextDisplay ext ? EditorUtility.GetIcon(ext.DisplayIcon) : null;
        }
    }

    public virtual Image PreviewIcon
    {
        get
        {
            return GetValue() is IPreviewDisplay ext ? EditorUtility.GetIcon(ext.PreviewIcon) : null;
        }
    }

    public string FieldDisplayName => string.Empty;

    public virtual bool CanEditText
    {
        get
        {
            ITextEdit ext = GetValue() as ITextEdit;

            return ext?.CanEditText ?? false;
        }
    }

    public virtual bool CanEditPreviewText
    {
        get
        {
            IPreviewEdit ext = GetValue() as IPreviewEdit;

            return ext?.CanEditPreviewText ?? false;
        }
    }

    public virtual TextStatus TextStatus
    {
        get
        {
            ITextDisplay ext = GetValue() as ITextDisplay;

            return ext?.DisplayStatus ?? TextStatus.Normal;
        }
    }

    public virtual void HandleNodeAction()
    {
        (GetValue() as IViewDoubleClickAction)?.DoubleClick();
    }

    public virtual void HandleKeyDown(int key)
    { }

    public virtual string GetContextMenuKey() => null;

    public virtual bool CanDropIn(object value) => false;

    public virtual object DropInConvert(object value) => value;

    #endregion
}