using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Views;

/// <summary>
/// Create new object in GUI
/// </summary>
public interface IHasObjectCreationGUI
{
    IEnumerable<ObjectCreationOption> CreationOptions { get; }

    Task<object> GuiCreateObjectAsync(Type typeHint = null);
}

public record ObjectCreationOption(Type Type, string Text = null, GuiObjectCreation Creation = null);

public delegate Task<object> GuiObjectCreation(Type typeHint = null);