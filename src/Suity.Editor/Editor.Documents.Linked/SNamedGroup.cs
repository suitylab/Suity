using Suity.Editor.Analyzing;
using Suity.Views.Named;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Represents a group of named items with analysis support.
/// </summary>
[DisplayText("Group", "*CoreIcon|Group")]
public class SNamedGroup : NamedGroup, ISupportAnalysis
{
    /// <summary>
    /// Gets the document containing this group.
    /// </summary>
    public SNamedDocument GetDocument() => (Root as SNamedRootCollection)?.Document;

    public SNamedGroup()
    {
    }

    public SNamedGroup(string groupName)
        : base(groupName)
    {
    }

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    public AnalysisResult Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems.
    /// </summary>
    /// <param name="problems">The problem collector.</param>
    /// <param name="intent">The analysis intent.</param>
    public virtual void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    { }

    #endregion
}