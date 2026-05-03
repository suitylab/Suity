using Suity.Drawing;
using Suity.Editor.VirtualTree.Actions;
using Suity.Editor.VirtualTree.Adapters;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A virtual tree node that represents a generic list with adapter-based operations.
/// Supports list item management including insertion, removal, drag-and-drop,
/// and GUI-based item creation through the <see cref="ListAdapter"/>.
/// </summary>
public class ListVirtualNode : VirtualNode, IVirtualNodeListOperation
{
    private readonly ListAdapter _adapter;
    private object _listObj;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListVirtualNode"/> class.
    /// </summary>
    public ListVirtualNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListVirtualNode"/> class with the specified adapter.
    /// </summary>
    /// <param name="adapter">The list adapter to use for list operations. Must not be null.</param>
    public ListVirtualNode(ListAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _adapter.SetupNode(this);
    }

    /// <summary>
    /// Gets the list adapter used by this node.
    /// </summary>
    internal ListAdapter Adapter => _adapter;

    /// <inheritdoc/>
    public override object DisplayedValue => _listObj;

    #region Editor

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        base.OnGetValue();

        if (IsContentInitialized)
        {
            _listObj = GetValue();
            UpdateElementNodes();
        }
        else
        {
            _listObj = null;
            ClearContent();
        }
    }

    /// <inheritdoc/>
    protected override void OnAdded() => _adapter.OnAdded();

    /// <inheritdoc/>
    protected override void OnRemoved() => _adapter.OnRemoved();

    /// <inheritdoc/>
    public override void HandleNodeAction() => _adapter.HandleNodeAction();

    /// <inheritdoc/>
    public override void HandleKeyDown(int key) => _adapter.HandleKeyDown(key);

    /// <inheritdoc/>
    public override string GetContextMenuKey() => VirtualNode.ContextMenu_ArrayOwner;

    /// <inheritdoc/>
    public override string GetChildContextMenuKey(VirtualNode childNode) => VirtualNode.ContextMenu_ArrayMember;

    /// <inheritdoc/>
    public override bool CanDropIn(VirtualNode node)
    {
        if (ReadOnly)
        {
            return false;
        }

        if (node.Parent == this)
        {
            return true;
        }
        else
        {
            return _adapter?.CanDropIn(node.DisplayedValue) == true;
        }
    }

    /// <inheritdoc/>
    public override bool CanDropInExternal(object obj)
    {
        if (ReadOnly)
        {
            return false;
        }

        return _adapter?.CanDropIn(obj) == true;
    }

    /// <inheritdoc/>
    public override object DropInConvert(object obj)
    {
        if (ReadOnly)
        {
            return null;
        }

        return _adapter?.DropInConvert(obj);
    }

    #endregion

    #region Display

    /// <inheritdoc/>
    protected override string GetText()
    {
        return _adapter?.Text ?? base.GetText();
    }

    /// <inheritdoc/>
    protected override void SetText(string value)
    {
        if (_adapter != null)
        {
            _adapter.Text = value;
        }
    }

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        do
        {
            if (_adapter is null)
            {
                break;
            }

            try
            {
                if (_adapter.GetValue() is ITextDisplay textDisplay)
                {
                    var status = textDisplay.DisplayStatus;
                    if (status != TextStatus.Normal)
                    {
                        return status;
                    }
                }

                if (_adapter.Count > 0)
                {
                    return TextStatus.Normal;
                }
            }
            catch (Exception)
            {
                break;
            }

        } while (false);

        return Styles?.GetEmptyListGray() == true ? TextStatus.Disabled : TextStatus.Normal;
    }

    /// <inheritdoc/>
    protected override string GetDescription()
    {
        return _adapter.Description ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override string GetPreviewText()
    {
        string customText = GetCustomPreviewText();
        if (customText != null)
        {
            return customText;
        }

        return _adapter?.PreviewText ?? base.GetPreviewText();
    }

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        if (_adapter != null)
        {
            _adapter.PreviewText = value;
        }
    }

    /// <inheritdoc/>
    protected override ImageDef GetMainIcon()
    {
        return _adapter?.Icon ?? base.GetMainIcon();
    }

    /// <inheritdoc/>
    protected override ImageDef GetPreviewIcon()
    {
        var customIcon = GetCustomPreviewIcon();
        if (customIcon != null)
        {
            return customIcon;
        }

        return _adapter?.PreviewIcon ?? base.GetPreviewIcon();
    }

    /// <inheritdoc/>
    protected override string GetFieldDisplayName()
    {
        return _adapter?.FieldDisplayName ?? base.GetFieldDisplayName();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditText()
    {
        return _adapter?.CanEditText ?? base.GetCanEditText();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditPreviewText()
    {
        return _adapter?.CanEditPreviewText ?? base.GetCanEditPreviewText();
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates the child element nodes to match the current state of the list adapter.
    /// Creates, replaces, or removes nodes as needed to synchronize with the adapter's item count.
    /// </summary>
    protected void UpdateElementNodes()
    {
        var model = FindModel();
        if (model is null) return;

        for (int i = 0; i < _adapter.Count; i++)
        {
            object item = _adapter.GetItem(i);
            Type elementType = item?.GetType();
            VirtualNode elementNode;

            if (i < this.Nodes.Count)
            {
                elementNode = Nodes[i];

                if (elementNode.EditedType != elementType)
                {
                    VirtualNode oldNode = elementNode;
                    elementNode = model.CreateNode(elementType, this);

                    this.Nodes.Add(elementNode, oldNode);
                    this.Nodes.Remove(oldNode);

                    model.ConfigureNode(elementNode);
                }
            }
            else
            {
                elementNode = model.CreateNode(elementType, this);
                this.Nodes.Add(elementNode);
                model.ConfigureNode(elementNode);
            }

            var listDisplay = _adapter.GetValue() as IListTextDisplay;

            elementNode.PropertyName = listDisplay?.GetText(i) ?? string.Format("[{0}]", i);
            elementNode.Getter = this.CreateElementValueGetter(i);
            elementNode.Setter = this.CreateElementValueSetter(i);

            elementNode.PerformGetValue();
        }

        if (this.Nodes.Count > _adapter.Count)
        {
            for (int i = this.Nodes.Count - 1; i >= _adapter.Count; i--)
            {
                VirtualNode oldNode = Nodes[i];
                Nodes.RemoveAt(i);
                oldNode.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates a getter function for retrieving the value at the specified list index.
    /// </summary>
    /// <param name="index">The index of the list element.</param>
    /// <returns>A function that returns the value at the specified index.</returns>
    protected Func<object> CreateElementValueGetter(int index)
    {
        return () => _adapter.GetItem(index);
    }

    /// <summary>
    /// Creates a setter action for updating the value at the specified list index.
    /// </summary>
    /// <param name="index">The index of the list element.</param>
    /// <returns>An action that sets the value at the specified index.</returns>
    protected Action<object> CreateElementValueSetter(int index)
    {
        return (object value) =>
        {
            var model = FindModel();
            if (model != null)
            {
                model.HandleSetterAction(new ListEditorSetterAction(this, index, value));
            }
            else
            {
                _adapter.SetItem(index, value);
            }
        };
    }

    #endregion

    #region IVirtualNodeListOperation

    /// <inheritdoc/>
    public Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        if (ReadOnly)
        {
            return Task.FromResult<object>(null);
        }

        return _adapter.GuiCreateItemAsync(typeHint);
    }

    /// <inheritdoc/>
    public Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        if (ReadOnly)
        {
            return Task.FromResult<object[]>([]);
        }

        return _adapter.GuiCreateItemsAsync(typeHint);
    }

    /// <inheritdoc/>
    public bool CanAddItem(object value)
    {
        if (ReadOnly)
        {
            return false;
        }

        return value != null && _adapter.CanDropIn(value);
    }

    /// <inheritdoc/>
    public int Count => _adapter.Count;

    /// <inheritdoc/>
    public VirtualNode InsertListItem(int index, object value, bool config)
    {
        if (ReadOnly)
        {
            return null;
        }

        if (value != null && !_adapter.CanDropIn(value))
        {
            //Suity.AppSystem.AppService.Instance.ShowMessageBox("This type of object is not accepted");
            return null;
        }

        value = _adapter.DropInConvert(value);
        if (value is null)
        {
            return null;
        }

        if (index < 0)
        {
            index = 0;
        }

        if (index >= this.Nodes.Count)
        {
            index = this.Nodes.Count;
        }

        var model = FindModel();
        model.PerformValueAction(new ListEditorInsertAction(this, index, value));

        // Synchronize editor nodes
        VirtualNode newNode = model.CreateNode(value.GetType(), this);
        this.Nodes.Insert(index, newNode);
        model.ConfigureNode(newNode);

        newNode.Getter = this.CreateElementValueGetter(index);
        newNode.Setter = this.CreateElementValueSetter(index);

        PerformGetValue();
        newNode.PerformGetValue();

        return newNode;

        //return this.Nodes[index];
    }

    /// <inheritdoc/>
    public void RemoveListItem(VirtualNode node)
    {
        if (ReadOnly)
        {
            return;
        }

        if (node.Parent != this)
        {
            return;
        }

        var model = FindModel();

        int index = node.Index;
        //Nodes.Remove(node);
        //node.Dispose();

        model.SuspendGetValue();
        model.PerformValueAction(new ListEditorRemoveAction(this, index));
        // Synchronized removal
        Nodes.RemoveAt(index);
        model.ResumeGetValue(false);

        PerformGetValue();
    }

    /// <inheritdoc/>
    public void RemoveListItems(IEnumerable<VirtualNode> nodes)
    {
        if (ReadOnly)
        {
            return;
        }

        var removeNodes = nodes.Where(o => o.Parent == this);
        if (!removeNodes.Any())
        {
            return;
        }

        var model = FindModel();
        IEnumerable<int> indexes = removeNodes.OrderByDescending(o => o.Index).Select(o => o.Index);

        model.SuspendGetValue();
        foreach (var index in indexes)
        {
            model.PerformValueAction(new ListEditorRemoveAction(this, index));
            // Synchronized removal
            Nodes.RemoveAt(index);
        }
        model.ResumeGetValue(false);

        PerformGetValue();
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _listObj = null;
    }
}
