using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents a function definition in a type design document.
/// </summary>
[NativeAlias]
[DisplayOrder(970)]
[DisplayText("Function", "*CoreIcon|Function")]
public class ClassFunction : FunctionDesignItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassFunction"/> class.
    /// </summary>
    public ClassFunction()
    {
        SupportParameter = true;
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        
        bool isPublic = AccessMode == AssetAccessMode.Public;
        isPublic = sync.Sync("IsPublic", isPublic);
        AccessMode = isPublic ? AssetAccessMode.Public : AssetAccessMode.Private;
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorFieldOf<bool>(new ViewProperty("IsPublic", "Public"));
        setup.InspectorField(ActionMode, new ViewProperty("ActionMode", "Action Mode"));
    }
}
