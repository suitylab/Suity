using Suity.Editor.CodeRender;
using System;

namespace Suity.Editor;

public class UserCodeFilter : IAssetFilter
{
    public UserCodeFilter()
    {
    }

    public UserCodeFilter(Guid affectedId)
    {
        AffectedId = affectedId;
    }

    public Guid AffectedId { get; set; }

    public bool FilterAsset(Asset asset)
    {
        if (asset is not ICodeLibrary userCode)
        {
            return false;
        }

        if (AffectedId != Guid.Empty)
        {
            return userCode.ContainsDependency(AffectedId);
        }
        else
        {
            return false;
        }
    }
}