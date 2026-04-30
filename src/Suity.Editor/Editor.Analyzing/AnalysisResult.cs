using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Analyzing;

/// <summary>
/// Represents the result of an analysis operation, containing various counts and problem information.
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// Gets or sets the unique identifier for this analysis result.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the asset key associated with this analysis result.
    /// </summary>
    public string AssetKey { get; set; }

    /// <summary>
    /// Gets or sets the number of members found during analysis.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Gets or sets the number of references found during analysis.
    /// </summary>
    public int ReferenceCount { get; set; }

    /// <summary>
    /// Gets or sets the number of resource usages found during analysis.
    /// </summary>
    public int ResourceUseCount { get; set; }

    /// <summary>
    /// Gets or sets the number of ID conflicts found during analysis.
    /// </summary>
    public int IdConflictCount { get; set; }

    /// <summary>
    /// Gets or sets the number of asset key conflicts found during analysis.
    /// </summary>
    public int AssetKeyConflictCount { get; set; }

    /// <summary>
    /// Gets or sets the number of types with multiple full type names found during analysis.
    /// </summary>
    public int MultipleFullTypeNameCount { get; set; }

    /// <summary>
    /// Gets or sets the number of missing references found during analysis.
    /// </summary>
    public int ReferenceMissingCount { get; set; }

    /// <summary>
    /// Gets or sets the number of reference conflicts found during analysis.
    /// </summary>
    public int ReferenceConflictCount { get; set; }


    /// <summary>
    /// Gets the list of dependent object IDs.
    /// </summary>
    public List<Guid> DependencyObjects { get; } = [];

    /// <summary>
    /// Gets the list of dependent file asset IDs.
    /// </summary>
    public List<Guid> DependencyFileAssets { get; } = [];

    /// <summary>
    /// Gets the root problem container containing all analysis problems.
    /// </summary>
    public AnalysisProblem Problems { get; }

    /// <summary>
    /// Gets the list of render targets generated during analysis.
    /// </summary>
    public List<RenderTarget> RenderTargets { get; } = [];

    /// <summary>
    /// Gets or sets the number of user code elements found during analysis.
    /// </summary>
    public int UserCodeCount { get; set; }

    /// <summary>
    /// Gets or sets the overall status of the analysis.
    /// </summary>
    public TextStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the text summary of the analysis.
    /// </summary>
    public string AnalysisText { get; set; } = string.Empty;


    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisResult"/> class with an owner object.
    /// </summary>
    /// <param name="owner">The owner object to associate with this analysis result.</param>
    /// <exception cref="ArgumentNullException">Thrown when owner is null.</exception>
    public AnalysisResult(object owner)
    {
        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        Problems = new AnalysisProblem(TextStatus.Normal, owner.ToDisplayText(), owner);
    }

    /// <summary>
    /// Clears all analysis results and resets counters to default values.
    /// </summary>
    public void Clear()
    {
        MemberCount = 0;
        ReferenceCount = 0;
        ResourceUseCount = 0;
        IdConflictCount = 0;
        AssetKeyConflictCount = 0;
        MultipleFullTypeNameCount = 0;
        ReferenceMissingCount = 0;
        ReferenceConflictCount = 0;
        UserCodeCount = 0;

        DependencyObjects.Clear();
        DependencyFileAssets.Clear();
        Problems.Clear();
        RenderTargets.Clear();
        Status = TextStatus.Normal;
        AnalysisText = string.Empty;
    }

    /// <summary>
    /// Clears specific analysis results based on the provided options.
    /// </summary>
    /// <param name="option">The <see cref="AnalysisOption"/> specifying which results to clear.</param>
    public void Clear(AnalysisOption option)
    {
        if (option.CollectProblem)
        {
            Status = TextStatus.Normal;
            Problems.Clear();
            AnalysisText = string.Empty;
        }

        if (option.CollectMember)
        {
            MemberCount = 0;
        }

        if (option.CollectReference)
        {
            ReferenceCount = 0;
            ResourceUseCount = 0;
        }

        if (option.CollectConflict)
        {
            IdConflictCount = 0;
            AssetKeyConflictCount = 0;
            MultipleFullTypeNameCount = 0;
        }

        if (option.CollectExternalDependencies)
        {
            DependencyObjects.Clear();
            DependencyFileAssets.Clear();
        }

        if (option.CollectProblemDependencies)
        {
            ReferenceMissingCount = 0;
            ReferenceConflictCount = 0;
        }

        if (option.CollectRenderTargets)
        {
            UserCodeCount = 0;
            RenderTargets.Clear();
        }
    }

    /// <summary>
    /// Updates the status to the higher of the current status and the provided status.
    /// </summary>
    /// <param name="status">The status to compare and potentially apply.</param>
    public void IncrementStatus(TextStatus status)
    {
        if (Status < status)
        {
            Status = status;
        }
    }
}