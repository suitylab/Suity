using Suity.Editor;
using Suity.Synchonizing;

namespace Suity.Views.Named;

/// <summary>
/// Interface for a named render target list that supports synchronization, viewing, and text display.
/// </summary>
public interface INamedRenderTargetList : 
    ISyncList, 
    IViewList, 
    ITextDisplay
{
}

/// <summary>
/// Interface for a named using list that supports synchronization, viewing, and text display.
/// </summary>
public interface INamedUsingList : 
    ISyncList, 
    IViewList, 
    ITextDisplay
{
}

/// <summary>
/// Interface for a named using list item that supports navigation, reference finding, and view interactions.
/// </summary>
public interface INamedUsingListItem : 
    INavigable, 
    IFindReferenceScope, 
    ITextDisplay,
    IViewDoubleClickAction,
    IViewObject
{
    /// <summary>
    /// Gets the target editor object referenced by this using item.
    /// </summary>
    EditorObject Target { get; }
}
