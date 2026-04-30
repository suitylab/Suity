namespace Suity.Views.PathTree;

/// <summary>
/// Represents the population state of a node's child items in the path tree.
/// </summary>
public enum PopulateState
{
    /// <summary>
    /// Child nodes not listed
    /// </summary>
    None = 0,

    /// <summary>
    /// Has child nodes but not listed
    /// </summary>
    PopulateDummy = 1,

    /// <summary>
    /// Child nodes listed
    /// </summary>
    Populated = 2,
}
