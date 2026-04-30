using Suity.Synchonizing;

namespace Suity.Views;

public interface IViewObject : ISyncObject
{
    void SetupView(IViewObjectSetup setup);
}