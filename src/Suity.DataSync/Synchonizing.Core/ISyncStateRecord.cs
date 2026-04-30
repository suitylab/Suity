namespace Suity.Synchonizing.Core;

public interface ISyncStateRecord
{
    void Record(ISyncObject obj);

    void Record(ISyncList list);
}