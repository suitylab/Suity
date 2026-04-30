using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// A workspace reference item that represents a user code library.
/// This item does not produce render targets directly but provides user code context.
/// </summary>
public class UserFileRefItem : WorkSpaceRefItem
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserFileRefItem"/>.
    /// </summary>
    public UserFileRefItem()
    {
    }

    /// <inheritdoc/>
    public override Guid Id
    {
        get => UserCodeId;
        set => UserCodeId = value;
    }

    /// <inheritdoc/>
    public override int Order => 2;

    /// <inheritdoc/>
    public override bool UploadMode => false;

    /// <inheritdoc/>
    public override IEnumerable<RenderTarget> GetRenderTargets()
    {
        return [];
    }
}