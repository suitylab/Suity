using static Suity.Helpers.GlobalLocalizer;
using Suity.Collections;
using Suity.Editor.AIGC.RAG;
using Suity.Editor.AIGC.Tools;
using Suity.Editor.Design;
using Suity.Editor.Flows;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;


#region BaseGenerativeAssistant
/// <summary>
/// Base class for generative AI assistants that handle document generation, editing, and batch operations.
/// </summary>
public abstract class BaseGenerativeAssistant : AIDocumentAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseGenerativeAssistant"/> class.
    /// </summary>
    protected BaseGenerativeAssistant()
    {
        // AddParameterType<GenerativeCreateParam>();
        AddParameterType<GenerativeEditParam>();

        //AddParameterType<GenerativeBatchParam>();
        //AddParameterType<GenerativeCreateFromKnowledgeBaseParam>();
    }

    /// <summary>
    /// Handles the creation of a new element based on the AI request.
    /// </summary>
    /// <param name="request">The AI request containing user message and context.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public override Task<AICallResult> HandleElementCreate(AIRequest request)
    {
        // Convert to AIGenerativeRequest
        if (request is not AIGenerativeRequest genReq || genReq.Assistant != this)
        {
            genReq = new AIGenerativeRequest(this, request);
        }

        var param = new GenerativeCreateParam
        {
            Prompt = request.UserMessage
        };

        return CallTool(genReq, param);
    }

    /// <summary>
    /// Handles batch creation of multiple elements based on guiding items.
    /// </summary>
    /// <param name="request">The AI request containing user message and context.</param>
    /// <param name="guidings">Array of guiding items that specify what to create.</param>
    /// <param name="componentId">Optional component ID for the generated items.</param>
    /// <param name="groupPath">Optional group path where items should be created.</param>
    /// <param name="recordTooltips">Indicates whether tooltips should be recorded.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public override Task<AICallResult> HandleBatchCreate(AIRequest request, GenerativeGuidingItem[] guidings, Guid? componentId = null, string groupPath = null, bool recordTooltips = false)
    {
        // Convert to AIGenerativeRequest
        if (request is not AIGenerativeRequest genReq || genReq.Assistant != this)
        {
            genReq = new AIGenerativeRequest(this, request) 
            {
                ComponentId = componentId,
                GroupPath = groupPath,
                RecordTooltips = recordTooltips,
            };
        }

        var param = new GenerativeBatchParam
        {
            Guidings = guidings,
        };

        return CallTool(genReq, param);
    }

    /// <summary>
    /// Handles editing of an existing element based on the AI request.
    /// </summary>
    /// <param name="request">The AI request containing user message and context.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public override Task<AICallResult> HandleElementEdit(AIRequest request) => HandleToolInput(request);

    /// <summary>
    /// Handles tool input from the AI request, converting it to a generative request if needed.
    /// </summary>
    /// <param name="request">The AI request containing user message and context.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public override Task<AICallResult> HandleToolInput(AIRequest request)
    {
        // Convert to AIGenerativeRequest
        if (request is not AIGenerativeRequest genReq || genReq.Assistant != this)
        {
            genReq = new AIGenerativeRequest(this, request);
        }

        return base.HandleToolInput(genReq);
    }


/// <summary>
/// Handle generation preparation pipeline
/// </summary>
    /// <param name="request"></param>
    /// <param name="pipeline"></param>
    /// <param name="intent"></param>
    /// <returns></returns>
    /// <exception cref="AigcException"></exception>
    public virtual Task HandlePipeline(AIGenerativeRequest request, GenerativePipeline pipeline, GenerativeIntent intent)
    {
        if (this.TargetDocument is not { } doc)
        {
            throw new AigcException(L("Target document not set."));
        }

        switch (pipeline)
        {
            case GenerativePipeline.BuildSourceKnowlegeBase:
                request.SourceKnowledgeBase ??= this.GetKnowledgeInputs();
                break;

            case GenerativePipeline.BuildEditableList:
                {
                    var memberContainer = doc as IMemberContainer
                        ?? throw new AigcException(L($"{doc.ToDisplayText()} document object is not a member container."));

                    if (request.EditableList is null)
                    {
                        var list = memberContainer.Members.Select(o => GetEditableItemInfo(request, o)).SkipNull().ToList();
                        request.EditableList = new GenerativeEditableList 
                        {
                            Items = list,
                        };
                    }
                }
                break;

            default:
                break;
        }

        return Task.CompletedTask;
    }

/// <summary>
/// Handle generation pipeline
/// </summary>
    /// <param name="request"></param>
    /// <param name="guiding"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="AigcException"></exception>
    public virtual Task<AIGenerativeCallResult> HandleGenerate(AIGenerativeRequest request, GenerativeGuidingItem guiding)
    {
        switch (guiding.EditType)
        {
            case GenerativeEditType.Create:
                throw new AigcException(L($"{this.ToDisplayText()} generation pipeline not implemented."));

            case GenerativeEditType.Modify:
                throw new AigcException(L($"{this.ToDisplayText()} update pipeline not implemented."));

            case GenerativeEditType.Rename:
                {
                    if (!NamingVerifier.VerifyIdentifier(guiding.Prompt))
                    {
                        throw new AigcException(L("New name is not a valid identifier: ") + guiding.Prompt);
                    }

                    if (GetEditableItem(guiding.Name) is NamedItem item)
                    {
                        var action = new NamedItemRenameAction(item, guiding.Prompt);
                        var result = new AIGenerativeCallResult(action, [item]);

                        return Task.FromResult(result);
                    }
                    else
                    {
                        throw new AigcException(L($"{this.ToDisplayText()} rename pipeline not implemented."));
                    }
                }

            case GenerativeEditType.Delete:
                {
                    if (GetEditableItem(guiding.Name) is NamedItem item)
                    {
                        var action = new NamedItemDeleteAction(item);
                        var result = new AIGenerativeCallResult(action, [item]);

                        return Task.FromResult(result);
                    }
                    else
                    {
                        throw new AigcException(L($"{this.ToDisplayText()} delete pipeline not implemented."));
                    }
                }

            default:
                return Task.FromResult(AIGenerativeCallResult.Empty);
        }
    }

/// <summary>
/// Get information for a single editable item
/// </summary>
    /// <param name="request"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual GenerativeEditableItem GetEditableItemInfo(AIGenerativeRequest request, object item)
    {
        switch (item)
        {
            case FlowDiagramItem diagramItem:
                {
                    string brief = diagramItem?.ToToolTipsText() ?? item?.ToDisplayText() ?? string.Empty;
                    return new GenerativeEditableItem
                    {
                        Name = diagramItem.Name,
                        Position = new Point(diagramItem.X, diagramItem.Y),
                        Brief = brief,
                    };
                }

            case INamed named:
                {
                    string brief = named?.ToToolTipsText() ?? item?.ToDisplayText() ?? string.Empty;
                    return new GenerativeEditableItem
                    {
                        Name = named.Name,
                        Brief = brief,
                    };
                }

            case GenerativeGuidingItem guiding:
                return new GenerativeEditableItem
                {
                    Name = guiding.Name,
                    Brief = guiding.Brief,
                };

            default:
                return null;
        }
    }

/// <summary>
/// Get editable item by name
/// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="AigcException"></exception>
    public virtual object GetEditableItem(string name)
    {
        if (this.TargetDocument is not { } doc)
        {
            throw new AigcException(L("Target document not set."));
        }

        var memberContainer = doc as IMemberContainer
            ?? throw new AigcException(L($"{doc.ToDisplayText()} document object is not a member container."));

        return memberContainer.GetMember(name);
    }

    #region Actions
    /// <summary>
    /// Action that renames a named item in the document.
    /// </summary>
    class NamedItemRenameAction : AIGenerativeApplyAction
    {
        readonly NamedItem _item;
        readonly string _oldName;
        readonly string _newName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedItemRenameAction"/> class.
        /// </summary>
        /// <param name="item">The named item to rename.</param>
        /// <param name="newName">The new name for the item.</param>
        public NamedItemRenameAction(NamedItem item, string newName)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _oldName = item.Name;
            _newName = newName;
        }

        /// <summary>
        /// Gets the display name of this action.
        /// </summary>
        public override string Name => L($"Rename {_oldName} > {_newName}");

        /// <summary>
        /// Gets the objects that were affected by this action.
        /// </summary>
        /// <returns>An array containing the renamed item.</returns>
        public override object[] GetAppliedObjects() => [_item];

        /// <summary>
        /// Executes the rename action.
        /// </summary>
        public override void Do()
        {
            _item.Name = _newName;
        }

        /// <summary>
        /// Undoes the rename action, restoring the original name.
        /// </summary>
        public override void Undo()
        {
            _item.Name = _oldName;
        }
    }

    /// <summary>
    /// Action that deletes a named item from the document.
    /// </summary>
    class NamedItemDeleteAction : AIGenerativeApplyAction
    {
        readonly NamedItem _item;
        readonly INamedItemList _list;
        readonly int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedItemDeleteAction"/> class.
        /// </summary>
        /// <param name="item">The named item to delete.</param>
        public NamedItemDeleteAction(NamedItem item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _list = _item._parentList;
            _index = _item.GetIndex();
        }

        /// <summary>
        /// Gets the display name of this action.
        /// </summary>
        public override string Name => $"Delete {_item.Name}";

        /// <summary>
        /// Gets the objects that were affected by this action.
        /// </summary>
        /// <returns>An empty array since the item is deleted.</returns>
        public override object[] GetAppliedObjects() => [];

        /// <summary>
        /// Executes the delete action by removing the item from its parent list.
        /// </summary>
        public override void Do()
        {
            if (_list is null)
            {
                return;
            }

            _list.Remove(_item);
        }

        /// <summary>
        /// Undoes the delete action by reinserting the item at its original position.
        /// </summary>
        public override void Undo()
        {
            if (_list is null)
            {
                return;
            }

            _list.Insert(_index, _item);
        }
    }
    #endregion
}
#endregion

#region AIGenerativeRequest
/// <summary>
/// AI Generation Context
/// </summary>
public class AIGenerativeRequest : AIRequest
{
    /// <summary>
    /// Gets the generative assistant associated with this request.
    /// </summary>
    public BaseGenerativeAssistant Assistant { get; }

    #region Planning

    /// <summary>
    /// Gets or sets the generation plan.
    /// </summary>
    public string Plan { get; set; }

/// <summary>
/// Group path to write to
/// </summary>
    public string GroupPath { get; set; }

/// <summary>
/// Used for data generation, specifies the component type of the data.
/// </summary>
    public Guid? ComponentId { get; set; }

    /// <summary>
    /// Gets or sets the edit information for the generation request.
    /// </summary>
    public GenerativeEditInfo EditInfo { get; set; }

/// <summary>
/// Feature query result
/// </summary>
    public FeatureQueryResult[] Features { get; set; }

/// <summary>
/// Repair information
/// </summary>
    public List<string> Fixture { get; } = [];

/// <summary>
/// Indicates whether knowledge needs to be recorded
/// </summary>
    public bool RecordTooltips { get; set; }

    #endregion

    #region Guiding & Result

/// <summary>
/// All design guidances for this generation, used to get overall reference when creating individual objects.
/// </summary>
    public GenerativeGuidingItem[] Guidings { get; private set; }

    /// <summary>
    /// Gets additional shared guiding text for the generation.
    /// </summary>
    public string AdditionalSharedGuiding { get; private set; }

    /// <summary>
    /// Builds the full text representation of all guiding items.
    /// </summary>
    /// <returns>A string containing the full text of all guiding items, or empty string if none exist.</returns>
    public string BuildGuidingsFullText()
    {
        if (Guidings is null || Guidings.Length == 0)
        {
            return string.Empty;
        }

        return GenerativeGuidingItem.ToFullText(Guidings);
    }

    /// <summary>
    /// Builds the full text representation of guiding items filtered by edit type.
    /// </summary>
    /// <param name="editType">The edit type to filter guiding items by.</param>
    /// <returns>A string containing the full text of filtered guiding items, or empty string if none exist.</returns>
    public string BuildGuidingsFullText(GenerativeEditType editType)
    {
        if (Guidings is null || Guidings.Length == 0)
        {
            return string.Empty;
        }

        return GenerativeGuidingItem.ToFullText(Guidings, editType);
    }

    /// <summary>
    /// Gets the results of the generation operation.
    /// </summary>
    public object[] Results { get; private set; }

    #endregion

    #region Pipeline

    /// <summary>
    /// Gets or sets the source knowledge base for the generation.
    /// </summary>
    public IKnowledgeBase[] SourceKnowledgeBase { get; set; }

/// <summary>
/// Document editable object list
/// </summary>
    public GenerativeEditableList EditableList { get; set; }

    /// <summary>
    /// Gets or sets the knowledge derived from guiding items.
    /// </summary>
    public string GuidingKnowledge { get; set; }

    /// <summary>
    /// Gets or sets the knowledge derived from the editable list.
    /// </summary>
    public string EditListKnowledge { get; set; }

/// <summary>
/// Generation context
/// </summary>
    public object GenerativeContext { get; set; }
    
    /// <summary>
    /// Gets or sets the preparation result of the generation pipeline.
    /// </summary>
    public AIGenerativeCallResult Preparation { get; set; }

    /// <summary>
    /// Gets or sets the conclusion result of the generation pipeline.
    /// </summary>
    public AIGenerativeCallResult Conclusion { get; set; }


    /// <summary>
    /// Builds the source knowledge base if not already set.
    /// </summary>
    /// <param name="intent">The generative intent for the pipeline.</param>
    /// <returns>A task representing the asynchronous operation, returning the source knowledge base array.</returns>
    public async Task<IKnowledgeBase[]> PipelineBuildSourceKnowlegeBase(GenerativeIntent intent)
    {
        if (SourceKnowledgeBase is null)
        {
            await Assistant.HandlePipeline(this, GenerativePipeline.BuildSourceKnowlegeBase, intent);
        }

        return SourceKnowledgeBase;
    }

    /// <summary>
    /// Builds the editable list if not already set.
    /// </summary>
    /// <param name="intent">The generative intent for the pipeline.</param>
    /// <returns>A task representing the asynchronous operation, returning the editable list.</returns>
    public async Task<GenerativeEditableList> PipelineBuildEditableList(GenerativeIntent intent)
    {
        if (EditableList is null)
        {
            await Assistant.HandlePipeline(this, GenerativePipeline.BuildEditableList, intent);
        }

        return EditableList;
    }

    /// <summary>
    /// Builds the guiding knowledge if not already set.
    /// </summary>
    /// <param name="intent">The generative intent for the pipeline.</param>
    /// <returns>A task representing the asynchronous operation, returning the guiding knowledge string.</returns>
    public async Task<string> PipelineBuildGuidingKnowledge(GenerativeIntent intent)
    {
        if (GuidingKnowledge is null)
        {
            await Assistant.HandlePipeline(this, GenerativePipeline.BuildGuidingKnowledge, intent);
        }

        return GuidingKnowledge;
    }

    /// <summary>
    /// Builds the edit list knowledge if not already set.
    /// </summary>
    /// <param name="intent">The generative intent for the pipeline.</param>
    /// <returns>A task representing the asynchronous operation, returning the edit list knowledge string.</returns>
    public async Task<string> PipelineBuildEditListKnowledge(GenerativeIntent intent)
    {
        if (EditListKnowledge is null)
        {
            await Assistant.HandlePipeline(this, GenerativePipeline.BuildEditListKnowledge, intent);
        }

        return EditListKnowledge;
    }

/// <summary>
/// Enter generation pipeline
/// </summary>
/// <param name="intent">Generation pipeline type</param>
/// <param name="guidings">Generation guidances</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<object> PipelinePrepareGenerative(GenerativeIntent intent, GenerativeGuidingItem[] guidings, string additionalSharedGuiding = null)
    {
        if (guidings is null)
        {
            throw new ArgumentNullException(nameof(guidings));
        }

        if (Guidings != null)
        {
            throw new InvalidOperationException("Guidings already set.");
        }

        Guidings = guidings;

        AdditionalSharedGuiding = additionalSharedGuiding;

        await Assistant.HandlePipeline(this, GenerativePipeline.PrepareGenerative, intent);

        if (Preparation?.Action is { } action)
        {
            DoAction(action);
        }

        return GenerativeContext;
    }

    /// <summary>
    /// Finishes the generation pipeline and produces the conclusion result.
    /// </summary>
    /// <param name="intent">The generative intent for the pipeline.</param>
    /// <param name="items">The generated items to include in the results.</param>
    /// <returns>A task representing the asynchronous operation, returning the generation call result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if results are already set.</exception>
    public async Task<AIGenerativeCallResult> PipelineFinishGenerative(GenerativeIntent intent, IEnumerable<object> items)
    {
        if (Results != null)
        {
            throw new InvalidOperationException("Results already set.");
        }

        Results = items?.ToArray() ?? [];

        if (Conclusion is null)
        {
            await Assistant.HandlePipeline(this, GenerativePipeline.FinishGenerative, intent);

            if (Conclusion != null)
            {
                if (Conclusion.Action is { } action)
                {
                    DoAction(action);
                }
            }
            else
            {
                Conclusion = new AIGenerativeCallResult(null, Results);
            }
        }

        return Conclusion;
    }

    #endregion

    /// <summary>
    /// Executes an undo/redo action on the canvas or directly.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void DoAction(UndoRedoAction action)
    {
        if (action is null)
        {
            return;
        }

        if (Assistant?.Context is { } canvas)
        {
            canvas.DoCanvasAction(action);
        }
        else
        {
            action.Do();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeRequest"/> class with the specified assistant.
    /// </summary>
    /// <param name="assistant">The generative assistant associated with this request.</param>
    public AIGenerativeRequest(BaseGenerativeAssistant assistant)
    {
        Assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeRequest"/> class with the specified assistant and origin request.
    /// </summary>
    /// <param name="assistant">The generative assistant associated with this request.</param>
    /// <param name="origin">The original AI request to copy from.</param>
    /// <param name="increaseDepth">Indicates whether to increase the request depth.</param>
    public AIGenerativeRequest(BaseGenerativeAssistant assistant, AIRequest origin, bool increaseDepth = false)
        : base(origin, increaseDepth)
    {
        Assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
    }
}
#endregion

#region AIGenerativeCallResult
/// <summary>
/// Represents the result of a generative AI call, containing actions, results, and messages.
/// </summary>
public class AIGenerativeCallResult : AICallResult
{
    /// <summary>
    /// Gets an empty generative call result instance.
    /// </summary>
    public static new AIGenerativeCallResult Empty { get; } = new();

    private readonly AIGenerativeApplyAction _action;
    private readonly string _message;
    private readonly object[] _results;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeCallResult"/> class.
    /// </summary>
    public AIGenerativeCallResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeCallResult"/> class with the specified action.
    /// </summary>
    /// <param name="action">The generative apply action to execute.</param>
    public AIGenerativeCallResult(AIGenerativeApplyAction action)
    {
        _action = action;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeCallResult"/> class with the specified action and results.
    /// </summary>
    /// <param name="action">The generative apply action to execute.</param>
    /// <param name="results">The collection of result objects.</param>
    public AIGenerativeCallResult(AIGenerativeApplyAction action, IEnumerable<object> results)
    {
        _action = action;
        _results = results?.SkipNull().ToArray() ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AIGenerativeCallResult"/> class with the specified action, results, and message.
    /// </summary>
    /// <param name="action">The generative apply action to execute.</param>
    /// <param name="results">The collection of result objects.</param>
    /// <param name="message">The message associated with this result.</param>
    public AIGenerativeCallResult(AIGenerativeApplyAction action, IEnumerable<object> results, string message)
    {
        _action = action;
        _results = results?.SkipNull().ToArray() ?? [];
        _message = message;
    }

    /// <summary>
    /// Gets the generative apply action associated with this result.
    /// </summary>
    public AIGenerativeApplyAction Action => _action;

    /// <summary>
    /// Gets the base result objects.
    /// </summary>
    public IEnumerable<object> BaseResult => _results ?? [];

    /// <summary>
    /// Gets the objects that were affected by the action.
    /// </summary>
    public IEnumerable<object> ActionResult
    {
        get
        {
            if (_action?.GetAppliedObjects() is { } objects && objects.Length > 0)
            {
                return objects.SkipNull();
            }
            else
            {
                return [];
            }
        }
    }

    /// <summary>
    /// Gets the status of the AI call, which is always Result for generative calls.
    /// </summary>
    public override AICallStatus Status => AICallStatus.Result;

    /// <summary>
    /// Gets the message associated with this result.
    /// </summary>
    public override string Message => _message ?? "";

    /// <summary>
    /// Gets the primary result object.
    /// </summary>
    public override object Result => Results;

    /// <summary>
    /// Gets all result objects, preferring action results over base results.
    /// </summary>
    public override IEnumerable<object> Results
    {
        get
        {
            // Prefer to get results from action
            if (_action?.GetAppliedObjects() is { } objects && objects.Length > 0)
            {
                return objects.SkipNull();
            }
            // Then get results from _results
            else
            {
                return _results ?? [];
            }
        }
    }

    /// <summary>
    /// Gets the type of the result, which is always object for generative calls.
    /// </summary>
    public override Type ResultType => typeof(object);

    /// <summary>
    /// Returns a string representation of this generative call result.
    /// </summary>
    /// <returns>A string containing the result message.</returns>
    public override string ToString()
    {
        return $"AIGenerativeCallResult: {Message}";
    }
}
#endregion

#region AIGenerativeApplyAction

/// <summary>
/// Base class for generative apply actions that can be executed and undone.
/// </summary>
public abstract class AIGenerativeApplyAction : UndoRedoAction
{
    /// <summary>
    /// Gets the objects that were affected by this action.
    /// </summary>
    /// <returns>An array of objects that were modified by the action.</returns>
    public abstract object[] GetAppliedObjects();
}

#endregion
