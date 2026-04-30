using Suity.NodeQuery;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Gui.InspectorGui;

/// <summary>
/// Test setup for verifying view ID support in the inspector.
/// </summary>
internal class ViewIdTestSetup : IViewObjectSetup
{
    /// <summary>
    /// Gets the view ID this setup is testing.
    /// </summary>
    public int ViewId { get; }

    /// <summary>
    /// Gets or sets whether the view ID is supported.
    /// </summary>
    public bool SupportViewId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewIdTestSetup"/> class.
    /// </summary>
    /// <param name="viewId">The view ID to test.</param>
    public ViewIdTestSetup(int viewId)
    {
        ViewId = viewId;
    }

    /// <inheritdoc/>
    public INodeReader Styles => EmptyNodeReader.Empty;

    /// <inheritdoc/>
    public object Parent => throw new NotImplementedException();

    /// <inheritdoc/>
    public void AddField(Type type, ViewProperty property)
    {
        if (property.ViewId == ViewId)
        {
            SupportViewId = true;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<object> GetObjects() => [];

    /// <inheritdoc/>
    public object GetService(Type serviceType) => null;

    /// <inheritdoc/>
    public bool IsTypeSupported(Type type) => false;

    /// <inheritdoc/>
    public bool IsViewIdSupported(int viewId) => viewId == ViewId;
}
