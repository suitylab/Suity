namespace Suity.Synchonizing;

/// <summary>
/// Interface for sync nodes that can contain lists
/// </summary>
public interface ISyncNode : ISyncObject
{
    ISyncList GetList();
}