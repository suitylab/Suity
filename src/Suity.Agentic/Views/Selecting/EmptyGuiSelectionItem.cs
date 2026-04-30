using Suity.Selecting;
using Suity.Views;

namespace Suity.Editor.Views.Selecting;

public sealed class EmptyGuiSelectionItem : ISelectionItem, ITextDisplay
{
    public static readonly EmptyGuiSelectionItem Empty = new();

    private EmptyGuiSelectionItem()
    {
    }

    public string SelectionKey => null;

    public string DisplayText => "(None)";

    public object? DisplayIcon => null;

    // Cannot set to Disabled, because it means it's not selectable
    public TextStatus DisplayStatus => TextStatus.Normal;
}