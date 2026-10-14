namespace Suity.Editor.AIGC;

/// <summary>
/// Marker returned from a chat/agent run when it was <b>cancelled</b> (e.g. the user stopped it)
/// rather than completed. Embedders (e.g. the OpenCode bridge) use this to distinguish a
/// resumable pause from a real completion/failure.
/// </summary>
public sealed class AgentRunCancelled
{
    /// <summary>Shared singleton; reference-compare with <c>ReferenceEquals</c>/pattern match.</summary>
    public static readonly AgentRunCancelled Instance = new();

    private AgentRunCancelled()
    {
    }

    public override string ToString() => "AgentRunCancelled";
}
