using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Assistants;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// Interface for executing AI-generated content workflows
/// </summary>
public interface IAigcWorkflowRunner
{
    /// <summary>
    /// Executes the workflow asynchronously with the specified request and options
    /// </summary>
    /// <param name="request">The AI request containing the input data for the workflow</param>
    /// <param name="workflowOption">The configuration options for the workflow execution</param>
    /// <returns>A task representing the asynchronous operation with the workflow result</returns>
    Task<object> RunWorkflow(AIRequest request, AigcWorkflowOption workflowOption);

    /// <summary>
    /// Gets the LLM chat provider used for AI interactions
    /// </summary>
    ILLmChatProvider ChatProvider { get; }
}

/// <summary>
/// Represents configuration options for an AIGC workflow
/// </summary>
public class AigcWorkflowOption
{
    /// <summary>
    /// Gets or sets the runnable workflow implementation
    /// </summary>
    public IFlowRunnable Runnable { get; set; }

    /// <summary>
    /// Gets or sets the view component for the workflow visualization
    /// </summary>
    public IFlowView View { get; set; }

    /// <summary>
    /// Gets or sets the initialization action for the workflow computation
    /// </summary>
    public Action<IFlowComputation> Config { get; set; }
}