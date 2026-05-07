using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Interface for assistants that can create root-level content and resume interrupted creation workflows.
/// </summary>
public interface IRootCreatorAssistant
{
    /// <summary>
    /// Handles the creation of new root-level content.
    /// </summary>
    /// <param name="request">The AI request containing user instructions.</param>
    /// <returns>The result of the creation operation.</returns>
    Task<AICallResult> HandleRootCreate(AIRequest request);

    /// <summary>
    /// Handles resuming an interrupted creation workflow with existing canvas context.
    /// </summary>
    /// <param name="request">The AI request containing user instructions.</param>
    /// <param name="canvasContext">The existing canvas context to resume from.</param>
    /// <returns>The result of the resume operation.</returns>
    Task<AICallResult> HandleRootResume(AIRequest request, CanvasContext canvasContext);
}

/// <summary>
/// Interface for assistants that can update existing root-level content.
/// </summary>
public interface IRootUpdaterAssistant
{
    /// <summary>
    /// Handles updating existing root-level content.
    /// </summary>
    /// <param name="request">The AI request containing update instructions.</param>
    /// <returns>The result of the update operation.</returns>
    Task<AICallResult> HandleRootUpdate(AIRequest request);
}

/// <summary>
/// Interface for assistants that handle document-level element operations.
/// </summary>
public interface IDocumentAssistant
{
    /// <summary>
    /// Handles creating a new element in the document.
    /// </summary>
    /// <param name="request">The AI request containing creation instructions.</param>
    /// <returns>The result of the create operation.</returns>
    Task<AICallResult> HandleElementCreate(AIRequest request);

    /// <summary>
    /// Handles batch creation of multiple elements.
    /// </summary>
    /// <param name="request">The AI request containing batch creation instructions.</param>
    /// <param name="prompts">Array of guiding items for each element to create.</param>
    /// <param name="componentId">Optional component type ID for the data.</param>
    /// <param name="groupPath">Optional group path where elements should be placed.</param>
    /// <param name="recordKnowledge">Whether to record knowledge on created elements.</param>
    /// <returns>The result of the batch create operation.</returns>
    Task<AICallResult> HandleBatchCreate(AIRequest request, GenerativeGuidingItem[] prompts, Guid? componentId = null, string groupPath = null, bool recordKnowledge = false);

    /// <summary>
    /// Handles editing an existing element in the document.
    /// </summary>
    /// <param name="request">The AI request containing edit instructions.</param>
    /// <returns>The result of the edit operation.</returns>
    Task<AICallResult> HandleElementEdit(AIRequest request);
}

/// <summary>
/// Interface for assistants that handle complex field generation.
/// </summary>
public interface IComplexFieldAssistant
{
    /// <summary>
    /// Handles generating content for complex fields.
    /// </summary>
    /// <param name="request">The AI request containing generation instructions.</param>
    /// <param name="itemName">The name of the item containing the complex field.</param>
    /// <param name="fields">Optional specific fields to generate.</param>
    /// <param name="dataKnowledge">Optional data knowledge for context.</param>
    /// <returns>The result of the complex field generation.</returns>
    Task<AICallResult> HandleComplexField(AIRequest request, string itemName, IEnumerable<DStructField> fields = null, IDataKnowledge dataKnowledge = null);
}