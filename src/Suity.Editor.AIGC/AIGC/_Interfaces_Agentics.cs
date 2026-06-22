using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Agentic;

/// <summary>
/// Represents an agent node in the AIGC agentic system.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Agent Node", Color = "#9900FF")]
[NativeAlias("Suity.Editor.AIGC.Agentic.IAgentNode")]
public interface IAgent
{
    /// <summary>
    /// Gets the name of the agent.
    /// </summary>
    string AgentName { get; }

    /// <summary>
    /// Gets the overview of the agent.
    /// </summary>
    string Overview { get; }

    /// <summary>
    /// Gets the parent agent, or null if this is a root agent.
    /// </summary>
    IAgent ParentAgent { get; }

    /// <summary>
    /// Gets all sub-agents of this agent.
    /// </summary>
    /// <returns>An array of child agents.</returns>
    IAgent[] GetSubAgents();

    /// <summary>
    /// Sets the parent agent for this agent.
    /// </summary>
    /// <param name="parent">The parent agent to assign.</param>
    void SetParentAgent(IAgent parent);

    /// <summary>
    /// Gets the starter workflow asset for this agent.
    /// </summary>
    ISubFlowAsset StarterWorkflow { get; }

    /// <summary>
    /// Gets all loops associated with this agent.
    /// </summary>
    /// <returns>An array of agent loops.</returns>
    IAgentLoop[] GetLoops();

    /// <summary>
    /// Gets a specific loop by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the loop.</param>
    /// <returns>The agent loop, or null if not found.</returns>
    IAgentLoop GetLoop(string id);

    /// <summary>
    /// Adds a new loop to this agent.
    /// </summary>
    /// <param name="loopAsset">The loop asset to associate.</param>
    /// <param name="description">The description of the loop.</param>
    /// <returns>The newly created agent loop.</returns>
    IAgentLoop AddLoop(IAigcLoopAsset loopAsset, string description);

    /// <summary>
    /// Runs the agent with the specified request.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="runner">The graph runner to execute the agent.</param>
    /// <returns>The result of the AI call.</returns>
    Task<AICallResult> Run(AIRequest request, IAgentGraphRunner runner);

    /// <summary>
    /// Queues a refresh view request for this agent.
    /// </summary>
    void QueueRefreshView();

    /// <summary>
    /// Flashes the connector in the specified direction.
    /// </summary>
    /// <param name="direction">The direction of the connector to flash.</param>
    void FlashingConnector(FlowDirections direction);
}

/// <summary>
/// Represents a loop within an agent that contains a reference to a loop asset.
/// </summary>
public interface IAgentLoop
{
    /// <summary>
    /// Gets the unique identifier of this loop.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the description of this loop.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the loop asset associated with this agent loop.
    /// </summary>
    IAigcLoopAsset LoopAsset { get; }
}


/// <summary>
/// Provides methods to manage and run agent loops within the AIGC graph.
/// </summary>
public interface IAgentGraphRunner
{
    /// <summary>
    /// Gets the state of the specified agent.
    /// </summary>
    /// <param name="agent">The agent to get state for.</param>
    /// <returns>The agent state.</returns>
    IAgentState GetAgentState(IAgent agent);

    /// <summary>
    /// Adds a new loop to the specified agent.
    /// </summary>
    /// <param name="agent">The agent to add the loop to.</param>
    /// <param name="description">The description of the loop.</param>
    /// <param name="prompt">The initial prompt for the loop.</param>
    /// <param name="loopFileName">Optional file name for the loop.</param>
    /// <returns>The newly created agent loop.</returns>
    IAgentLoop AddLoop(IAgent agent, string description, string prompt, string loopFileName = null);

    /// <summary>
    /// Runs a specific loop for the specified agent.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="agent">The agent that owns the loop.</param>
    /// <param name="loop">The loop to run.</param>
    /// <returns>The result of the AI call.</returns>
    Task<AICallResult> RunLoop(AIRequest request, IAgent agent, IAgentLoop loop);
}

/// <summary>
/// Represents the runtime state of an agent.
/// </summary>
public interface IAgentState
{
    /// <summary>
    /// Gets the agent associated with this state.
    /// </summary>
    IAgent Agent { get; }

    /// <summary>
    /// Gets the state of a specific loop within this agent.
    /// </summary>
    /// <param name="loop">The loop to get state for.</param>
    /// <returns>The loop state.</returns>
    IAgentLoopState GetLoopState(IAgentLoop loop);

    /// <summary>
    /// Gets a value indicating whether the agent is currently running.
    /// </summary>
    bool IsRunning { get; }
}

/// <summary>
/// Represents the runtime state of an agent loop.
/// </summary>
public interface IAgentLoopState
{
    /// <summary>
    /// Gets the loop associated with this state.
    /// </summary>
    IAgentLoop Loop { get; }

    /// <summary>
    /// Gets a value indicating whether the loop is currently running.
    /// </summary>
    bool IsRunning { get; }
}