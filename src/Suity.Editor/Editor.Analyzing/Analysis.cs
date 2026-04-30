namespace Suity.Editor.Analyzing;

/// <summary>
/// Defines an interface for objects that support analysis operations.
/// </summary>
public interface ISupportAnalysis
{
    /// <summary>
    /// Gets or sets the analysis result for this object.
    /// </summary>
    AnalysisResult Analysis { get; set; }

    /// <summary>
    /// Collects problems found during analysis.
    /// </summary>
    /// <param name="problems">The problem collector to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    void CollectProblem(AnalysisProblem problems, AnalysisIntents intent);
}

/// <summary>
/// Abstract base class for problem collectors that can identify issues in analyzed objects.
/// </summary>
public abstract class ProblemCollector
{
    /// <summary>
    /// Gets or sets the base problem collector.
    /// </summary>
    protected internal ProblemCollector Base { get; internal set; } = EmptyProblemCollector.Empty;

    /// <summary>
    /// Collects problems from the specified object.
    /// </summary>
    /// <param name="obj">The object to analyze for problems.</param>
    /// <param name="problems">The problem collector to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    public abstract void CollectProblem(object obj, AnalysisProblem problems, AnalysisIntents intent);
}

internal class EmptyProblemCollector : ProblemCollector
{
    public static readonly EmptyProblemCollector Empty = new();

    public override void CollectProblem(object obj, AnalysisProblem problems, AnalysisIntents intent)
    { }
}

/// <summary>
/// Generic abstract base class for problem collectors that can identify issues in objects of a specific type.
/// </summary>
public abstract class ProblemCollector<T> : ProblemCollector
{
    /// <summary>
    /// Collects problems from the specified object, casting it to the target type.
    /// </summary>
    /// <param name="obj">The object to analyze for problems.</param>
    /// <param name="problems">The problem collector to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    public override void CollectProblem(object obj, AnalysisProblem problems, AnalysisIntents intent)
    {
        CollectProblem((T)obj, problems, intent);
    }

    /// <summary>
    /// Collects problems from the specified target object of type T.
    /// </summary>
    /// <param name="target">The target object to analyze for problems.</param>
    /// <param name="problems">The problem collector to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    public abstract void CollectProblem(T target, AnalysisProblem problems, AnalysisIntents intent);
}

/// <summary>
/// Defines an interface for objects that can be analyzed.
/// </summary>
public interface IAnalysable
{
    /// <summary>
    /// Requests an analysis to be performed on this object.
    /// </summary>
    void RequestAnalyze();

    /// <summary>
    /// Requests an update to the reference count for this object.
    /// </summary>
    void RequestUpdateReferenceCount();
}

/// <summary>
/// Analysis intent
/// </summary>
public enum AnalysisIntents
{
    /// <summary>
    /// Normal
    /// </summary>
    Normal,

    /// <summary>
    /// At project startup
    /// </summary>
    Startup,
}

/// <summary>
/// Options for controlling what data is collected during analysis.
/// </summary>
public class AnalysisOption
{
    /// <summary>
    /// Gets or sets the analysis intent.
    /// </summary>
    public AnalysisIntents Intent { get; set; } = AnalysisIntents.Normal;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisOption"/> class with default settings.
    /// </summary>
    public AnalysisOption()
    {
    }

    /// <summary>
    /// Gets or sets whether to collect problems during analysis.
    /// </summary>
    public bool CollectProblem { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect member information during analysis.
    /// </summary>
    public bool CollectMember { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect reference information during analysis.
    /// </summary>
    public bool CollectReference { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect conflict information during analysis.
    /// </summary>
    public bool CollectConflict { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect external dependencies during analysis.
    /// </summary>
    public bool CollectExternalDependencies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect problem dependencies during analysis.
    /// </summary>
    public bool CollectProblemDependencies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect render targets during analysis.
    /// </summary>
    public bool CollectRenderTargets { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate result text during analysis.
    /// </summary>
    public bool GenerateResultText { get; set; }
}