using Suity.Collections;
using Suity.Editor.AIGC.Tools;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC.Assistants;

#region AIDocumentAssistantResolver
/// <summary>
/// Resolves and creates AI document assistant instances based on canvas context and document types.
/// </summary>
internal class AIDocumentAssistantResolver
    : BaseServiceTypeResolver<AIDocumentAssistant, AIAssistantUsageAttribute, DefaultAIAssistantUsageAttribute>
{
    /// <summary>
    /// Gets the singleton instance of <see cref="AIDocumentAssistantResolver"/>.
    /// </summary>
    public static AIDocumentAssistantResolver Instance { get; } = new();


    private AIDocumentAssistantResolver() : base("AI document assistant")
    {
    }

    /// <summary>
    /// Creates a single document assistant for the given canvas selection.
    /// </summary>
    /// <param name="selection">The canvas context containing the target document.</param>
    /// <returns>An <see cref="AIDocumentAssistant"/> instance if a matching assistant type is found; otherwise, null.</returns>
    public AIDocumentAssistant CreateDocumentAssistant(CanvasContext selection)
    {
        if (!IsInitialized)
        {
            Initialize();
        }

        if (selection is null || selection.TargetDocument is null)
        {
            return null;
        }

        var assistantType = ResolveServiceType(selection.TargetDocument.GetType());
        if (assistantType != null)
        {
            var assistant = (AIDocumentAssistant)assistantType.CreateInstanceOf();
            assistant.InitializeCanvas(selection);

            return assistant;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Creates multiple document assistants for all target asset nodes in the given canvas selection.
    /// </summary>
    /// <param name="selection">The canvas context containing the target documents.</param>
    /// <returns>An array of <see cref="AIDocumentAssistant"/> instances, using <see cref="EmptyDocumentAssistant"/> as fallback when no matching assistant is found.</returns>
    public AIDocumentAssistant[] CreateDocumentAssistants(CanvasContext selection)
    {
        if (!IsInitialized)
        {
            Initialize();
        }

        if (selection is null || selection.TargetDocuments is null)
        {
            return [];
        }

        var nodes = selection.TargetAssetNodes.ToArray();
        if (nodes.Length == 0)
        {
            return [];
        }

        List<AIDocumentAssistant> assistants = [];

        foreach (var node in nodes)
        {
            var document = node.GetTargetDocument();
            if (document != null)
            {
                var assistantType = ResolveServiceType(selection.TargetDocument.GetType());
                if (assistantType != null)
                {
                    var assistant = (AIDocumentAssistant)assistantType.CreateInstanceOf();
                    var newSel = CanvasContext.Create(selection.Canvas, node);
    
                    assistant.InitializeCanvas(newSel);
                    assistants.Add(assistant);
                    continue;
                }
            }

            // Failed to find the corresponding assistant
            var emptyAssistant = new EmptyDocumentAssistant();
            var emptySel = CanvasContext.Create(selection.Canvas, node);
            emptyAssistant.InitializeCanvas(emptySel);
            assistants.Add(emptyAssistant);
        }

        return assistants.ToArray();
    }

    /// <summary>
    /// Creates a RAG (Retrieval-Augmented Generation) assistant for the given canvas selection.
    /// </summary>
    /// <param name="selection">The canvas context for the RAG assistant.</param>
    /// <returns>Currently always returns null as RAG assistant creation is not yet implemented.</returns>
    public AIDocumentAssistant CreateRAGAssistant(CanvasContext selection)
    {
        return null;

        //if (!IsInitialized)
        //{
        //    Initialize();
        //}

        //if (selection?.TargetDocument is null)
        //{
        //    return null;
        //}

        //var assistant = new AttachedRAGAssistant();
        //assistant.InitializeCanvas(selection);

        //return assistant;
    }

    /// <inheritdoc/>
    protected override Type GetTargetType(AIAssistantUsageAttribute attribute)
    {
        return attribute.ObjectType;
    }
}
#endregion

#region AssistantInfoCollection
/// <summary>
/// A collection that stores <see cref="AIAssistantInfo"/> instances indexed by assistant type.
/// </summary>
internal class AssistantInfoCollection : IEnumerable<AIAssistantInfo>
{
    readonly Dictionary<Type, AIAssistantInfo> _infosByType = [];

    /// <summary>
    /// Adds an assistant info entry to the collection, keyed by its assistant type.
    /// </summary>
    /// <param name="info">The <see cref="AIAssistantInfo"/> to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
    public void Add(AIAssistantInfo info)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        _infosByType[info.AssistantType] = info;
    }

    #region IEnumerator
    /// <inheritdoc/>
    public IEnumerator<AIAssistantInfo> GetEnumerator()
    {
        return _infosByType.Values.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}
#endregion

#region AIAssistantInfoManager
/// <summary>
/// Manages and provides metadata information about all available AI assistant types.
/// </summary>
internal class AIAssistantInfoManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="AIAssistantInfoManager"/>.
    /// </summary>
    public static AIAssistantInfoManager Instance { get; } = new AIAssistantInfoManager();

    bool _isInitialized;

    readonly Dictionary<string, AIAssistantInfo> _infosByFullName = [];

    readonly Dictionary<Type, AIAssistantInfo> _infosByType = [];

    readonly Dictionary<Type, AssistantInfoCollection> _assistantsByInterface = [];

    private AIAssistantInfoManager()
    {
    }

    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        foreach (var type in typeof(AIAssistant).GetDerivedTypes())
        {
            if (type.IsAbstract || !type.IsClass || !type.IsPublic)
            {
                continue;
            }

            string text = type.GetAttributeCached<DisplayTextAttribute>()?.DisplayText;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = type.Name;
            }

            string toolTips = type.GetAttributeCached<ToolTipsTextAttribute>()?.ToolTips;
            if (string.IsNullOrWhiteSpace(toolTips))
            {
                toolTips = $"Assistant for {text}";
            }

            var info = new AIAssistantInfo
            {
                AssistantType = type,
                DisplayText = text,
                ToolTips = toolTips,
            };

            _infosByType.Add(type, info);
            _infosByFullName.Add(type.FullName, info);
        }
    }

    /// <summary>
    /// Retrieves all AI assistant infos that implement or inherit from the specified interface or base type.
    /// </summary>
    /// <typeparam name="T">The interface or base type to filter assistants by.</typeparam>
    /// <returns>An array of <see cref="AIAssistantInfo"/> instances matching the specified type constraint.</returns>
    public AIAssistantInfo[] GetAIAssistantInfos<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_assistantsByInterface.TryGetValue(typeof(T), out var infos))
        {
            return infos.ToArray();
        }

        infos = new AssistantInfoCollection();
        foreach (var info in _infosByType.Values)
        {
            if (typeof(T).IsAssignableFrom(info.AssistantType))
            {
                infos.Add(info);
            }
        }

        _assistantsByInterface.Add(typeof(T), infos);

        return infos.ToArray();
    }

    /// <summary>
    /// Gets the assistant info for the specified assistant type.
    /// </summary>
    /// <param name="assistantType">The type of the AI assistant.</param>
    /// <returns>The <see cref="AIAssistantInfo"/> for the specified type, or null if not found.</returns>
    public AIAssistantInfo GetAssistantInfo(Type assistantType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_infosByType.TryGetValue(assistantType, out var info))
        {
            return info;
        }

        return null;
    }

    /// <summary>
    /// Gets the assistant info for the specified fully qualified type name.
    /// </summary>
    /// <param name="fullTypeName">The fully qualified name of the assistant type.</param>
    /// <returns>The <see cref="AIAssistantInfo"/> for the specified type name, or null if not found.</returns>
    public AIAssistantInfo GetAssistantInfo(string fullTypeName)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_infosByFullName.TryGetValue(fullTypeName, out var info))
        {
            return info;
        }

        return null;
    }
}
#endregion

#region AIToolInfoManager
/// <summary>
/// Manages and provides metadata information about all available AI tool types.
/// </summary>
internal class AIToolInfoManager
{
    /// <summary>
    /// Gets the singleton instance of <see cref="AIToolInfoManager"/>.
    /// </summary>
    public static AIToolInfoManager Instance { get; } = new();

    bool _isInitialized;

    readonly Dictionary<string, AIToolInfo> _infosByFullName = [];

    readonly Dictionary<Type, AIToolInfo> _infosByType = [];

    readonly UniqueMultiDictionary<Type, AIToolInfo> _infosByParameterType = new();


    private AIToolInfoManager()
    {
    }


    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        foreach (var type in typeof(AITool<>).GetDerivedTypes())
        {
            if (type.IsAbstract || !type.IsClass || !type.IsPublic)
            {
                continue;
            }

            Type baseToolType = GetGenericType(type, typeof(AITool<>));

            string text = type.GetAttributeCached<DisplayTextAttribute>()?.DisplayText;
            if (string.IsNullOrWhiteSpace(text))
            {
                text = type.Name;
            }

            string toolTips = type.GetAttributeCached<ToolTipsTextAttribute>()?.ToolTips;
            if (string.IsNullOrWhiteSpace(toolTips))
            {
                toolTips = $"Tool for {text}";
            }

            Type targetType = null;
            if (typeof(AITool<,>).IsAssignableFrom(type))
            {
                var targetToolType = GetGenericType(type, typeof(AITool<,>));
                targetType = targetToolType.GetGenericArguments()[1];
            }

            var info = new AIToolInfo
            {
                ToolType = type,
                ParameterType = baseToolType.GetGenericArguments()[0],
                DocumentType = targetType,
                DisplayText = text,
                ToolTips = toolTips,
            };

            _infosByType.Add(type, info);
            _infosByParameterType.Add(info.ParameterType, info);
            _infosByFullName.Add(type.FullName, info);
        }
    }

    /// <summary>
    /// Gets the tool info for the specified tool type.
    /// </summary>
    /// <param name="toolType">The type of the AI tool.</param>
    /// <returns>The <see cref="AIToolInfo"/> for the specified type, or null if not found.</returns>
    public AIToolInfo GetToolInfo(Type toolType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_infosByType.TryGetValue(toolType, out var info))
        {
            return info;
        }

        return null;
    }


    /// <summary>
    /// Gets the first tool info that handles the specified parameter type without a document type constraint.
    /// </summary>
    /// <typeparam name="T">The parameter type to search for.</typeparam>
    /// <returns>The matching <see cref="AIToolInfo"/>, or null if none is found.</returns>
    public AIToolInfo GetToolForParameter<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = _infosByParameterType[typeof(T)]
            .Where(o => o.DocumentType is null)
            .FirstOrDefault(); ;

        return info;
    }

    /// <summary>
    /// Gets the first tool info that handles the specified parameter type without a document type constraint.
    /// </summary>
    /// <param name="paramType">The parameter type to search for.</param>
    /// <returns>The matching <see cref="AIToolInfo"/>, or null if none is found.</returns>
    public AIToolInfo GetToolInfoByParameter(Type paramType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = _infosByParameterType[paramType]
            .Where(o => o.DocumentType is null)
            .FirstOrDefault();

        return info;
    }

    /// <summary>
    /// Gets the first tool info that handles the specified parameter type and is compatible with the given document type.
    /// </summary>
    /// <param name="paramType">The parameter type to search for.</param>
    /// <param name="documentType">The document type that the tool must support.</param>
    /// <returns>The matching <see cref="AIToolInfo"/>, or null if none is found.</returns>
    public AIToolInfo GetToolInfoByParameter(Type paramType, Type documentType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = _infosByParameterType[paramType]
            .Where(o => o.DocumentType is not null && documentType.IsAssignableFrom(o.DocumentType))
            .FirstOrDefault();

        return info;
    }


    /// <summary>
    /// Creates an AI tool instance for the specified parameter type.
    /// </summary>
    /// <param name="parameterType">The parameter type to determine which tool to create.</param>
    /// <returns>A new <see cref="AITool"/> instance, or null if no matching tool is found.</returns>
    public AITool CreateTool(Type parameterType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = GetToolInfoByParameter(parameterType);
        if (info is null)
        {
            return null;
        }

        return (AITool)Activator.CreateInstance(info.ToolType);
    }

    /// <summary>
    /// Creates an AI tool instance for the specified parameter type and document type.
    /// </summary>
    /// <param name="parameterType">The parameter type to determine which tool to create.</param>
    /// <param name="documentType">The document type that the tool must support.</param>
    /// <returns>A new <see cref="AITool"/> instance, or null if no matching tool is found.</returns>
    public AITool CreateTool(Type parameterType, Type documentType)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = GetToolInfoByParameter(parameterType, documentType);
        if (info is null)
        {
            return null;
        }

        return (AITool)Activator.CreateInstance(info.ToolType);
    }

    /// <summary>
    /// Creates a generic AI tool instance for the specified parameter type.
    /// </summary>
    /// <typeparam name="T">The parameter type to determine which tool to create.</typeparam>
    /// <returns>A new <see cref="AITool{T}"/> instance, or null if no matching tool is found.</returns>
    public AITool<T> CreateTool<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        var info = GetToolInfoByParameter(typeof(T));
        if (info is null)
        {
            return null;
        }

        return (AITool<T>)Activator.CreateInstance(info.ToolType);
    }

    /// <summary>
    /// Gets the tool info for the specified fully qualified type name.
    /// </summary>
    /// <param name="fullTypeName">The fully qualified name of the tool type.</param>
    /// <returns>The <see cref="AIToolInfo"/> for the specified type name, or null if not found.</returns>
    public AIToolInfo GetToolInfo(string fullTypeName)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_infosByFullName.TryGetValue(fullTypeName, out var info))
        {
            return info;
        }

        return null;
    }


    /// <summary>
    /// Searches the inheritance hierarchy of a type to find a generic type matching the specified generic definition.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="genericDefinitionType">The generic type definition to look for.</param>
    /// <returns>The matching generic type, or null if not found.</returns>
    public static Type GetGenericType(Type type, Type genericDefinitionType)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericDefinitionType)
            {
                return type;
            }
            type = type.BaseType;
        }
        return null; // Generic definition not found
    }
} 
#endregion
