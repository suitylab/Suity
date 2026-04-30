using Suity.Synchonizing;

namespace Suity.Views;

public interface IViewList : ISyncList, IDropInCheck
{
    int ListViewId { get; }
}