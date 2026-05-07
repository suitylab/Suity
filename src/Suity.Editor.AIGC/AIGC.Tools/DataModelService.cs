namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Provides an abstract service for parsing and managing data model specifications.
/// </summary>
public abstract class DataModelService
{
    /// <summary>
    /// The external service instance.
    /// </summary>
    internal static DataModelService _external;

    /// <summary>
    /// Gets the current instance of the <see cref="DataModelService"/>.
    /// </summary>
    public static DataModelService Instance => _external;

    /// <summary>
    /// Attempts to parse a <see cref="StructureSegment"/> into a <see cref="StructureSpecification"/>.
    /// </summary>
    /// <param name="seg">The structure segment to parse.</param>
    /// <param name="spec">When this method returns, contains the parsed specification, or <c>null</c> if parsing failed.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public abstract bool TryParseSpecification(StructureSegment seg, out StructureSpecification spec);
}