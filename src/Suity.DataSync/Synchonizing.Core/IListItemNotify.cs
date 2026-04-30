namespace Suity.Synchonizing.Core;

public interface IListItemNotify<T>
{
    void NotifyItemAdded(T item);

    void NotifyItemRemoved(T item);
}