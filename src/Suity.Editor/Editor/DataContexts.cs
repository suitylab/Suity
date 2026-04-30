using Suity.Editor.Design;
using Suity.Editor.Types;

namespace Suity.Editor;

/// <summary>
/// Data table asset。
/// To support language rendering other than json format, you need to inherit <see cref="DataTableAsset"/>
/// </summary>
public interface IDataTableAsset : IHasId
{
    /// <summary>
    /// Get data container
    /// </summary>
    /// <param name="tryLoadStorage">Try load data from storage if it's not loaded</param>
    /// <returns></returns>
    IDataContainer GetDataContainer(bool tryLoadStorage);
}

public interface IDataAsset : IHasId
{
    TypeDefinition[] GetDataTypes();

    bool SupportType(TypeDefinition type);

    IDataItem GetData(bool tryLoadStorage);
}