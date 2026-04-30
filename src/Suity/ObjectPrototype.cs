namespace Suity;

/// <summary>
/// Represents a prototype that can create objects.
/// Used for object instantiation patterns.
/// </summary>
public abstract class ObjectPrototype
{
    public virtual object CreateObject() => null;
}