namespace Suity;

/// <summary>
/// Specifies the type of action performed on an entity.
/// </summary>
public enum EntityActionTypes
{
    CreateEntity,
    DestroyEntity,
    AddOrReplaceValue,
    RemoveValue,

    AddedToLogic,
    RemovedFromLogic,

    Message,
}