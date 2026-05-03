using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Design;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using Suity.Drawing;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// An abstract base class for virtual tree nodes that represent objects with properties.
/// Provides infrastructure for displaying, editing, and synchronizing object properties,
/// and implements <see cref="ISyncContext"/> for synchronization support.
/// </summary>
public abstract class BaseObjectNode : VirtualNode, ISyncContext
{
    private int _setupNodeIndex = -1;
    private VirtualNode[] _setupNodes;
    private bool _updating;

    #region Editor

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        base.OnGetValue();

        if (_updating)
        {
            throw new InvalidOperationException();
        }

        if (!IsContentInitialized)
        {
            return;
        }

        try
        {
            _updating = true;

            _setupNodeIndex = 0;
            _setupNodes = [.. Nodes];

            OnSetupObjectNode();

            if (_setupNodeIndex < _setupNodes.Length)
            {
                for (int i = _setupNodeIndex; i < _setupNodes.Length; i++)
                {
                    this.Nodes.Remove(_setupNodes[i]);
                }
            }
        }
        finally
        {
            _setupNodeIndex = -1;
            _setupNodes = null;
            _updating = false;
        }
    }

    /// <summary>
    /// Called during value retrieval to set up the object node's child properties.
    /// Override this method to define the properties and fields to display.
    /// </summary>
    protected virtual void OnSetupObjectNode()
    { }

    /// <summary>
    /// Gets the value of a property by name. Override to provide custom property retrieval.
    /// </summary>
    /// <param name="name">The name of the property to get.</param>
    /// <returns>The property value, or null if not found.</returns>
    protected internal virtual object OnGetProperty(string name)
    {
        return null;
    }

    /// <summary>
    /// Sets the value of a property by name. Override to provide custom property setting.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="obj">The value to set.</param>
    protected internal virtual void OnSetProperty(string name, object obj)
    {
    }

    #endregion

    #region Display

    /// <inheritdoc/>
    protected override string GetText()
    {
        bool editable = DisplayedValue is ITextEdit edit && edit.CanEditText;

        var obj = DisplayedValue;
        string result;

        if (editable)
        {
            // If editable, display as raw text
            result = (obj as ITextDisplay)?.DisplayText ?? PropertyName;
        }
        else
        {
            // If not editable, display as localized text
            result = obj?.ToDisplayTextL(PropertyName);
        }

        if (result is null && obj != null)
        {
            result = obj.ToString();
        }

        return result;
    }

    /// <inheritdoc/>
    protected override void SetText(string value)
    {
        if (DisplayedValue is ITextEdit edit && edit.CanEditText)
        {
            edit.SetText(value, this);
        }
    }

    /// <inheritdoc/>
    protected override bool GetCanEditText()
    {
        return DisplayedValue is ITextEdit ext ? ext.CanEditText : base.GetCanEditText();
    }

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        TextStatus status = (DisplayedValue as ITextDisplay)?.DisplayStatus ?? base.GetTextStatus();
        if (DisplayedValue is IViewComment { IsComment: true } && status < TextStatus.Comment)
        {
            status = TextStatus.Comment;
        }

        return status;
    }

    /// <inheritdoc/>
    protected override Color? GetColor()
    {
        return (DisplayedValue as IViewColor)?.ViewColor;
    }

    /// <inheritdoc/>
    protected override ImageDef GetMainIcon()
    {
        return DisplayedValue is ITextDisplay ext ? EditorUtility.GetIcon(ext.DisplayIcon) : base.GetMainIcon();
    }

    /// <inheritdoc/>
    protected override ImageDef GetCustomIcon()
    {
        return DisplayedValue is ICustomIcon customIcon ? customIcon.CustomIcon : base.GetCustomIcon();
    }

    /// <inheritdoc/>
    protected override string GetDescription()
    {
        IDescriptionDisplay display = DisplayedValue as IDescriptionDisplay;

        return display?.Description ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override string GetPreviewText()
    {
        string preview = DisplayedValue is IPreviewDisplay ext ? ext.PreviewText : base.GetPreviewText();

        return preview ?? string.Empty;
    }

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        if (DisplayedValue is IMember)
        {
            return;
        }

        IPreviewEdit edit = DisplayedValue as IPreviewEdit;
        if (edit?.CanEditPreviewText == true)
        {
            edit.SetPreviewText(value, null);
        }
    }

    /// <inheritdoc/>
    protected override bool GetCanEditPreviewText()
    {
        return DisplayedValue is IPreviewEdit ext ? ext.CanEditPreviewText : base.CanEditPreviewText;
    }

    /// <inheritdoc/>
    protected override TextStatus GetPreviewTextStatus()
    {
        return TextStatus.Disabled;
    }

    /// <inheritdoc/>
    protected override ImageDef GetPreviewIcon()
    {
        var customIcon = GetCustomPreviewIcon();
        if (customIcon != null)
        {
            return customIcon;
        }


        return DisplayedValue is IPreviewDisplay ext ? EditorUtility.GetIcon(ext.PreviewIcon) : base.GetPreviewIcon();
    }

    /// <inheritdoc/>
    protected override string GetFieldDisplayName()
    {
        return string.Empty;
    }

    #endregion

    #region Field

    /// <summary>
    /// Adds a simple text field node with the specified display text and optional icons.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="text">The display text.</param>
    /// <param name="icon">Optional main icon for the field.</param>
    /// <param name="previewText">Optional preview text.</param>
    /// <param name="previewIcon">Optional preview icon.</param>
    protected void FieldSimpleText(string name, string text, ImageDef icon = null, string previewText = null, ImageDef previewIcon = null)
    {
        var node = new SimpleTextNode(text, icon, previewText, previewIcon);
        InternalAddNode(node, new ViewProperty(name));
    }

    /// <summary>
    /// Adds a ToStringNode field for displaying the object's string representation.
    /// Only adds if the view ID matches or is unspecified.
    /// </summary>
    /// <param name="property">The view property configuration.</param>
    protected void FieldToString(ViewProperty property)
    {
        if (property.ViewId == 0 || property.ViewId == this.ViewId)
        {
            var node = new ToStringNode();
            InternalAddNode(node, property);
        }
    }

    /// <summary>
    /// Adds a StringNode field for displaying and editing string values.
    /// Only adds if the view ID matches or is unspecified.
    /// </summary>
    /// <param name="property">The view property configuration.</param>
    protected void FieldString(ViewProperty property)
    {
        if (property.ViewId == 0 || property.ViewId == this.ViewId)
        {
            var node = new StringNode();
            InternalAddNode(node, property);
        }
    }

    /// <summary>
    /// Adds a field node of the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="property">The view property configuration.</param>
    protected void FieldOfType<T>(ViewProperty property)
    {
        InternalAddNode(typeof(T), property);
    }

    /// <summary>
    /// Adds a field node of the specified type.
    /// </summary>
    /// <param name="editedType">The type to edit.</param>
    /// <param name="property">The view property configuration.</param>
    protected void FieldOfType(Type editedType, ViewProperty property)
    {
        InternalAddNode(editedType, property);
    }

    /// <summary>
    /// Adds a field node with the specified value and type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The field value.</param>
    /// <param name="property">The view property configuration.</param>
    protected void Field<T>(T value, ViewProperty property)
    {
        InternalAddNode(typeof(T), property);
    }

    /// <summary>
    /// Performs the set value action for a named property. Override to customize property setting behavior.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    protected internal virtual void PerformSetValueAction(string name, object value)
    {
    }

    #endregion

    #region AddNode

    /// <summary>
    /// Internally adds a node of the specified type with the given property configuration.
    /// Reuses existing nodes when possible to preserve state.
    /// </summary>
    /// <param name="editedType">The type to edit.</param>
    /// <param name="property">The view property configuration.</param>
    internal void InternalAddNode(Type editedType, ViewProperty property)
    {
        if (!IsViewIdSupported(property.ViewId))
        {
            return;
        }

        if (_setupNodes is null)
        {
            return;
        }

        VirtualNode node = null;

        ImageDef icon = EditorUtility.GetIcon(property.Icon);

        if (_setupNodeIndex < _setupNodes.Length)
        {
            if (_setupNodes[_setupNodeIndex].EditedType == editedType)
            {
                node = _setupNodes[_setupNodeIndex];
                node.PropertyName = property.Name;
                node.PropertyDescription = L(property.Description);
                node.PropertyStatus = property.Status;
                node.Icon = icon;
                node.ReadOnly = property.ReadOnly;
                node.Getter = CreateGetter(property.Name);
                node.Setter = CreateSetter(property.Name);
                _setupNodeIndex++;
            }
        }

        if (node is null)
        {
            node = FindModel().CreateNode(editedType, this);
            node.PropertyDescription = L(property.Description);
            node.PropertyStatus = property.Status;
            node.Icon = icon;
            node.ReadOnly = property.ReadOnly;

            AddNode(node, property.Name);
        }

        node.PerformGetValue();
    }

    /// <summary>
    /// Internally adds a pre-created node with the given property configuration.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="property">The view property configuration.</param>
    internal void InternalAddNode(VirtualNode node, ViewProperty property)
    {
        if (property.ViewId != 0 && property.ViewId != this.ViewId)
        {
            return;
        }

        if (_setupNodes is null)
        {
            return;
        }

        ImageDef icon = null;
        if (property.Icon is ImageDef image)
        {
            icon = image;
        }
        else if (property.Icon is string)
        {
            icon = EditorUtility.GetIcon(property.Icon);
        }

        node.PropertyDescription = L(property.Description);
        node.PropertyStatus = property.Status;
        node.Icon = icon;
        node.ReadOnly = property.ReadOnly;
        AddNode(node, property.Name);

        node.PerformGetValue();
    }

    /// <summary>
    /// Sets the value of this node directly. Used for internal operations.
    /// </summary>
    /// <param name="value">The value to set.</param>
    internal void InternalSetValue(object value)
    {
        this.SetValue(value);
    }

    /// <summary>
    /// Adds a child node with the specified name, configuring its getter and setter.
    /// Reuses existing nodes when possible to preserve editor state.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="name">The property name.</param>
    private void AddNode(VirtualNode node, string name)
    {
        node.PropertyName = name;
        node.Getter = CreateGetter(name);
        node.Setter = CreateSetter(name);

        if (_setupNodeIndex < _setupNodes.Length)
        {
            VirtualNode oldEditor = _setupNodes[_setupNodeIndex];
            this.Nodes.Add(node, oldEditor);
            this.Nodes.Remove(oldEditor);
        }
        else
        {
            this.Nodes.Add(node);
        }

        var model = FindModel();
        model?.ConfigureNode(node);

        _setupNodeIndex++;
    }

    /// <summary>
    /// Creates a getter function that retrieves a property value by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>A function that returns the property value.</returns>
    private Func<object> CreateGetter(string name)
    {
        return new Func<object>(() => OnGetProperty(name));
    }

    /// <summary>
    /// Creates a setter action that sets a property value by name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>An action that sets the property value.</returns>
    private Action<object> CreateSetter(string name)
    {
        return new Action<object>(obj => PerformSetValueAction(name, obj));
    }

    #endregion

    #region ISyncContext

    /// <inheritdoc/>
    object ISyncContext.Parent => ParentValue;

    /// <inheritdoc/>
    object IServiceProvider.GetService(Type serviceType) => GetService(serviceType);

    #endregion
}

/// <summary>
/// A generic variant of <see cref="BaseObjectNode"/> that provides typed access to the displayed object.
/// </summary>
/// <typeparam name="T">The type of the displayed object.</typeparam>
public abstract class BaseObjectNode<T> : BaseObjectNode
{
    /// <summary>
    /// Gets the displayed value cast to type <typeparamref name="T"/>, or the default value if cast fails.
    /// </summary>
    public T DisplayedObject => DisplayedValue is T tObj ? tObj : default;

    /// <summary>
    /// Gets the underlying value cast to type <typeparamref name="T"/>, or the default value if cast fails.
    /// </summary>
    /// <returns>The value as type <typeparamref name="T"/>, or default.</returns>
    protected T GetObject() => GetValue() is T tObj ? tObj : default;
}
