using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a container of event handlers.
/// </summary>
public interface IHandlerContainer
{
    /// <summary>
    /// Gets all handler definitions.
    /// </summary>
    IEnumerable<string> HandlerDefinitions { get; }

    /// <summary>
    /// Checks if a handler definition exists.
    /// </summary>
    bool ContainsHandlerDefinition(string handlerName);

    /// <summary>
    /// Gets a handler function by name.
    /// </summary>
    IFunction GetHandlerFunction(string name);
}

/// <summary>
/// Represents a controller that handles logic.
/// </summary>
public interface IController : IMember, IHandlerContainer
{
    /// <summary>
    /// Gets the parent state.
    /// </summary>
    IState ParentState { get; }

    /// <summary>
    /// Gets whether the controller is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the type of the controller.
    /// </summary>
    TypeDefinition ControllerType { get; }

    /// <summary>
    /// Gets all controller field inputs.
    /// </summary>
    IEnumerable<IFieldInput> ControllerFields { get; }
}

/// <summary>
/// Represents a container of controllers.
/// </summary>
public interface IControllerContainer : IFieldContainer, IFunctionContainer, IHandlerContainer, IHasId
{
    /// <summary>
    /// Gets all controllers.
    /// </summary>
    IEnumerable<IController> Controllers { get; }

    /// <summary>
    /// Gets all triggers.
    /// </summary>
    IEnumerable<ITrigger> Triggers { get; }

    /// <summary>
    /// Gets all states.
    /// </summary>
    IEnumerable<IState> States { get; }

    /// <summary>
    /// Gets all state machines.
    /// </summary>
    IEnumerable<IStateMachine> StateMachines { get; }

    /// <summary>
    /// Gets a controller by name.
    /// </summary>
    IController GetController(string name);

    /// <summary>
    /// Gets a trigger by name.
    /// </summary>
    ITrigger GetTrigger(string name);

    /// <summary>
    /// Gets a state machine by name.
    /// </summary>
    IStateMachine GetStateMachine(string name);

    /// <summary>
    /// Gets a state by name.
    /// </summary>
    IState GetState(string name);

    /// <summary>
    /// Gets the start function.
    /// </summary>
    IFunction StartFunction { get; }

    /// <summary>
    /// Gets the stop function.
    /// </summary>
    IFunction StopFunction { get; }

    /// <summary>
    /// Gets the enter function.
    /// </summary>
    IFunction EnterFunction { get; }

    /// <summary>
    /// Gets the exit function.
    /// </summary>
    IFunction ExitFunction { get; }

    /// <summary>
    /// Gets the update function.
    /// </summary>
    IFunction UpdateFunction { get; }
}

/// <summary>
/// Represents a state in a state machine.
/// </summary>
public interface IState : IMember, IHandlerContainer
{
    /// <summary>
    /// Gets the parent state machine.
    /// </summary>
    IStateMachine ParentStateMachine { get; }

    /// <summary>
    /// Gets or sets the auto change to next state time.
    /// </summary>
    float AutoChangeToNextState { get; }

    /// <summary>
    /// Gets all nested state machines.
    /// </summary>
    IEnumerable<IStateMachine> StateMachines { get; }

    /// <summary>
    /// Gets all controllers in this state.
    /// </summary>
    IEnumerable<IController> Controllers { get; }

    /// <summary>
    /// Gets all triggers in this state.
    /// </summary>
    IEnumerable<ITrigger> Triggers { get; }

    /// <summary>
    /// Gets the enter function for this state.
    /// </summary>
    IFunction EnterFunction { get; }

    /// <summary>
    /// Gets the exit function for this state.
    /// </summary>
    IFunction ExitFunction { get; }

    /// <summary>
    /// Gets the update function for this state.
    /// </summary>
    IFunction UpdateFunction { get; }
}

/// <summary>
/// Represents a state machine.
/// </summary>
public interface IStateMachine : IMemberContainer, IMember
{
    /// <summary>
    /// Gets the initial state.
    /// </summary>
    IState ParentState { get; }

    /// <summary>
    /// Gets or sets whether to auto change to first state.
    /// </summary>
    bool AutoChangeToFirstState { get; }

    /// <summary>
    /// Gets all states in this machine.
    /// </summary>
    IEnumerable<IState> States { get; }

    /// <summary>
    /// Gets a state by name.
    /// </summary>
    IState GetState(string name);
}

/// <summary>
/// Represents a trigger that responds to events.
/// </summary>
public interface ITrigger : IMember, IVariableContainer
{
    /// <summary>
    /// Gets the parent state.
    /// </summary>
    IState ParentState { get; }

    /// <summary>
    /// Gets whether the trigger is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets all events for this trigger.
    /// </summary>
    IEnumerable<DEvent> Events { get; }

    /// <summary>
    /// Gets the conditions for this trigger.
    /// </summary>
    SArray Conditions { get; }

    /// <summary>
    /// Gets the actions for this trigger.
    /// </summary>
    SArray Actions { get; }
}

/// <summary>
/// Represents a template for trigger controller expressions.
/// </summary>
[AssetTypeBinding("TriggerControllerTemplate", "Trigger Controller Template")]
[Obsolete]
public interface ITriggerControllerTemplate
{
    /// <summary>
    /// Gets an expression for the controller container.
    /// </summary>
    ExpressionNode GetExpression(IControllerContainer ctrl, ExpressionContext option);
}