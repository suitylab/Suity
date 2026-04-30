using Suity.Editor.Design;
using Suity.Editor.Types;
using System.Collections.Generic;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides an interface for retrieving data knowledge associated with a data item.
/// </summary>
public interface IDataKnowledge
{
    /// <summary>
    /// Gets the knowledge description for a specific data item and compound data type.
    /// </summary>
    /// <param name="data">The data item to get knowledge for.</param>
    /// <param name="dataType">The compound data type.</param>
    /// <returns>A string containing the knowledge description for the data item.</returns>
    string GetDataKnowledge(IDataItem data, DCompond dataType);
}

/// <summary>
/// Provides an interface for composing and accessing data-related information in a document context.
/// </summary>
public interface IDataCompose : IDataKnowledge
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    string ProductId { get; }

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    string ProductName { get; }

    /// <summary>
    /// Gets the namespace associated with the data composition.
    /// </summary>
    string NameSpace { get; }

    /// <summary>
    /// Gets the narrative overview of the document or product.
    /// </summary>
    string NarrativeOverview { get; }

    /// <summary>
    /// Gets the design overview of the document or product.
    /// </summary>
    string DesignOverview { get; }

    /// <summary>
    /// Gets the combined expert overview from all relevant experts.
    /// </summary>
    string AllExpertOverview { get; }

    /// <summary>
    /// Gets the canvas document associated with this data composition.
    /// </summary>
    ICanvasDocument Canvas { get; }

    /// <summary>
    /// Gets or sets the linked data handler for managing data relationships.
    /// </summary>
    ILinkedDataHandler LinkedDataHandler { get; set; }

    /// <summary>
    /// Gets the data document for the specified usage mode and data type.
    /// </summary>
    /// <param name="mode">The data usage mode.</param>
    /// <param name="dataType">The compound data type.</param>
    /// <param name="autoCreate">Whether to automatically create the document if it does not exist.</param>
    /// <returns>The data container for the specified parameters.</returns>
    IDataContainer GetDataDocument(DataUsageMode mode, DCompond dataType, bool autoCreate = false);

    /// <summary>
    /// Gets the data document for the specified usage, usage mode, and data type.
    /// </summary>
    /// <param name="mode">The data usage mode.</param>
    /// <param name="usage">The specific usage identifier.</param>
    /// <param name="dataType">The compound data type.</param>
    /// <param name="autoCreate">Whether to automatically create the document if it does not exist.</param>
    /// <returns>The data container for the specified parameters.</returns>
    IDataContainer GetDataDocument(DataUsageMode mode, string usage, DCompond dataType, bool autoCreate = false);

    /// <summary>
    /// Gets the table identifier for the specified compound data type.
    /// </summary>
    /// <param name="dataType">The compound data type.</param>
    /// <returns>The table identifier string.</returns>
    string GetDataTableId(DCompond dataType);

    /// <summary>
    /// Gets the data fill plan for the specified compound data type.
    /// </summary>
    /// <param name="dataType">The compound data type.</param>
    /// <returns>The data table plan for the specified type.</returns>
    IDataTablePlan GetDataFillPlan(DCompond dataType);
}

/// <summary>
/// Provides an interface for accessing system-level information and writing.
/// </summary>
public interface IDataSystem
{
    /// <summary>
    /// Gets the name of the system.
    /// </summary>
    string SystemName { get; }

    /// <summary>
    /// Gets the narrative writing content for the system.
    /// </summary>
    string NarrativeWriting { get; }

    /// <summary>
    /// Gets the design writing content for the system.
    /// </summary>
    string DesignWriting { get; }

    /// <summary>
    /// Gets the guiding information for the system.
    /// </summary>
    string Guiding { get; }
}

/// <summary>
/// Provides an interface for accessing data table plan information.
/// </summary>
public interface IDataTablePlan
{
    /// <summary>
    /// Gets the unique identifier of the table.
    /// </summary>
    string TableId { get; }

    /// <summary>
    /// Gets the collection of data names in this table plan.
    /// </summary>
    IEnumerable<string> DataNames { get; }

    /// <summary>
    /// Checks whether the table plan contains a data item with the specified name.
    /// </summary>
    /// <param name="name">The data name to check.</param>
    /// <returns>True if the data name exists in the plan; otherwise, false.</returns>
    bool ContainsDataName(string name);

    /// <summary>
    /// Checks whether the table plan contains a data item with the specified ID.
    /// </summary>
    /// <param name="dataId">The data ID to check.</param>
    /// <returns>True if the data ID exists in the plan; otherwise, false.</returns>
    bool ContainsDataId(string dataId);

    /// <summary>
    /// Gets the tag string associated with a named data item.
    /// </summary>
    /// <param name="name">The name of the data item.</param>
    /// <returns>The tag string for the specified data name.</returns>
    string GetDataTagString(string name);
}

/// <summary>
/// Provides an interface for accessing a complete data plan including items and metadata.
/// </summary>
public interface IDataPlan
{
    /// <summary>
    /// Gets the data composition associated with this plan.
    /// </summary>
    IDataCompose Compose { get; }

    /// <summary>
    /// Gets the compound data type for this plan.
    /// </summary>
    DCompond DataType { get; }

    /// <summary>
    /// Gets the data usage mode for this plan.
    /// </summary>
    DataUsageMode DataUsage { get; }

    /// <summary>
    /// Gets the data document associated with this plan.
    /// </summary>
    IDataDocument DataDocument { get; }

    /// <summary>
    /// Gets the name of the group this plan belongs to.
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// Gets the overall guiding information for this plan.
    /// </summary>
    string OverallGuiding { get; }

    /// <summary>
    /// Gets or sets the plan knowledge content.
    /// </summary>
    string PlanKnowledge { get; set; }

    /// <summary>
    /// Gets a value indicating whether tooltips should be recorded.
    /// </summary>
    bool RecordTooltips { get; }

    /// <summary>
    /// Gets a value indicating whether knowledge should be generated.
    /// </summary>
    bool GenerateKnowledge { get; }

    /// <summary>
    /// Gets a value indicating whether complex fields should be generated.
    /// </summary>
    bool GenerateComplexField { get; }

    /// <summary>
    /// Gets the total number of items in this plan.
    /// </summary>
    int ItemCount { get; }

    /// <summary>
    /// Gets the collection of items in this plan.
    /// </summary>
    IEnumerable<IDataPlanItem> Items { get; }

    /// <summary>
    /// Builds and returns the data plan as a formatted string.
    /// </summary>
    /// <returns>A string representation of the data plan.</returns>
    string BuildDataPlan();
}

/// <summary>
/// Provides an interface for accessing individual data plan item information.
/// </summary>
public interface IDataPlanItem
{
    /// <summary>
    /// Gets the name of the data plan item.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the localized name of the data plan item.
    /// </summary>
    string LocalName { get; }

    /// <summary>
    /// Gets the description of the data plan item.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets or sets the knowledge associated with this data plan item.
    /// </summary>
    string DataKnowledge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item has been generated.
    /// </summary>
    bool IsGenerated { get; set; }
}
