using Suity.Editor.Analyzing;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Provides abstract methods for analyzing objects and collecting problems in the editor.
/// </summary>
public abstract class AnalysisService
{
    /// <summary>
    /// Gets or sets the current analysis service instance.
    /// </summary>
    public static AnalysisService Current { get; internal set; }

    /// <summary>
    /// Queues an object for analysis to be processed asynchronously.
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="option">Optional analysis options.</param>
    /// <param name="callBack">Optional callback to invoke when analysis completes.</param>
    public abstract void QueueAnalyze(object obj, AnalysisOption option = null, Action callBack = null);

    /// <summary>
    /// Analyzes the specified object synchronously.
    /// </summary>
    /// <param name="obj">The object to analyze.</param>
    /// <param name="option">Optional analysis options.</param>
    /// <param name="callBack">Optional callback to invoke when analysis completes.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Analyze(object obj, AnalysisOption option = null, Action callBack = null);

    /// <summary>
    /// Analyzes the specified item that supports analysis.
    /// </summary>
    /// <param name="item">The item that supports analysis.</param>
    /// <param name="option">Optional analysis options.</param>
    /// <param name="callBack">Optional callback to invoke when analysis completes.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Analyze(ISupportAnalysis item, AnalysisOption option = null, Action callBack = null);

    /// <summary>
    /// Collects problems from the specified object.
    /// </summary>
    /// <param name="obj">The object to collect problems from.</param>
    /// <param name="problems">The analysis problem collection to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    public abstract void CollecctProblems(object obj, AnalysisProblem problems, AnalysisIntents intent);

    /// <summary>
    /// Shows the problems from an analysis result.
    /// </summary>
    /// <param name="result">The analysis result containing problems to display.</param>
    public abstract void ShowProblems(AnalysisResult result);

    /// <summary>
    /// Shows the specified analysis problems.
    /// </summary>
    /// <param name="problems">The analysis problems to display.</param>
    public abstract void ShowProblems(AnalysisProblem problems);
}