using Suity.Editor;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Views.Im.PropertyEditing.ViewObjects;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views.Im.PropertyEditing;

#region BaseDesignMenu

/// <summary>
/// Base class for design-related menu commands in the property grid.
/// Validates that the selected target is a <see cref="SItem"/> or <see cref="DesignValue"/> type before becoming visible.
/// </summary>
internal abstract class BaseDesignMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignMenu"/> class.
    /// </summary>
    protected BaseDesignMenu()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignMenu"/> class with the specified text and optional icon.
    /// </summary>
    /// <param name="text">The display text for the menu item.</param>
    /// <param name="icon">The optional icon for the menu item.</param>
    protected BaseDesignMenu(string text, Image? icon = null) : base(text, icon)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignMenu"/> class with the specified key, text, and optional icon.
    /// </summary>
    /// <param name="key">The command key.</param>
    /// <param name="text">The display text for the menu item.</param>
    /// <param name="icon">The optional icon for the menu item.</param>
    protected BaseDesignMenu(string key, string text, Image? icon = null) : base(key, text, icon)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        if (Sender is not IPropertyGrid grid)
        {
            Visible = false;
            return;
        }

        var target = grid.GridData.SelectedField?.Target;

        Type? type = target?.EditedType;
        if (type is null)
        {
            Visible = false;

            return;
        }

        Visible = typeof(SItem).IsAssignableFrom(type) || typeof(DesignValue).IsAssignableFrom(type);
    }
}

#endregion

/// <summary>
/// Root menu command for the property grid, containing array operations, navigation, clipboard, and advanced menus.
/// </summary>
internal class PropertyGridRootMenu : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridRootMenu"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the root menu.</param>
    public PropertyGridRootMenu(string name)
        : base(name)
    {
        AddCommand(new PropertyGridArrayMenu());

        AddSeparator();

        AddCommand("Go to Definition", icon: CoreIconCache.GotoDefination,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleGotoDefinition());
        AddCommand(new GotoFieldDefinitionMenu());
        AddCommand("Find References", icon: CoreIconCache.Search,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleFindReference());

        AddSeparator();

        AddCommand(new CopyMenu());
        AddCommand(new PasteMenu());

        AddSeparator();

        AddCommand(new PropertyGridPreviewMenu());
        AddCommand(new PropertyGridAdvancedMenu());
        AddCommand(new PropertyGridDynamicMenu());
        AddCommand(new OperateMenu());
    }
}

/// <summary>
/// Menu command for array element operations such as remove, clone, and reorder.
/// Visible only when the selected field's parent has an array target.
/// </summary>
internal class PropertyGridArrayMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridArrayMenu"/> class.
    /// </summary>
    public PropertyGridArrayMenu()
        : base("Array", "Array", CoreIconCache.Array)
    {
        AddCommand("Remove", icon: CoreIconCache.Delete,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleArrayOp(ArrayElementOp.Delete));
        AddCommand("Clone", icon: CoreIconCache.Copy,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleArrayOp(ArrayElementOp.Clone));
        AddSeparator();
        AddCommand("Move Up", icon: CoreIconCache.Up,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleArrayOp(ArrayElementOp.MoveUp));
        AddCommand("Move Down", icon: CoreIconCache.Down,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleArrayOp(ArrayElementOp.MoveDown));
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        if (Sender is not IPropertyGrid grid)
        {
            Visible = false;
            return;
        }

        var target = grid.GridData.SelectedField?.Target;

        Visible = target?.Parent?.ArrayTarget != null;
    }
}

/// <summary>
/// Menu command that navigates to the definition of the selected field.
/// </summary>
internal class GotoFieldDefinitionMenu : BaseDesignMenu
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoFieldDefinitionMenu"/> class.
    /// </summary>
    public GotoFieldDefinitionMenu()
        : base("Go to Field Definition", CoreIconCache.GotoDefination)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as IPropertyGrid)?.HandleGotoFieldDefinition();
    }
}

/// <summary>
/// Menu command that copies the selected property grid content.
/// </summary>
internal class CopyMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CopyMenu"/> class.
    /// </summary>
    public CopyMenu()
        : base("Copy", CoreIconCache.Copy)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as IPropertyGrid)?.HandleCopy();
    }
}

/// <summary>
/// Menu command that pastes content into the selected property grid field.
/// </summary>
internal class PasteMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasteMenu"/> class.
    /// </summary>
    public PasteMenu()
        : base("Paste", CoreIconCache.Paste)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        (Sender as IPropertyGrid)?.HandlePaste();
    }
}

/// <summary>
/// Menu command for preview-related operations such as adding, removing, and clearing previews.
/// Visible only when an <see cref="IColumnPreview"/> service is available.
/// </summary>
internal class PropertyGridPreviewMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridPreviewMenu"/> class.
    /// </summary>
    public PropertyGridPreviewMenu()
        : base("Preview", CoreIconCache.Preview)
    {
        AddCommand("Add Preview", icon: CoreIconCache.Preview,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleAddPreview());
        AddCommand("Remove Preview",
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleRemovePreview());
        AddSeparator();
        AddCommand("Remove All", icon: CoreIconCache.Remove,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleRemoveAllPreview());
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        var grid = Sender as IPropertyGrid;
        var preview = grid?.Context?.GetService<IColumnPreview>();

        Visible = preview != null;
    }
}

/// <summary>
/// Menu command for advanced edit operations including XML/JSON copy, paste, and edit, as well as repair functionality.
/// </summary>
internal class PropertyGridAdvancedMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridAdvancedMenu"/> class.
    /// </summary>
    public PropertyGridAdvancedMenu()
        : base("Convert", CoreIconCache.Setting)
    {
        AddCommand("Copy XML", icon: CoreIconCache.Copy,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleCopyText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));
        AddCommand("Paste XML", icon: CoreIconCache.Paste,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandlePasteText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));
        AddCommand("Edit XML", icon: CoreIconCache.Edit,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleEditText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));

        AddSeparator();

        AddCommand("Copy Json", icon: CoreIconCache.Copy,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleCopyText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));
        AddCommand("Paste Json", icon: CoreIconCache.Paste,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandlePasteText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));
        AddCommand("Edit Json", icon: CoreIconCache.Edit,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleEditText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));

        AddSeparator();

        //RegisterSimpleCommand("Go to Field Definition", icon: CoreIconCache.GotoDefination,
        //    action: cmd => (cmd.Sender as IPropertyGrid)?.HandleGotoFieldDefinition());
        AddCommand("Repair", icon: CoreIconCache.Setting,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleRepair(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));
        //RegisterSimpleCommand("Redirect",
        //    action: cmd => (cmd.Sender as IPropertyGrid)?.HandleRelocate());
    }

    /// <summary>
    /// Checks and sets the visibility of a menu command based on the specified feature.
    /// </summary>
    /// <param name="cmd">The menu command to check.</param>
    /// <param name="feature">The advanced edit feature to verify.</param>
    private void CheckPopFeature(MenuCommand cmd, ViewAdvancedEditFeatures feature)
    {
        cmd.Visible = true;

        //var grid = cmd.Sender as IPropertyGrid;
        //if (grid is null)
        //{
        //    cmd.Visible = false;
        //    return;
        //}

        //IViewAdvancedEdit? r = grid.GridData.SelectedField?.Target?.GetValues().FirstOrDefault() as IViewAdvancedEdit;

        //cmd.Visible = r?.GetHasFeature(feature) == true;
    }
}

/// <summary>
/// Menu command for dynamic binding operations.
/// Dynamically populates commands from all types derived from <see cref="SDynamic"/> and allows canceling the dynamic action.
/// Visible only when the selected target type is assignable from <see cref="SItem"/>.
/// </summary>
internal class PropertyGridDynamicMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyGridDynamicMenu"/> class.
    /// Discovers and registers commands for all derived <see cref="SDynamic"/> types.
    /// </summary>
    public PropertyGridDynamicMenu()
        : base("Dynamic Binding", CoreIconCache.Dynamic)
    {
        foreach (var type in typeof(SDynamic).GetDerivedTypes())
        {
            SDynamic? d = null;

            try
            {
                d = (SDynamic)type.CreateInstanceOf();
            }
            catch (Exception err)
            {
                err.LogError();
                continue;
            }

            if (d is null)
            {
                continue;
            }

            AddCommand(d.GetType().ToDisplayText(), icon: d.Icon, action: CreateCommand(type));
        }

        AddSeparator();
        AddCommand("Cancel", icon: CoreIconCache.Disable, action: cmd =>
        {
            (cmd.Sender as IPropertyGrid)?.HandleSetDynamcAction(null);
        });
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        if (Sender is not IPropertyGrid grid)
        {
            Visible = false;
            return;
        }

        var target = grid.GridData.SelectedField?.Target;

        Type? type = target?.EditedType;
        if (type is null)
        {
            Visible = false;
            return;
        }

        Visible = typeof(SItem).IsAssignableFrom(type);
    }

    /// <summary>
    /// Creates an action that sets the dynamic action type on the property grid.
    /// </summary>
    /// <param name="type">The dynamic type to set.</param>
    /// <returns>An action that executes the dynamic binding command.</returns>
    private Action<MenuCommand> CreateCommand(Type type)
    {
        return cmd =>
        {
            (cmd.Sender as IPropertyGrid)?.HandleSetDynamcAction(type);
        };
    }
}

/// <summary>
/// Menu command for miscellaneous operations such as filling random values, replacing strings,
/// filling from resources, and removing attachments.
/// </summary>
internal class OperateMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperateMenu"/> class.
    /// </summary>
    public OperateMenu()
        : base("Operations", CoreIconCache.Action)
    {
        AddCommand("Fill Random Value", icon: CoreIconCache.Random,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleFillRandomValue(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));

        AddCommand("Replace String", icon: CoreIconCache.Edit,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleReplace(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));

        AddCommand("Fill from Resource", icon: CoreIconCache.Asset,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleFillFromAsset(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));

        AddCommand("Remove All Attachments", icon: CoreIconCache.Attachment,
            action: cmd => (cmd.Sender as IPropertyGrid)?.HandleRemoveAllAttachments(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));
    }

    /// <summary>
    /// Checks and sets the visibility of a menu command based on the specified feature.
    /// </summary>
    /// <param name="cmd">The menu command to check.</param>
    /// <param name="feature">The advanced edit feature to verify.</param>
    private void CheckPopFeature(MenuCommand cmd, ViewAdvancedEditFeatures feature)
    {
        cmd.Visible = true;

        //var grid = cmd.Sender as IPropertyGrid;
        //if (grid is null)
        //{
        //    cmd.Visible = false;
        //    return;
        //}

        //IViewAdvancedEdit? r = grid.GridData.SelectedField?.Target?.GetValues().FirstOrDefault() as IViewAdvancedEdit;

        //cmd.Visible = r?.GetHasFeature(feature) == true;
    }
}
