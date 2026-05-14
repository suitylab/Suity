using Suity.Editor.Selecting;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Synchonizing;
using System;

namespace Suity.Editor.Types;

public class DImplementationSelection : AssetSelection<DStruct>
{
    public DImplementationSelection()
    {
    }

    public DImplementationSelection(DAbstract baseType)
    {
        BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
    }

    public DImplementationSelection(TypeDefinition typeDef)
    {
        BaseType = typeDef.Target as DAbstract
            ?? throw new ArgumentNullException(nameof(typeDef));
    }

    public DAbstract BaseType { get; set; }

    public override ISelectionList GetSelectionList()
    {
        return BaseType?.Definition?.GetSelectionList(this.Filter) ?? EmptySelectionList.Empty;
    }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            BaseType = sync.Sync(nameof(BaseType), BaseType, SyncFlag.AttributeMode | SyncFlag.ByRef);
        }
    }
}
