using System;

namespace Suity.Editor;

public interface IAssetUpdateAction<TAsset>
    where TAsset : Asset, new()
{
    void DoAction(TAsset asset);
}

public interface IValueUpdateAction
{
}

public interface IValueUpdateAction<TValue> : IValueUpdateAction
{
    void UpdateValue(TValue value);
}

public interface IRefUpdateAction<TRef> : IValueUpdateAction<TRef>
{
    void UpdateId(Guid id);
}