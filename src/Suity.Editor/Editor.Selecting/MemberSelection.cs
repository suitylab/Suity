using Suity.Editor.Design;
using Suity.Synchonizing;
using System;

namespace Suity.Editor.Selecting;

/// <summary>
/// A base class for selections that reference members within a container.
/// </summary>
/// <typeparam name="TContainer">The type of container.</typeparam>
/// <typeparam name="TMember">The type of member.</typeparam>
public abstract class MemberSelection<TContainer, TMember> : EditorObjectSelection<TMember>
    where TContainer : class, IMemberContainer
    where TMember : class, IMember
{
    private Func<TContainer> _containerGetter;

    /// <summary>
    /// Initializes a new instance of the MemberSelection class.
    /// </summary>
    public MemberSelection()
    { }

    /// <summary>
    /// Initializes a new instance of the MemberSelection class with the specified container getter.
    /// </summary>
    /// <param name="containerGetter">The function to get the container.</param>
    public MemberSelection(Func<TContainer> containerGetter)
    {
        _containerGetter = containerGetter ?? throw new ArgumentNullException(nameof(containerGetter));
    }

    /// <inheritdoc />
    public override TMember Target => TargetObject?.GetStorageObject(true) as TMember;

    /// <summary>
    /// Gets the container.
    /// </summary>
    /// <returns>The container.</returns>
    public TContainer GetContainer() => _containerGetter?.Invoke();

    /// <summary>
    /// Gets whether the container getter is defined.
    /// </summary>
    public bool ContainerGetterDefined => _containerGetter != null;

    /// <inheritdoc />
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            _containerGetter = sync.Sync("ContainerGetter", _containerGetter, SyncFlag.AttributeMode | SyncFlag.ByRef);
        }
    }

    /// <summary>
    /// Updates the container getter.
    /// </summary>
    /// <param name="containerGeter">The new container getter.</param>
    public void UpdateContainerGetter(Func<TContainer> containerGeter)
    {
        _containerGetter = containerGeter;
        RepairId();
    }

    /// <inheritdoc />
    protected internal override Guid ResolveId(string key)
    {
        if (_containerGetter == null)
        {
            Logs.LogError("Unable to resolve the key due to container getter is missing.");
            return Guid.Empty;
        }

        TContainer container = _containerGetter();
        if (container == null)
        {
            Logs.LogError("Unable to resolve the key due to container getter is missing.");
            return Guid.Empty;
        }

        return container.GetMember(key)?.Id ?? Guid.Empty;
    }
}