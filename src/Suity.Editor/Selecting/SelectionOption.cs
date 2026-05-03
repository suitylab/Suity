using Suity.Drawing;
using System;
using System.Drawing;

namespace Suity.Selecting;

/// <summary>
/// Select options on the interface
/// </summary>
public class SelectionOption
{
    /// <summary>
    /// Initial selected key
    /// </summary>
    public string SelectedKey { get; set; }

    /// <summary>
    /// Hide all selection initially
    /// </summary>
    public bool InitialHideItems { get; set; }

    /// <summary>
    /// Hide empty selection
    /// </summary>
    public bool HideEmptySelection { get; set; }

    /// <summary>
    /// Support multiple selection
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// Allow select list
    /// </summary>
    public bool AllowSelectList { get; set; }

    /// <summary>
    /// Display filter
    /// </summary>
    public Predicate<ISelectionItem> DisplayFilter { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    public ImageDef Icon { get; set; }
}