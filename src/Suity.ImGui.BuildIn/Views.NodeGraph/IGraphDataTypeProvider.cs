using System.Collections.Generic;

namespace Suity.Views.NodeGraph;


/// <summary>
/// Node graph data type provider
/// </summary>
public interface IGraphDataTypeProvider
{
    /// <summary>
    /// Gets the action data type.
    /// </summary>
    GraphDataType? ActionDataType { get; }

    /// <summary>
    /// Gets a data type by name.
    /// </summary>
    /// <param name="name">The name of the data type.</param>
    /// <returns>The data type if found; otherwise, null.</returns>
    GraphDataType? GetDataType(string name);

    /// <summary>
    /// Gets a value indicating whether data array directions should be reverted.
    /// </summary>
    bool RevertDataArray { get; }

    /// <summary>
    /// Determines whether a connection can be made between two data types.
    /// </summary>
    /// <param name="fromDataType">The source data type.</param>
    /// <param name="toDataType">The target data type.</param>
    /// <param name="toMultiple">Whether the target allows multiple connections.</param>
    /// <param name="converted">When this method returns, indicates whether a type conversion is required.</param>
    /// <returns>True if the connection is valid; otherwise, false.</returns>
    bool GetCanConnectTo(GraphDataType fromDataType, GraphDataType toDataType, bool toMultiple, out bool converted);

    /// <summary>
    /// Determines whether an association can be made between two values.
    /// </summary>
    /// <param name="fromDataType">The source data type.</param>
    /// <param name="fromValue">The source value.</param>
    /// <param name="toDataType">The target data type.</param>
    /// <param name="toValue">The target value.</param>
    /// <returns>True if the association is valid; otherwise, false.</returns>
    bool GetCanAssociate(GraphDataType fromDataType, object fromValue, GraphDataType toDataType, object toValue);
}

/// <summary>
/// Default implementation of <see cref="IGraphDataTypeProvider"/> that performs strict type matching.
/// </summary>
public class DefaultGraphDataTypeProvider : IGraphDataTypeProvider
{
    private readonly Dictionary<string, GraphDataType> _dataTypes = [];

    /// <inheritdoc/>
    public GraphDataType? ActionDataType => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGraphDataTypeProvider"/> class.
    /// </summary>
    public DefaultGraphDataTypeProvider()
    {
        RegisterDataType(new GraphDataTypeBase());
    }

    /// <inheritdoc/>
    public GraphDataType? GetDataType(string name)
    {
        if (_dataTypes.TryGetValue(name, out var dataType))
        {
            return dataType;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public bool RevertDataArray => false;

    /// <inheritdoc/>
    public bool GetCanConnectTo(GraphDataType fromDataType, GraphDataType toDataType, bool toMultiple, out bool converted)
    {
        converted = false;

        if (fromDataType is null || toDataType is null)
        {
            return false;
        }

        if (fromDataType != toDataType)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public bool GetCanAssociate(GraphDataType fromDataType, object fromValue, GraphDataType toDataType, object toValue)
    {
        if (fromDataType is null || toDataType is null)
        {
            return false;
        }

        if (fromDataType != toDataType)
        {
            return false;
        }

        if (fromValue is null || toValue is null) 
        {
            return false;
        }


        return fromValue.Equals(toValue);
    }

    /// <summary>
    /// Registers a data type with the provider.
    /// </summary>
    /// <param name="dataType">The data type to register.</param>
    public void RegisterDataType(GraphDataType dataType)
    {
        _dataTypes.Add(dataType.ToString(), dataType);
    }
}
