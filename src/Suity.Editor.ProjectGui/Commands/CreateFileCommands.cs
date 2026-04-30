using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Represents a category menu command that groups file creation commands by category.
/// </summary>
internal class CategoryMenuCommand : MenuCommand
{
    /// <summary>
    /// Contains display information for a category menu.
    /// </summary>
    /// <param name="Key">The unique key for the category.</param>
    /// <param name="Text">The display text for the category.</param>
    /// <param name="Icon">The icon to display for the category.</param>
    /// <param name="Order">The sort order for the category.</param>
    public record CategoryInfo(string Key, string Text, Image Icon, int Order);

    private static Lazy<Dictionary<string, CategoryInfo>> _categoryDisplays = new(CreateCategoryDisplays);

    private static Dictionary<string, CategoryInfo> CreateCategoryDisplays()
    {
        Dictionary<string, CategoryInfo> d = [];
        d["AIGC"] = new("AIGC", "AIGC", CoreIconCache.Aigc, 1000);
        d["Type"] = new("Type", "Type", CoreIconCache.Box, 900);
        d["Data"] = new("Data", "Data", CoreIconCache.Data, 800);
        d["Render"] = new("Render", "Render", CoreIconCache.Render, 700);
        d["Control"] = new("Control", "Control", CoreIconCache.Controller, 600);
        d["Value"] = new("Value", "Value", CoreIconCache.Value, 500);
        d["Fomula"] = new("Fomula", "Fomula", CoreIconCache.Function, 400);
        d["System"] = new("System", "System", CoreIconCache.System, 300);
        d["Misc"] = new("Misc", "Misc", null, 200);

        return d;
    }

    /// <summary>
    /// Gets the category information for this menu command.
    /// </summary>
    public CategoryInfo Info { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMenuCommand"/> class with category info.
    /// </summary>
    /// <param name="info">The category information.</param>
    public CategoryMenuCommand(CategoryInfo info)
         : base(info.Key, info.Text, info.Icon)
    {
        Info = info;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMenuCommand"/> class.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="icon">The icon.</param>
    public CategoryMenuCommand(string text, Image icon)
        : base(text, icon)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMenuCommand"/> class.
    /// </summary>
    /// <param name="key">The unique key.</param>
    /// <param name="text">The display text.</param>
    /// <param name="icon">The icon.</param>
    public CategoryMenuCommand(string key, string text, Image icon)
        : base(key, text, icon)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMenuCommand"/> class.
    /// </summary>
    /// <param name="key">The unique key and display text.</param>
    public CategoryMenuCommand(string key)
        : base(key, key)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryMenuCommand"/> class.
    /// </summary>
    /// <param name="key">The unique key.</param>
    /// <param name="text">The display text.</param>
    public CategoryMenuCommand(string key, string text)
        : base(key, text)
    {
    }

    /// <summary>
    /// Gets an existing sub-menu with the specified key, or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="key">The key of the sub-menu to find or create.</param>
    /// <returns>The existing or newly created <see cref="CategoryMenuCommand"/>.</returns>
    public CategoryMenuCommand GetOrCreateMenu(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return this;
        }

        var menu = FindCommand<CategoryMenuCommand>(key);
        if (menu is null)
        {
            if (_categoryDisplays.Value.GetValueSafe(key) is { } info)
            {
                menu = new CategoryMenuCommand(info);
            }
            else
            {
                menu = new CategoryMenuCommand(key);
            }

            AddCommand(menu);
        }

        return menu;
    }
}

/// <summary>
/// Command that groups all file creation options into a categorized menu.
/// </summary>
internal class CreateFileGroupCommand : CategoryMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFileGroupCommand"/> class,
    /// populating it with all available document formats and folder creation options.
    /// </summary>
    public CreateFileGroupCommand()
        : base("Create", CoreIconCache.New.ToIconSmall())
    {
        Dictionary<string, MenuCommand> _categories = [];
        List<CreateFileCommand> miscCmds = [];

        foreach (DocumentFormat format in DocumentManager.Instance.GetDocumentFormats().OrderByDescending(o => o.Order))
        {
            if (!format.CanCreate)
            {
                continue;
            }

            if (format.FormatNames.Contains(format.FormatName))
            {
                continue;
            }

            if (format.DocumentType.HasAttributeCached<NotAvailableAttribute>())
            {
                continue;
            }

            string category = format.Category;
            if (!string.IsNullOrWhiteSpace(category))
            {
                string[] cChain = category.Split('/');

                CategoryMenuCommand menu = this;
                foreach (var key in cChain)
                {
                    menu = menu.GetOrCreateMenu(key);
                }

                menu.AddCommand(new CreateFileCommand(format));
            }
            else
            {
                miscCmds.Add(new CreateFileCommand(format));
            }
        }

        AcceptType<AssetRootNode>(false);
        AcceptType<AssetDirectoryNode>(false);
        //RegisterAcceptedTypes<WorkSpaceRootNode>();

        AcceptOneItemOnly = true;

        List<MenuBase> cmds = [.. this.ChildCommands];
        Clear();
        cmds.Sort((a, b) =>
        {
            int orderA = (a as CategoryMenuCommand)?.Info?.Order ?? 0;
            int orderB = (b as CategoryMenuCommand)?.Info?.Order ?? 0;

            return -orderA.CompareTo(orderB);
        });

        AddCommand(new CreateFolderCommand());
        AddSeparator();

        if (miscCmds.Count > 0)
        {
            foreach (var cmd in miscCmds)
            {
                AddCommand(cmd);
            }

            AddSeparator();
        }

        foreach (var command in cmds)
        {
            AddCommand(command);
        }
    }

    /// <inheritdoc/>
    protected override void OnDropDown()
    {
        foreach (var subCommand in ChildCommands.OfType<CreateFileCommand>())
        {
            subCommand.UpdateIcon();
        }
    }
}

/// <summary>
/// Command that creates a new folder in the selected directory.
/// </summary>
internal class CreateFolderCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFolderCommand"/> class.
    /// </summary>
    public CreateFolderCommand()
        : base("Folder", CoreIconCache.Folder.ToIconSmall())
    {
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var dirNode = view.SelectedDirectory;
        if (dirNode is null)
        {
            return;
        }

        string name = GetAvailableFolderName(dirNode);
        //name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Enter folder name", name, str =>
        //{
        //    string dirNameTest = Path.Combine(dirNode.NodePath, str);
        //    return !Directory.Exists(dirNameTest);
        //});

        //if (string.IsNullOrEmpty(name))
        //{
        //    return;
        //}

        string dirName = Path.Combine(dirNode.NodePath, name);

        try
        {
            Directory.CreateDirectory(dirName);
            Thread.Sleep(100);

            dirNode.PopulateUpdate();

            if (view.Model.GetNode(dirName) is DirectoryNode newDirNode)
            {
                view.SelectNode(newDirNode, true);
            }
        }
        catch (Exception err)
        {
            DialogUtility.ShowMessageBoxAsync(L("Folder creation failed") + ": " + name);
            EditorUtility.ShowError(L("Folder creation failed") + ": " + dirName, err);
        }
    }

    private string GetAvailableFolderName(DirectoryNode dirNode)
    {
        int index = 1;
        while (true)
        {
            string name = "Folder" + index;
            string dirName = Path.Combine(dirNode.NodePath, name);
            if (!Directory.Exists(dirName))
            {
                return name;
            }
            else
            {
                index++;
            }
        }
    }
}

/// <summary>
/// Command that creates a new file of a specific document format.
/// </summary>
internal class CreateFileCommand : MenuCommand
{
    private readonly DocumentFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFileCommand"/> class.
    /// </summary>
    /// <param name="format">The document format to use for the new file.</param>
    public CreateFileCommand(DocumentFormat format)
        : base(format.FormatName, format.DisplayText, format.Icon)
    {
        _format = format ?? throw new ArgumentNullException(nameof(format));
    }

    /// <summary>
    /// Gets the document format associated with this command.
    /// </summary>
    public DocumentFormat Formst => _format;

    /// <summary>
    /// Updates the icon for this command based on the document format.
    /// </summary>
    public void UpdateIcon()
    {
        Icon = _format.Icon;
        Icon ??= EditorUtility.GetIconForFileExact("some." + _format.GetAdditionalExtensions()[0])?.ToIconSmall();
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        //var cap = _format.DocumentType?.GetAttributeCached<EditorCapabilityAttribute>();
        //if (cap != null && !ServiceInternals._license.GetCapability(cap.Capability))
        //{
        //    this.Enabled = false;
        //}
        //else
        //{
        //    this.Enabled = true;
        //}
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var directory = view.SelectedDirectory;
        if (directory is null)
        {
            return;
        }

        //var cap = _format.DocumentType?.GetAttributeCached<EditorCapabilityAttribute>();
        //if (cap != null && !ServiceInternals._license.GetCapability(cap.Capability))
        //{
        //    string msg = ServiceInternals._license.GetFailedMessage(cap.Capability);
        //    if (!string.IsNullOrWhiteSpace(msg))
        //    {
        //        await DialogUtility.ShowMessageBoxAsync(msg);
        //    }

        //    return;
        //}

        string fileName = await _format.OpenCreationUI(directory.NodePath);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        string fullPath = Path.Combine(directory.NodePath, fileName);
        if (File.Exists(fullPath))
        {
            await DialogUtility.ShowMessageBoxAsyncL("File already exists");
            return;
        }

        DocumentEntry docEntry = DocumentManager.Instance.NewDocument(fullPath, _format);
        if (docEntry is null)
        {
            return;
        }

        docEntry.MarkDirty(this);
        docEntry.Save();

        DocumentManager.Instance.ShowDocument(fullPath, _format);

        directory.PopulateUpdate();

        if (view.Model.GetNode(fullPath) is FileNode newFileNode)
        {
            view.SelectNode(newFileNode, false);
        }
    }
}