using Suity;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.VirtualTree.SValues;

/// <summary>
/// Virtual tree node that represents an <see cref="SArray"/> value,
/// providing list operations such as add, insert, remove, and drag-and-drop support.
/// </summary>
[VirtualNodeUsage(typeof(SArray))]
public class SArrayNode : VirtualNode, IVirtualNodeListOperation
{
    private SArray _list;

    /// <inheritdoc/>
    public override object DisplayedValue => _list;

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        base.OnGetValue();

        _list = GetValue() as SArray;
        if (_list is null)
        {
            ClearContent();
        }
        else
        {
            if (IsContentInitialized)
            {
                UpdateElementNodes(_list);
            }
            else
            {
                ClearContent();
            }
        }
    }

    /// <summary>
    /// Updates child element nodes to match the current state of the array.
    /// Replaces nodes whose edited type has changed, creates new nodes for added elements,
    /// and removes nodes for deleted elements.
    /// </summary>
    /// <param name="value">The array whose elements should be reflected in the node tree.</param>
    protected void UpdateElementNodes(SArray value)
    {
        var model = FindModel();

        Type elementType = value.InputType.ElementType.GetNativeType();

        for (int i = 0; i < value.Count; i++)
        {
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

            elementNode.PropertyName = string.Format("[{0}]", i);
            elementNode.Getter = this.CreateElementValueGetter(i);
            elementNode.Setter = this.CreateElementValueSetter(i);
            elementNode.PerformGetValue();
        }

        if (this.Nodes.Count > value.Count)
        {
            for (int i = this.Nodes.Count - 1; i >= value.Count; i--)
            {
                VirtualNode oldNode = Nodes[i];
                Nodes.RemoveAt(i);
                oldNode.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates a getter function that retrieves the value at the specified array index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>A function that returns the element value at the given index.</returns>
    protected Func<object> CreateElementValueGetter(int index)
    {
        return () =>
        {
            var list = this.GetValue() as SArray;
            return list?[index];
        };
    }

    /// <summary>
    /// Creates a setter action that assigns a value at the specified array index,
    /// routing through the model's setter action system when available.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>An action that sets the element value at the given index.</returns>
    protected Action<object> CreateElementValueSetter(int index)
    {
        return (object value) =>
        {
            var model = FindModel();
            if (model != null)
            {
                model.HandleSetterAction(new EditorArrayValueSetterAction(this, index, value));
            }
            else
            {
                if (this.GetValue() is SArray sary)
                {
                    sary[index] = value;
                }
            }
        };
    }

    /// <inheritdoc/>
    public override string GetContextMenuKey()
    {
        return VirtualNode.ContextMenu_ArrayOwner;
    }

    /// <inheritdoc/>
    public override string GetChildContextMenuKey(VirtualNode childNode)
    {
        return VirtualNode.ContextMenu_ArrayMember;
    }

    /// <inheritdoc/>
    public override bool CanDropIn(VirtualNode node)
    {
        if (ReadOnly)
        {
            return false;
        }

        if (this.DisplayedValue is not SArray list)
        {
            return false;
        }

        bool nullable = list.GetParentField()?.Optional == true;
        return list.InputType.ElementType.SupportValue(node.DisplayedValue, nullable);
    }

    /// <summary>
    /// Gets the displayed <see cref="SArray"/> value cast from <see cref="DisplayedValue"/>.
    /// </summary>
    public SArray DisplayedArray => DisplayedValue as SArray;

    #region IVirtualNodeListOperation

    /// <inheritdoc/>
    public async Task<object> GuiCreateItemAsync(Type typeHint = null)
    {
        if (ReadOnly)
        {
            return null;
        }

        if (this.DisplayedValue is SArray list)
        {
            var innerType = list.InputType.ElementType;
            if (innerType.IsAbstract)
            {
                return await innerType.GuiCreateObject(list, innerType.ToDisplayString());
            }
            else
            {
                return list.InputType.ElementType.CreateDefaultValue();
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<object[]> GuiCreateItemsAsync(Type typeHint = null)
    {
        object obj = await GuiCreateItemAsync(typeHint);
        if (obj != null)
        {
            return [obj];
        }
        else
        {
            return [];
        }
    }

    /// <inheritdoc/>
    public bool CanAddItem(object value)
    {
        if (ReadOnly)
        {
            return false;
        }

        //return value is DValue;
        if (this.DisplayedValue is not SArray list)
        {
            return false;
        }

        bool nullable = list.GetParentField()?.Optional == true;
        return list.InputType.ElementType.SupportValue(value, nullable);
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            if (this.DisplayedValue is SArray list)
            {
                return list.Count;
            }
            else
            {
                return 0;
            }
        }
    }

    /// <inheritdoc/>
    public VirtualNode InsertListItem(int index, object value, bool config)
    {
        if (ReadOnly)
        {
            return null;
        }

        if (value is null)
        {
            return null;
        }

        var model = FindModel();

        if (this.DisplayedValue is not SArray list)
        {
            return null;
        }

        Type elementType = value.GetType();

        // Protection
        if (index < 0)
        {
            index = 0;
        }

        if (index > list.Count)
        {
            index = list.Count;
        }

        if (list != null)
        {
            model.PerformValueAction(new EditorArrayInsertSetterAction(this, index, value));

            VirtualNode newNode = model.CreateNode(elementType, this);
            this.Nodes.Insert(index, newNode);
            model.ConfigureNode(newNode);

            newNode.Getter = this.CreateElementValueGetter(index);
            newNode.Setter = this.CreateElementValueSetter(index);
            PerformGetValue();

            if (config)
            {
                SObject obj = value as SObject;
                obj?.GuiConfigObject(string.Empty);
                newNode.PerformGetValue();
            }

            return newNode;
        }
        else
        {
            return null;
        }
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
        model.PerformValueAction(new EditorArrayRemoveSetterAction(this, index));
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
            model.PerformValueAction(new EditorArrayRemoveSetterAction(this, index));
            // Synchronized removal
            Nodes.RemoveAt(index);
        }
        model.ResumeGetValue(false);

        PerformGetValue();
    }

    #endregion

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        if (_list?.Count > 0)
        {
            return TextStatus.Normal;
        }
        else
        {
            return Styles?.GetEmptyListGray() == true ? TextStatus.Disabled : TextStatus.Normal;
        }
    }

    /// <inheritdoc/>
    protected override Color? GetColor()
    {
        return _list?.InputType?.Target?.GetAttribute<IViewColor>()?.ViewColor;
    }

    /// <inheritdoc/>
    protected override Image GetMainIcon()
    {
        SArray ary = DisplayedArray;
        if (ary != null)
        {
            var icon = ary.GetIcon();
            if (icon != null)
            {
                return icon;
            }
        }

        return base.GetMainIcon();
    }

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _list = null;
    }
}
