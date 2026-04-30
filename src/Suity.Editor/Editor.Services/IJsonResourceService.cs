using ComputerBeacon.Json;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Options for JSON resource operations.
/// </summary>
public class JsonResourceOptions
{
    /// <summary>
    /// Field writable filter
    /// </summary>
    public Predicate<DStructField> FieldFilter { get; set; }

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    public ICondition Condition { get; set; }

    /// <summary>
    /// Gets or sets the render config.
    /// </summary>
    public RenderConfig RenderConfig { get; set; }

    /// <summary>
    /// Gets or sets whether to log errors.
    /// </summary>
    public bool LogError { get; set; } = true;
}

/// <summary>
/// Options for SItem resource operations.
/// </summary>
public class SItemResourceOptions
{
    /// <summary>
    /// Gets or sets the type hint.
    /// </summary>
    public TypeDefinition TypeHint { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically add new enum values.
    /// </summary>
    public bool AutoAddNewEnumValue { get; set; }
}

public interface IJsonResourceService
{
    bool TryExtractJson(string s, out JsonObject obj);

    JsonObject GetJson(Guid id, string materialName = null, JsonResourceOptions options = null);

    JsonObject GetJson(Guid id, IRenderable renderable, IMaterial material, JsonResourceOptions options = null);

    JsonObject GetJson(IDataInputOwner dataInputOwner, JsonResourceOptions options = null);

    JsonObject GetJson(IDataInput dataInput, JsonResourceOptions options = null);

    JsonObject GetJson(IDataItem row, JsonResourceOptions options = null);

    JsonArray GetJsonComponents(IDataItem row, JsonResourceOptions options = null);

    object GetJson(SItem item, bool writeTypeName = true, JsonResourceOptions options = null);

    SItem FromJson(object obj, SItemResourceOptions options = null);

    Dictionary<string, object> FromJson(JsonObject obj, SimpleType type, SItemResourceOptions options = null);
}