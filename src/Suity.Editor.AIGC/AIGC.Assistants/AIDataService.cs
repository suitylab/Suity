using ComputerBeacon.Json;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Abstract service for handling AI-driven data generation and manipulation.
/// </summary>
public abstract class AIDataService
{
    internal static AIDataService _external;

    /// <summary>
    /// Gets the singleton instance of the data service.
    /// </summary>
    public static AIDataService Instance => _external;

    /// <summary>
    /// Batch generates data items according to the specified plan.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="dataPlan">The plan defining what data to generate.</param>
    /// <param name="counter">Optional progress counter.</param>
    public abstract Task BatchGenerate(AIRequest request, IDataPlan dataPlan, ProgressCounter counter = null);

    /// <summary>
    /// Fills data based on the provided data request.
    /// </summary>
    /// <param name="request">The data request containing generation parameters.</param>
    /// <returns>The result of the data fill operation.</returns>
    public abstract Task<AICallResult> FillData(AIDataRequest request);

    /// <summary>
    /// Fills complex fields on generated objects.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="compose">The data compose providing structure context.</param>
    /// <param name="doc">The target document.</param>
    /// <param name="objs">The objects to fill complex fields on.</param>
    /// <param name="fields">Optional specific fields to fill.</param>
    public abstract Task FillComplexFields(AIRequest request, IDataCompose compose, Document doc, IEnumerable<object> objs, IEnumerable<DStructField> fields = null);

    /// <summary>
    /// Creates a linked data handler for the specified data compose.
    /// </summary>
    /// <param name="compose">The data compose to create a handler for.</param>
    /// <returns>A new linked data handler instance.</returns>
    public abstract ILinkedDataHandler CreateLinkedDataHandler(IDataCompose compose);

    /// <summary>
    /// Generates a JSON object based on the specified type and request parameters.
    /// </summary>
    /// <param name="request">The JSON request containing type and message.</param>
    /// <returns>The generated JSON object.</returns>
    public abstract Task<JsonObject> GenerateJsonObject(AIJsonRequest request);

    /// <summary>
    /// Edits an existing JSON object based on the request parameters.
    /// </summary>
    /// <param name="request">The JSON request containing the object to edit.</param>
    /// <returns>The edited JSON object.</returns>
    public abstract Task<JsonObject> EditJsonObject(AIJsonRequest request);

    /// <summary>
    /// Generate complex fields, including substructures, abstract structures, and links.
    /// Abstract structures and links need to be generated with the help of knowledge base.
    /// </summary>
    /// <param name="request">The complex fields request containing the object and context.</param>
    public abstract Task FillComplexFields(AIComplexFieldsRequest request);

    /// <summary>
    /// Modifies complex fields on an existing object.
    /// </summary>
    /// <param name="request">The complex fields request containing the object to modify.</param>
    public abstract Task ModifyComplexFields(AIComplexFieldsRequest request);
}
