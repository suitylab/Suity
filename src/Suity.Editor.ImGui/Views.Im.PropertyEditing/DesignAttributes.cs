using Suity.Editor;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.PropertyEditing;


/// <summary>
/// Attribute that marks a property to use a file selection editor in the property grid.
/// Allows users to browse and select files with optional filtering and relative path support.
/// </summary>
[NativeType(CodeBase = "*Design", Name = "FileSelectionEditor", Description = "File Selection Editor", Icon = "*CoreIcon|File")]
public class FileSelectionEditorAttribute : DesignAttribute, IViewObject, IImGuiCustomPropertyEditor
{
    private string? _filter = string.Empty;
    private bool _relative;

    /// <summary>
    /// Gets the file filter string used when browsing for files.
    /// </summary>
    public string Filter => _filter ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the selected file path should be stored as a relative path.
    /// </summary>
    public bool Relative => _relative;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _filter = sync.Sync(nameof(Filter), _filter);
        _relative = sync.Sync(nameof(Relative), _relative);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_filter, new ViewProperty(nameof(Filter), "Filter"));
        setup.InspectorField(_relative, new ViewProperty(nameof(Relative), "Relative Path"));
    }

    /// <summary>
    /// Creates a property row function that renders the file selection editor.
    /// </summary>
    /// <returns>A <see cref="PropertyRowFunction"/> for rendering the file selection property editor.</returns>
    public PropertyRowFunction GetRowFunction()
    {
        PropertyEditorFunction func = SFileSelectionEditor;
        return func.MakeRowFunction();
    }

    /// <summary>
    /// Opens the specified file in the associated application or file explorer.
    /// </summary>
    /// <param name="file">The file path to navigate to. If <see cref="Relative"/> is true, the path is resolved relative to the project base path.</param>
    public void Navigate(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return;
        }

        if (_relative)
        {
            file = PathUtility.MakeFullPath(file, EditorServices.CurrentProject.ProjectBasePath);
        }

        TextFileHelper.NavigateFile(file);
    }

    private ImGuiNode SFileSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var cTarget = new ConvertedValueTarget<SString, string>(
            target,
            o => o?.Value?.ToString() ?? string.Empty,
            v => new SString(v));

        return FileSelectionEditor(gui, cTarget, handler);
    }

    private ImGuiNode FileSelectionEditor(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var values = target.GetValues();

        string value;
        if (!target.ValueMultiple || values.Any())
        {
            value = values.First()?.ToString() ?? string.Empty;
        }
        else
        {
            value = string.Empty;
        }

        var node = gui.StringInput($"{target.PropertyName}#string", value)
            .InitFullWidth()
            .SetClass(target.GetPropertyInputClass())
            .SetValueEditorColor(target)
            .InitWidthRest(24)
            .OnEdited(n =>
            {
                string v = n.Text ?? string.Empty;
                var action = target.SetValuesAction([v]);
                handler(action);
                n.SetClass(PropertyGridThemes.ClassPropertyInput);
            });

        void Apply(string str)
        {
            List<object> newValues = [];

            foreach (var _ in values)
            {
                newValues.Add(str);
            }

            var action = target.SetValuesAction(newValues);
            handler(action);

            node.SetClass(PropertyGridThemes.ClassPropertyInput);
        }

        gui.Button("##open", ImGuiIcons.More)
            .InitClass("configBtn")
            .OnClick(async () =>
            {
                if (target.ReadOnly)
                {
                    return;
                }

                try
                {
                    string filter = _filter ?? string.Empty;
                    if (!filter.Contains('|'))
                    {
                        filter = $"{_filter}|{_filter}";
                    }

                    string initFile = value ?? string.Empty;
                    string initPath = Path.GetDirectoryName(initFile);

                    if (_relative)
                    {
                        if (!string.IsNullOrWhiteSpace(initFile))
                        {
                            initFile = PathUtility.MakeFullPath(initFile, EditorServices.CurrentProject.ProjectBasePath);
                        }

                        if (!string.IsNullOrWhiteSpace(initPath))
                        {
                            initPath = PathUtility.MakeFullPath(initPath, EditorServices.CurrentProject.ProjectBasePath);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(initPath))
                    {
                        initPath = EditorServices.CurrentProject.ProjectBasePath;
                    }

                    string result = await DialogUtility.ShowOpenFileAsync(filter, initPath, initFile ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        if (_relative)
                        {
                            result = PathUtility.MakeRalativePath(result, EditorServices.CurrentProject.ProjectBasePath);
                        }

                        Apply(result);
                    }
                }
                catch (Exception err)
                {
                    err.LogError(L("Open file failed"));
                }
            });

        return node;
    }
}