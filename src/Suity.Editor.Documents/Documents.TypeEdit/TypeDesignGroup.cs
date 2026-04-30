using Suity.Editor.Design;
using Suity.Editor.Types;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents a group container in the type design document.
/// </summary>
[NativeAlias("TypeEditGroup")]
[NativeAlias("Suity.Editor.Documents.TypeEdit.TypeEditGroup")]
[DisplayText("Group", "*CoreIcon|Group")]
public class TypeDesignGroup : DesignGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDesignGroup"/> class.
    /// </summary>
    public TypeDesignGroup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDesignGroup"/> class with the specified group name.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    public TypeDesignGroup(string groupName) 
        : base(groupName)
    {
    }
}
