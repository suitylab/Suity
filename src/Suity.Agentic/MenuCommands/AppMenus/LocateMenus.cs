using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Helpers;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Menu;
using System;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.MenuCommands.AppMenus;

#region NavigateMenuCommand

class NavigateMenuCommand : MenuCommand
{
    public NavigateMenuCommand()
        : base("Navigate", CoreIconCache.GotoDefination)
    {
        HotKey = "F3";
    }

    public override void DoCommand() => HandleNavigate();

    public static async void HandleNavigate()
    {
        var result = await new GlobalSelectionList().ShowSelectionGUIAsync(
            L("Navigate"),
            new SelectionOption
            {
                HideEmptySelection = true,
                InitialHideItems = true,
                Icon = CoreIconCache.Select
            });

        if (!result.IsSuccess)
        {
            return;
        }

        if (result.Item is EditorObject obj)
        {
            EditorUtility.NavigateTo(obj);
            return;
        }

        if (result.Item is RenderFileName renderFileName)
        {
            if (DocumentManager.Instance.ShowDocument(renderFileName.PhysicFullPath) is null)
            {
                EditorUtility.NavigateTo(result.Item);
            }

            return;
        }

        var asset = AssetManager.Instance.GetAsset(result.SelectedKey);
        if (asset != null)
        {
            EditorUtility.NavigateTo(asset);
            return;
        }

        EditorUtility.NavigateTo(result.Item);
    }
}

#endregion

#region GotoMenuCommand

class GotoMenuCommand : MenuCommand
{
    public GotoMenuCommand()
        : base("Goto")
    {
    }

    public override async void DoCommand()
    {
        string str = await DialogUtility.ShowSingleLineTextDialogAsyncL("Goto", string.Empty, s => true);
        if (string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        if (Guid.TryParse(str, out var id))
        {
            var obj = EditorObjectManager.Instance.GetObject(id);
            if (obj != null)
            {
                if (EditorUtility.NavigateTo(id))
                {
                    return;
                }
                else
                {
                    Logs.LogInfo(L($"Object: {obj.Name}, Type: {obj.GetType().Name}."));
                }

                return;
            }

            string resolved = GlobalIdResolver.RevertResolve(id);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                Logs.LogInfo(L($"Resolved string from history: {resolved}"));
            }
        }
        else
        {
            var asset = AssetManager.Instance.GetAsset(str);
            asset ??= AssetManager.Instance.GetAssetByResourceName(str);

            if (asset != null)
            {
                if (EditorUtility.NavigateTo(asset))
                {
                    return;
                }
                else
                {
                    Logs.LogInfo(L($"Asset: {asset.AssetKey}, Type: {asset.GetType().Name}."));
                    return;
                }
            }

            Logs.LogError(L($"Cannot find: {str}"));
        }
    }
}

#endregion

#region SearchMenuCommand

class SearchMenuCommand : MenuCommand
{
    public SearchMenuCommand()
        : base("Search", CoreIconCache.Search)
    {
        HotKey = "Ctrl+Shift+F";
    }

    public override async void DoCommand()
    {
        var viewSearch = DocumentViewManager.Current.ActiveDocument?.View?.GetService<IViewSearch>();

        if (viewSearch != null)
        {
            try
            {
                viewSearch.OpenSearch();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        else
        {
            await new SearchDialogImGui().CreateImGuiDialog("Search", 600, 140);
        }
    }
}

#endregion

#region SearchDialogImGui

class SearchDialogImGui : IDrawImGui
{
    string _input = string.Empty;
    bool _matchCase;
    bool _matchWholeWorld;

    public void OnGui(ImGui gui)
    {
        gui.VerticalLayout()
        .InitChildSpacing(10)
        .InitPadding(10)
        .InitFullSize()
        .OnContent(() => 
        {
            gui.VerticalLayout()
            .InitFullWidth()
            .InitHeightRest(40)
            .OnContent(() =>
            {
                gui.StringInput("#input", _input, null, "Search text...")
                .InitFullWidth()
                .OnInitialize(n =>
                {
                    n.BeginEdit();
                })
                .OnEdited(n => _input = n.Text ?? string.Empty);

                gui.HorizontalLayout()
                .InitFullWidth()
                .InitFitVertical()
                .InitChildSpacing(10)
                .OnContent(() =>
                {
                    _matchCase = gui.CheckBox("#matchCase", "Match case", _matchCase).InitClass("toolBtn").GetIsChecked();
                    _matchWholeWorld = gui.CheckBox("#matchWholeWord", "Match whole word", _matchWholeWorld).InitClass("toolBtn").GetIsChecked();
                });
            });

            gui.Button("search_btn","Search", CoreIconCache.Search).InitClass("toolBtn").InitCenterHorizontal().OnClick(() => 
            {
                gui.IsClosing = true;
                QueuedAction.Do(() => 
                {
                    DoSearch();
                });
            });
        });
    }

    private void DoSearch()
    {
        string findStr = _input;
        if (string.IsNullOrEmpty(findStr))
        {
            return;
        }

        SearchOption option = SearchOption.None;
        if (_matchCase)
        {
            option |= SearchOption.MatchCase;
        }

        if (_matchWholeWorld)
        {
            option |= SearchOption.MatchWholeWord;
        }

        EditorRexes.GlobalSearch.Invoke(findStr, option);
    }
}

#endregion
