using Suity.Editor.Types;
using Suity.Networking;
using Suity.NodeQuery;
using System;

namespace Suity.Editor.Services;

/// <summary>
/// Event arguments for console command events.
/// </summary>
[Serializable]
public class CommandEventArgs : EventArgs
{
    /// <summary>
    /// Gets the command string.
    /// </summary>
    public string Command { get; private set; }

    /// <summary>
    /// Initializes a new instance of the CommandEventArgs class.
    /// </summary>
    /// <param name="command">The command string.</param>
    public CommandEventArgs(string command)
    {
        Command = command;
    }
}

/// <summary>
/// Interface for objects that can be monitored.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Monitorable interface", Color = "#99FF00")]
public interface IMonitable
{
    /// <summary>
    /// Gets the name of the monitorable object.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the monitor.
    /// </summary>
    IMonitor Monitor { get; set; }

    /// <summary>
    /// Requests a console command to be executed.
    /// </summary>
    /// <param name="command">The command to request.</param>
    void RequestConsoleCommand(string command);
}

/// <summary>
/// Interface for monitoring runtime operations, network traffic, and resource usage.
/// </summary>
public interface IMonitor : IRuntimeLog, INetworkLog, IOperationLog, IResourceLog, IEntityLog
{
    /// <summary>
    /// Clears all monitoring data.
    /// </summary>
    void Clear();

    /// <summary>
    /// Sets module data for monitoring.
    /// </summary>
    /// <param name="node">The node reader containing module data.</param>
    void SetModuleData(INodeReader node);

    /// <summary>
    /// Adds command hints for auto-complete.
    /// </summary>
    /// <param name="hints">The command hints.</param>
    void AddCommandHints(string[] hints);

    /// <summary>
    /// Clears all command hints.
    /// </summary>
    void ClearCommandHints();

    /// <summary>
    /// Gets the resource count for the specified ID.
    /// </summary>
    /// <param name="id">The resource ID.</param>
    /// <returns>The resource count.</returns>
    int GetResourceCount(Guid id);

    /// <summary>
    /// Event raised when the module object is being updated.
    /// </summary>
    event EventHandler UpdatingModuleObject;

    /// <summary>
    /// Event raised when a console command is requested.
    /// </summary>
    event EventHandler<CommandEventArgs> RequestingConsoleCommand;
}

/// <summary>
/// Service interface for managing monitors.
/// </summary>
public interface IMonitorService
{
    /// <summary>
    /// Shows the monitor for a specific monitorable object.
    /// </summary>
    /// <param name="monitable">The object to monitor.</param>
    void ShowMonitor(IMonitable monitable);

    /// <summary>
    /// Clears the current monitor.
    /// </summary>
    void ClearMonitor();

    /// <summary>
    /// Creates a new monitor instance.
    /// </summary>
    /// <returns>A new monitor instance.</returns>
    IMonitor CreateMonitor();

    /// <summary>
    /// Gets the current monitor.
    /// </summary>
    IMonitor Current { get; }

    /// <summary>
    /// Event raised when the monitor changes.
    /// </summary>
    event Action<IMonitor> MonitorChanged;

    /// <summary>
    /// Event raised when resource log is updated.
    /// </summary>
    event Action ResourceLogUpdated;
}

/// <summary>
/// Empty implementation of the monitor service.
/// </summary>
public sealed class EmptyMonitorService : IMonitorService
{
    /// <summary>
    /// Gets the singleton instance of EmptyMonitorService.
    /// </summary>
    public static readonly EmptyMonitorService Empty = new();

    private EmptyMonitorService()
    {
    }

    /// <inheritdoc/>
    public void ShowMonitor(IMonitable monitable)
    {
    }

    /// <inheritdoc/>
    public void ClearMonitor()
    {
    }

    /// <inheritdoc/>
    public IMonitor CreateMonitor()
    {
        return null;
    }

    /// <inheritdoc/>
    public IMonitor Current { get; }

    /// <inheritdoc/>
    public event Action<IMonitor> MonitorChanged;

    /// <inheritdoc/>
    public event Action ResourceLogUpdated;
}

/// <summary>
/// Interface for sending data over a channel.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Data transport", Color = "#9900FF")]
public interface IDataTransport
{
    /// <summary>
    /// Sends data over the specified channel.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="channel">The channel number.</param>
    void SendData(object data, int channel);
}

/// <summary>
/// Record for setting a float value via transport.
/// </summary>
public record SetFloatValueTransport(int Channel, string Name, float Value);

/// <summary>
/// Record for setting an action via transport.
/// </summary>
public record SetActionTransport(int Channel, string Name, string Action);


/// <summary>
/// Interface for remote control operations.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Remote control", Color = "#99FF00")]
public interface IRemoteControl : IMonitable, IDataTransport
{
}

/// <summary>
/// Interface for assignable data values.
/// </summary>
public interface IAssignableData
{
    /// <summary>
    /// Gets whether this data can be listened to.
    /// </summary>
    bool CanListen { get; }

    /// <summary>
    /// Gets the listen key.
    /// </summary>
    string ListenKey { get; }

    /// <summary>
    /// Gets the original value.
    /// </summary>
    int OriginValue { get; }

    /// <summary>
    /// Gets the current value.
    /// </summary>
    float Value { get; }
}

/// <summary>
/// Interface for live data.
/// </summary>
public interface ILiveData
{
}

/// <summary>
/// Represents the state of an assignment.
/// </summary>
public enum AssignState
{
    /// <summary>
    /// The value is the same.
    /// </summary>
    Same,

    /// <summary>
    /// The value has been assigned.
    /// </summary>
    Assigned,

    /// <summary>
    /// The value has been unassigned.
    /// </summary>
    Unassigned,
}

/// <summary>
/// Interface for assignable control.
/// </summary>
public interface IAssignableControl
{
    /// <summary>
    /// Gets the listen key.
    /// </summary>
    string ListenKey { get; }

    /// <summary>
    /// Assigns data to this control.
    /// </summary>
    /// <param name="data">The data to assign.</param>
    /// <returns>The assignment state.</returns>
    AssignState Assign(IAssignableData data);

    /// <summary>
    /// Handles data input.
    /// </summary>
    /// <param name="liveValue">The live data value.</param>
    /// <param name="data">The assignable data.</param>
    /// <param name="channel">The channel number.</param>
    void HandleDataInput(ILiveData liveValue, IAssignableData data, int channel);
}

/// <summary>
/// Interface for live command execution.
/// </summary>
public interface ILiveCommand
{
    /// <summary>
    /// Executes a live command.
    /// </summary>
    /// <param name="liveData">The live data.</param>
    /// <param name="value">The value.</param>
    /// <param name="originData">The origin data.</param>
    /// <param name="originChannel">The origin channel.</param>
    void DoLiveCommand(ILiveData liveData, float value, object originData, int originChannel);
}

/// <summary>
/// Interface for control automation.
/// </summary>
public interface IControlAutomation
{
    /// <summary>
    /// Handles automation input.
    /// </summary>
    /// <param name="liveValue">The live data value.</param>
    /// <param name="value">The value.</param>
    /// <param name="depth">The depth.</param>
    /// <param name="fromAutomation">The automation source.</param>
    void HandleAutomation(ILiveData liveValue, float value, int depth, string fromAutomation);
}