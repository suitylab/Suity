using System.ComponentModel;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Request parameters for creating example data.
/// </summary>
[Description("Tool for creating example data.")]
public class CreateExampleDataRequest
{
    /// <summary>
    /// Gets or sets the location where example data should be created. Leave blank if not specified in the user prompt.
    /// </summary>
    [Description("The location that example data to create, if the location is not concerened in user prompt, leave it blank.")]
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the requirements for the example data. Leave blank if not specified in the user prompt.
    /// </summary>
    [Description("The requirement for example data, if the requirement is not concerened in user prompt, leave it blank.")]
    public string Requirement { get; set; }
}

/// <summary>
/// Request parameters for creating example data on a specific table.
/// </summary>
[Description("Tool for creating example data on a specific table.")]
public class CreateTableExampleDataRequest
{
    /// <summary>
    /// Gets or sets the data type name of the example data to create.
    /// </summary>
    [Description("The data type of example data.")]
    public string DataTypeName { get; set; }

    /// <summary>
    /// Gets or sets the target table name where example data will be created.
    /// </summary>
    [Description("The target table name.")]
    public string NodeName { get; set; }

    /// <summary>
    /// Gets or sets the user requirements for example data generation.
    /// </summary>
    [Description("User requirements for example data generation.")]
    public string Requirement { get; set; }
}
