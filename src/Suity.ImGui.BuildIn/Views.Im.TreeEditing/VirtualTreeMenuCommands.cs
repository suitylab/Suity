using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.VirtualTree;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Defines the operations available for creating items in a virtual tree.
/// </summary>
public enum VirtualCreateOp
{
    /// <summary>
    /// Add an item to the end of a list.
    /// </summary>
    Add,
    /// <summary>
    /// Insert an item before the current selection.
    /// </summary>
    Insert,
    /// <summary>
    /// Append an item after the current selection.
    /// </summary>
    Append,
}

#region VirtualTreeRootMenu
/// <summary>
/// Represents the root menu command for virtual tree views, providing common tree operations like add, delete, copy, paste, and navigation.
/// </summary>
internal class VirtualTreeRootMenu : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTreeRootMenu"/> class.
    /// </summary>
    /// <param name="name">The name of the menu.</param>
    public VirtualTreeRootMenu(string name)
        : base(name)
    {
        AddCommand(new VirtualTreeCreateMenu(VirtualCreateOp.Add, "Add", CoreIconCache.Add));
        AddCommand(new VirtualTreeCreateMenu(VirtualCreateOp.Insert, "Insert", CoreIconCache.Add));
        AddCommand(new VirtualTreeCreateMenu(VirtualCreateOp.Append, "Append", CoreIconCache.Add));

        AddCommand(new SimpleMenuCommand("Delete", CoreIconCache.Delete)
        {
            CheckPopStateAction = CheckAllParentIsListOp,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleItemRemove(),
        });

        AddSeparator();

        AddCommand(new SimpleMenuCommand("Go to Definition", CoreIconCache.GotoDefination)
        {
            AcceptOneItemOnly = true,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleGotoDefinition(),
        });

        AddCommand(new SimpleMenuCommand("Find References", CoreIconCache.Search)
        {
            AcceptOneItemOnly = true,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleFindReference(),
        });

        AddCommand(new SimpleMenuCommand("View Problems", CoreIconCache.Question)
        {
            AcceptOneItemOnly = true,
            CheckPopStateAction = CheckIsAnalysisResult,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleShowProblems(),
        }); ;

        AddSeparator();

        AddCommand(new SimpleMenuCommand("Copy", CoreIconCache.Copy)
        {
            CheckPopStateAction = CheckParentIsListOp,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleArraySetClipboard(true),
        });

        AddCommand(new SimpleMenuCommand("Cut", CoreIconCache.Cut)
        {
            CheckPopStateAction = CheckParentIsListOp,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleArraySetClipboard(false),
        });

        AddCommand(new SimpleMenuCommand("Paste", CoreIconCache.Paste)
        {
            CheckPopStateAction = CheckPasteIsListOp,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleArrayPaste(),
        });

        AddSeparator();

        AddCommand(new SimpleMenuCommand("Comment", CoreIconCache.Comment)
        {
            CheckPopStateAction = CanComment,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleComment(true),
        });

        AddCommand(new SimpleMenuCommand("Uncomment")
        {
            CheckPopStateAction = CanComment,
            CommandAction = cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleComment(false),
        });

        AddSeparator();

        AddCommand(new VirtualTreeAdvancedMenu());
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        Visible = selectionCount > 0;
    }

    private void CanComment(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        cmd.Visible = treeView.GetCanComment();
    }

    private void CheckIsAnalysisResult(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        cmd.Visible = (treeView.SelectedObjects.FirstOrDefault() as ISupportAnalysis)?.Analysis != null;
    }

    private void CheckIsListOp(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        cmd.Visible = treeView.SelectedNodes.FirstOrDefault() is IVirtualNodeListOperation;
    }

    private void CheckParentIsListOp(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        cmd.Visible = treeView.GetSelectedNodeParent() is IVirtualNodeListOperation;
    }

    private void CheckAllParentIsListOp(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        cmd.Visible = treeView.GetSelectedNodeParents().All(o => o is IVirtualNodeListOperation);
    }

    private void CheckPasteIsListOp(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView treeView)
        {
            cmd.Visible = false;
            return;
        }

        if (treeView.SelectedNodes.CountOne() && treeView.SelectedNodes.First() is IVirtualNodeListOperation)
        {
            cmd.Visible = true;
        }
        else if (treeView.GetSelectedNodeParent() is IVirtualNodeListOperation)
        {
            cmd.Visible = true;
        }
        else
        {
            cmd.Visible = false;
        }
    }
}

#endregion

#region VirtualTreeCreateMenu

/// <summary>
/// Represents a menu command for creating new items in a virtual tree, supporting add, insert, and append operations.
/// </summary>
internal class VirtualTreeCreateMenu : MenuCommand
{
    readonly VirtualCreateOp _op;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTreeCreateMenu"/> class.
    /// </summary>
    /// <param name="op">The create operation type.</param>
    /// <param name="text">The display text for the menu item.</param>
    /// <param name="icon">The icon for the menu item.</param>
    public VirtualTreeCreateMenu(VirtualCreateOp op, string text, Image icon)
        : base(text, icon)
    {
        _op = op;

        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        Clear();

        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        if (Sender is not ImGuiVirtualTreeView treeView)
        {
            Visible = false;
            return;
        }

        switch (_op)
        {
            case VirtualCreateOp.Add:
                Visible = treeView.SelectedNodes.FirstOrDefault() is IVirtualNodeListOperation;
                break;

            case VirtualCreateOp.Insert:
            case VirtualCreateOp.Append:
                Visible = treeView.GetSelectedNodeParent() is IVirtualNodeListOperation;
                break;
        }

        BuildDropDown();
    }

    private void BuildDropDown()
    {
        Clear();

        if (ResolveGui() is not { } gui)
        {
            return;
        }

        var options = gui.CreationOptions;
        if (options is null)
        {
            return;
        }

        foreach (var option in options.SkipNull())
        {
            AddCommand(new CreationItemCommand(_op, option));
        }
    }

    /// <summary>
    /// Resolves the object creation GUI from the current tree view selection.
    /// </summary>
    /// <returns>The resolved <see cref="IHasObjectCreationGUI"/> or null if not available.</returns>
    public IHasObjectCreationGUI? ResolveGui()
    {
        if (Sender is not ImGuiVirtualTreeView treeView)
        {
            return null;
        }

        switch (_op)
        {
            case VirtualCreateOp.Add:
                return treeView.SelectedNode?.DisplayedValue as IHasObjectCreationGUI;

            case VirtualCreateOp.Insert:
            case VirtualCreateOp.Append:
                return treeView.SelectedNode?.Parent?.DisplayedValue as IHasObjectCreationGUI;

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (ChildCommandCount > 0)
        {
            return;
        }

        try
        {
            switch (_op)
            {
                case VirtualCreateOp.Add:
                    (Sender as ImGuiVirtualTreeView)?.HandleArrayAdd();
                    break;

                case VirtualCreateOp.Insert:
                    (Sender as ImGuiVirtualTreeView)?.HandleItemInsert(false);
                    break;

                case VirtualCreateOp.Append:
                    (Sender as ImGuiVirtualTreeView)?.HandleItemInsert(true);
                    break;
            }
        }
        catch (Exception err)
        {
            err.LogError(L("Create item failed"));
        }
    }
}

#endregion

#region VirtualTreeAdvancedMenu
/// <summary>
/// Represents an advanced menu for virtual tree views, providing XML/Json copy, paste, edit, export, and repair operations.
/// </summary>
internal class VirtualTreeAdvancedMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTreeAdvancedMenu"/> class.
    /// </summary>
    public VirtualTreeAdvancedMenu()
        : base("Advanced", CoreIconCache.Setting)
    {
        AcceptOneItemOnly = true;

        AddCommand("Copy XML", icon: CoreIconCache.Copy,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleCopyText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));
        AddCommand("Paste XML", icon: CoreIconCache.Paste,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandlePasteText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));
        AddCommand("Edit XML", icon: CoreIconCache.Edit,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleEditText(ViewAdvancedEditFeatures.XML),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.XML));

        AddSeparator();

        AddCommand("Copy Json", icon: CoreIconCache.Copy,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleCopyText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));
        AddCommand("Paste Json", icon: CoreIconCache.Paste,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandlePasteText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));
        AddCommand("Edit Json", icon: CoreIconCache.Edit,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleEditText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));
        AddCommand("Export Json", icon: CoreIconCache.Export,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleExportText(ViewAdvancedEditFeatures.Json),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Json));

        AddSeparator();

        AddCommand("Repair", icon: CoreIconCache.Setting,
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleRepair(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));
        AddCommand("Relocate",
            action: cmd => (cmd.Sender as ImGuiVirtualTreeView)?.HandleRelocate(),
            checkPopState: (cmd, _, _, _) => CheckPopFeature(cmd, ViewAdvancedEditFeatures.Repair));
    }

    private void CheckPopFeature(MenuCommand cmd, ViewAdvancedEditFeatures feature)
    {
        if (cmd.Sender is not ImGuiVirtualTreeView tree)
        {
            cmd.Visible = false;
            return;
        }

        IViewAdvancedEdit? r = tree.SelectedNodes.FirstOrDefault() as IViewAdvancedEdit;

        cmd.Visible = r?.GetHasFeature(feature) == true;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        if (Sender is not ImGuiVirtualTreeView tree)
        {
            Visible = false;
            return;
        }

        IViewAdvancedEdit? r = tree.SelectedNodes.FirstOrDefault() as IViewAdvancedEdit;
        Visible = r != null;
    }
}
#endregion

#region CreationMenuItem
/// <summary>
/// Represents a menu command for creating a specific item type in a virtual tree.
/// </summary>
internal class CreationItemCommand : MenuCommand
{
    readonly VirtualCreateOp _op;
    readonly ObjectCreationOption _option;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreationItemCommand"/> class.
    /// </summary>
    /// <param name="op">The create operation type.</param>
    /// <param name="option">The object creation option containing type and display information.</param>
    public CreationItemCommand(VirtualCreateOp op, ObjectCreationOption option)
        : base(option.Text, option.Type.ToDisplayIcon())
    {
        _op = op;
        _option = option;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        try
        {
            switch (_op)
            {
                case VirtualCreateOp.Add:
                    (Sender as ImGuiVirtualTreeView)?.HandleArrayAdd(_option);
                    break;

                case VirtualCreateOp.Insert:
                    (Sender as ImGuiVirtualTreeView)?.HandleItemInsert(false, _option);
                    break;

                case VirtualCreateOp.Append:
                    (Sender as ImGuiVirtualTreeView)?.HandleItemInsert(true, _option);
                    break;
            }
        }
        catch (Exception err)
        {
            err.LogError(L("Create item failed") + ": " + _option.Type.Name);
        }
    }
} 
#endregion