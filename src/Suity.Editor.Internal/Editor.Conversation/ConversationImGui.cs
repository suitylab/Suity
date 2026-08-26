using Suity.Collections;
using Suity.Editor.Services;
using Suity.Views.Im;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Conversation;

/// <summary>
/// Handles conversation UI rendering using ImGui and manages conversation lifecycle, input handling, and message display.
/// </summary>
public class ConversationImGui : ConversationHost,
    IDrawImGuiNode,
    IConversationImGui
{
    private readonly ImGuiNodeRef _guiRef = new();


    private bool _scrollToButtom;

    private readonly DialogMenuRootCommand _menu = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationImGui"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this conversation handler.</param>
    public ConversationImGui(string id)
        : base(id)
    {
    }

    [ThreadStatic]
    readonly static List<DialogItem> _tempItems = [];

    #region IDrawImGuiNode & ImGui
    /// <inheritdoc/>
    public ImGuiNode OnNodeGui(ImGui gui)
    {
        var node = _guiRef.Node = gui.ScrollableFrame($"#conversation_{Id}", GuiOrientation.Vertical)
        .InitFullWidth()
        .InitHeightRest()
        .InitChildSpacing(5)
        .OnPartialContent(() =>
        {
            _tempItems.Clear();
            base.FillItems(_tempItems);

            if (DisableOldMessage)
            {
                for (int i = 0; i < _tempItems.Count; i++)
                {
                    var item = _tempItems[i];
                    item?.OnGui(gui, i, i == _tempItems.Count - 1, _menu, this);
                }
            }
            else
            {
                for (int i = 0; i < _tempItems.Count; i++)
                {
                    var item = _tempItems.GetListItemSafe(i);
                    item?.OnGui(gui, i, true, _menu, this);
                }
            }

            _tempItems.Clear();
        })
        .AutoScrollToBottom();

        return node;
    }

    /// <summary>
    /// Scrolls the conversation view to the bottom.
    /// </summary>
    public void ScrollToBottom()
    {
        _guiRef.Node?.SetScrollRateY(1);

        _scrollToButtom = true;

        _guiRef.QueueRefresh();
    }
    #endregion

    #region Virtual

    protected override void OnRefreshRequested()
    {
        _guiRef.QueueRefresh();
    }

    protected override void OnScrollToButtonRequested()
    {
        ScrollToBottom();
        QueuedAction.Do(ScrollToBottom);
    }

    #endregion
}