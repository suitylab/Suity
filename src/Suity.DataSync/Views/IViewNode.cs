using Suity.Synchonizing;

namespace Suity.Views;

public interface IViewNode : ISyncNode, IDropInCheck
{
    int ListViewId { get; }
}