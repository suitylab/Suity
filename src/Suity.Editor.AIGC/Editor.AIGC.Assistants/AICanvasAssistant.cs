using Suity.Editor.Documents;
using Suity.Editor.Design;
using Suity.Editor.Flows;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Base class for AI assistants that operate within a canvas context.
/// </summary>
public abstract class AICanvasAssistant : AIAssistant
{
    /// <summary>
    /// Gets the canvas context associated with this assistant.
    /// </summary>
    public CanvasContext Context { get; private set; }

    /// <summary>
    /// Gets the target asset node from the canvas context.
    /// </summary>
    public CanvasAssetNode TargetAssetNode => Context?.TargetAssetNode;

    /// <summary>
    /// Target document
    /// </summary>
    public Document TargetDocument => Context?.TargetDocument;

    /// <summary>
    /// Gets the current selection from the canvas context.
    /// </summary>
    public object[] Selection => Context.Selection;

    /// <summary>
    /// Document view. Note that this view is not necessarily the view of <see cref="TargetDocument"/>, possibly the view of <see cref="ICanvasDocument"/>.)
    /// </summary>
    public IDocumentView RootView => Context?.GetCanvasView();

    /// <summary>
    /// Initializes the assistant with the specified canvas context.
    /// </summary>
    /// <param name="canvasContext">The canvas context to initialize with.</param>
    protected internal void InitializeCanvas(CanvasContext canvasContext)
    {
        Context = canvasContext ?? throw new System.ArgumentNullException(nameof(canvasContext));
    }

    /// <summary>
    /// Creates a new canvas from the specified file path.
    /// </summary>
    /// <param name="rFilePath">The file path for the canvas.</param>
    /// <param name="showView">Whether to show the canvas view. Defaults to true.</param>
    /// <returns>The created canvas document.</returns>
    protected ICanvasDocument CreateCanvas(string rFilePath, bool showView = true) 
        => AIAssistantService.Instance.CreateCanvas(rFilePath, showView);

    #region Assistant

    /// <summary>
    /// Creates a document assistant for the specified canvas context.
    /// </summary>
    /// <param name="selection">The canvas context to create the assistant for.</param>
    /// <returns>A new document assistant instance.</returns>
    protected AIDocumentAssistant CreateDocumentAssistant(CanvasContext selection)
    {
        return AIAssistantService.Instance.CreateDocumentAssistant(selection);
    }

    /// <summary>
    /// Creates a RAG (Retrieval-Augmented Generation) assistant for the specified canvas context.
    /// </summary>
    /// <param name="selection">The canvas context to create the assistant for.</param>
    /// <returns>A new RAG assistant instance.</returns>
    protected AIDocumentAssistant CreateRAGAssistant(CanvasContext selection)
    {
        return AIAssistantService.Instance.CreateRAGAssistant(selection);
    }

    /// <summary>
    /// Selects an assistant of the specified type based on the request and current context.
    /// </summary>
    /// <typeparam name="T">The type of assistant to select.</typeparam>
    /// <param name="request">The AI request to base the selection on.</param>
    /// <param name="withCurrentSelection">Whether to include the current selection in the context. Defaults to false.</param>
    /// <returns>The selected assistant instance, or null if none found.</returns>
    protected async Task<T> SelectAssistant<T>(AIRequest request, bool withCurrentSelection = false)
        where T : class
    {
        var sel = this.Context.Clone(withCurrentSelection);

        var assistant = await AIAssistantService.Instance.SelectAssistant<T>(request, sel);

        return assistant;
    }

    /// <summary>
    /// Selects an assistant of the specified type based on the request and specified context.
    /// </summary>
    /// <typeparam name="T">The type of assistant to select.</typeparam>
    /// <param name="request">The AI request to base the selection on.</param>
    /// <param name="selection">The canvas context to use for selection.</param>
    /// <returns>The selected assistant instance, or null if none found.</returns>
    protected async Task<T> SelectAssistant<T>(AIRequest request, CanvasContext selection)
        where T : class
    {
        var assistant = await AIAssistantService.Instance.SelectAssistant<T>(request, selection);

        return assistant;
    }

    /// <summary>
    /// Creates a canvas assistant of the specified type using the current context.
    /// </summary>
    /// <typeparam name="T">The type of canvas assistant to create. Must inherit from <see cref="AICanvasAssistant"/> and have a parameterless constructor.</typeparam>
    /// <param name="withCurrentSelection">Whether to include the current selection in the context. Defaults to false.</param>
    /// <returns>A new canvas assistant instance.</returns>
    protected T CreateCanvasAssistance<T>(bool withCurrentSelection = false)
        where T : AICanvasAssistant, new()
    {
        var sel = this.Context.Clone(withCurrentSelection);

        var assistant = AIAssistantService.Instance.CreateCanvasAssistant<T>(sel);

        return assistant;
    }

    /// <summary>
    /// Creates a canvas assistant of the specified type using the provided context.
    /// </summary>
    /// <typeparam name="T">The type of canvas assistant to create. Must inherit from <see cref="AICanvasAssistant"/> and have a parameterless constructor.</typeparam>
    /// <param name="selection">The canvas context to use.</param>
    /// <returns>A new canvas assistant instance.</returns>
    protected T CreateCanvasAssistance<T>(CanvasContext selection)
        where T : AICanvasAssistant, new()
    {
        var assistant = AIAssistantService.Instance.CreateCanvasAssistant<T>(selection);

        return assistant;
    }

    #endregion

    #region Tools

    /// <summary>
    /// Calls a tool with the specified request and parameter.
    /// </summary>
    /// <typeparam name="T">The type of the tool parameter.</typeparam>
    /// <param name="request">The AI request to execute.</param>
    /// <param name="parameter">The parameter to pass to the tool.</param>
    /// <returns>A task representing the asynchronous operation, containing the tool call result.</returns>
    protected Task<AICallResult> CallTool<T>(AIRequest request, T parameter)
        => AIAssistantService.Instance.CallTool<T>(request, this.Context, parameter);


    #endregion
}
