using Suity.Collections;
using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// ImGui-based dialog for finding and replacing string values in property targets.
/// Supports case-sensitive and whole-word matching options.
/// </summary>
internal class ReplaceStringImGui : IDrawImGui
{
    private static bool _caseSensitive;
    private static bool _fullMatch;

    private static string _find = string.Empty;
    private static string _replace = string.Empty;


    readonly IPropertyGrid _grid;

    ImGui? _gui;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceStringImGui"/> class.
    /// </summary>
    /// <param name="grid">The property grid to operate on.</param>
    public ReplaceStringImGui(IPropertyGrid grid)
    {
        _grid = grid ?? throw new System.ArgumentNullException(nameof(grid));
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        if (gui is null)
        {
            return;
        }

        if (!ReferenceEquals(gui, _gui))
        {
            _gui = gui;
        }

        gui.Frame("#bg")
        .InitClass("frameBg")
        .InitFullSize()
        .OnContent(() =>
        {
            gui.OverlayFrame()
            .InitClass("formBody")
            .InitFullWidth()
            .InitSizeRest()
            .OnContent(() =>
            {
                gui.VerticalLayout()
                .InitWidthPercentage(75f)
                .InitCenter()
                .OnContent(() =>
                {
                    gui.Text("Find");
                    _find = gui.StringInput("#find", null, _find).InitFullWidth().Text ?? string.Empty;
                    gui.Text("Replace");
                    _replace = gui.StringInput("#replace", null, _replace).InitFullWidth().Text ?? string.Empty;

                    gui.HorizontalLayout()
                    .InitFullWidth()
                    .OnContent(() =>
                    {
                        _caseSensitive = gui.CheckBox("#case_s", "Case Sensitive", _caseSensitive).InitClass("propInput").GetIsChecked();
                        _fullMatch = gui.CheckBox("#full_match", "Match Whole Word", _fullMatch).InitClass("propInput").GetIsChecked();
                    });

                    gui.VerticalLayout().InitHeight(30);

                    gui.Button("Replace")
                    .InitClass("toolBtn")
                    .InitWidth(100)
                    .InitCenter()
                    .OnClick(() =>
                    {
                        HandleReplace();
                    });
                });
            });
        });
    }

    /// <summary>
    /// Handles the replace operation by finding matching values and replacing them.
    /// Dispatches to the appropriate handler based on the value type.
    /// </summary>
    private void HandleReplace()
    {
        if (string.IsNullOrEmpty(_find))
        {
            return;
        }

        var target = _grid.GetPropertyTarget();
        if (target is null)
        {
            return;
        }

        var first = target.GetValues().FirstOrDefault();
        if (first is string)
        {
            HandleReplace_String(target);
        }
        else if (first is SString)
        {
            HandleReplace_SString(target);
        }
    }

    /// <summary>
    /// Performs find and replace on plain string property values.
    /// </summary>
    /// <param name="target">The property target containing string values.</param>
    private void HandleReplace_String(PropertyTarget target)
    {
        SearchOption option = SearchOption.None;
        if (_caseSensitive)
        {
            option |= SearchOption.MatchCase;
        }

        if (_fullMatch)
        {
            option |= SearchOption.MatchWholeWord;
        }

        string?[] strs = target.GetValues().As<string>().ToArray();
        bool modify = false;

        for (int i = 0; i < strs.Length; i++)
        {
            string? str = strs[i];
            if (str is null)
            {
                continue;
            }

            string replace = Validator.ReplaceString(str, _find, _replace ?? string.Empty, option);
            if (replace != str)
            {
                modify = true;
                strs[i] = replace;
            }
        }

        if (modify)
        {
            var act = target.SetValuesAction(strs);
            if (act is { })
            {
                _grid.DoAction(act);
            }

            if (_gui != null)
            {
                _gui.IsClosing = true;
            }
        }
    }

    /// <summary>
    /// Performs find and replace on <see cref="SString"/> property values.
    /// </summary>
    /// <param name="target">The property target containing SString values.</param>
    private void HandleReplace_SString(PropertyTarget target)
    {
        SearchOption option = SearchOption.None;
        if (_caseSensitive)
        {
            option |= SearchOption.MatchCase;
        }

        if (_fullMatch)
        {
            option |= SearchOption.MatchWholeWord;
        }

        SString?[] strs = target.GetValues().As<SString>().ToArray();
        bool modify = false;

        for (int i = 0; i < strs.Length; i++)
        {
            SString? sstr = strs[i];
            if (sstr is null)
            {
                continue;
            }

            string? str = sstr.StringValue;
            if (str is null)
            {
                continue;
            }

            string replace = Validator.ReplaceString(str, _find, _replace ?? string.Empty, option);
            if (replace != str)
            {
                modify = true;
                strs[i] = new SString(replace);
            }
        }

        if (modify)
        {
            var act = target.SetValuesAction(strs);
            if (act is { })
            {
                _grid.DoAction(act);
            }

            if (_gui != null)
            {
                _gui.IsClosing = true;
            }
        }
    }
}
