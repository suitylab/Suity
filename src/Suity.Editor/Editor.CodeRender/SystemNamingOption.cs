namespace Suity.Editor.CodeRender;

/// <summary>
/// System naming options for code generation.
/// </summary>
public class SystemNamingOption
{
    /// <summary>
    /// Default instance.
    /// </summary>
    public static readonly SystemNamingOption Instance = new();

    /// <summary>
    /// Creates a new system naming option.
    /// </summary>
    public SystemNamingOption()
    { }

    /// <summary>
    /// Object.ToString method name.
    /// </summary>
    public virtual string ObjectToString => "ToString";

    /// <summary>
    /// FunctionContext type name.
    /// </summary>
    public virtual string FunctionContext => "Suity.FunctionContext";

    /// <summary>
    /// FunctionContext variable name.
    /// </summary>
    public virtual string FunctionContextVarName => "ctx";

    /// <summary>
    /// FunctionContext.GetArgument method name.
    /// </summary>
    public virtual string FunctionContext_GetArgument => "GetArgument";

    /// <summary>
    /// FunctionContext.SetArgument method name.
    /// </summary>
    public virtual string FunctionContext_SetArgument => "SetArgument";

    /// <summary>
    /// System namespace.
    /// </summary>
    public virtual string SystemNameSpace => "Suity";

    /// <summary>
    /// IInitialize interface name.
    /// </summary>
    public virtual string IInitialize => "Suity.IInitialize";

    /// <summary>
    /// ObjectType type name.
    /// </summary>
    public virtual string ObjectType => "Suity.ObjectType";

    /// <summary>
    /// ObjectType.RegisterClassType method name.
    /// </summary>
    public virtual string ObjectType_RegisterClassType => "RegisterClassType";

    /// <summary>
    /// ObjectType.RegisterEnumType method name.
    /// </summary>
    public virtual string ObjectType_RegisterEnumType => "RegisterEnumType";

    /// <summary>
    /// ObjectType.RegisterFunction method name.
    /// </summary>
    public virtual string ObjectType_RegisterFunction => "RegisterFunction";

    /// <summary>
    /// ObjectType.RegisterFamily method name.
    /// </summary>
    public virtual string ObjectType_RegisterFamily => "RegisterFamily";

    /// <summary>
    /// ObjectType.RegisterAssetImplement method name.
    /// </summary>
    public virtual string ObjectType_RegisterAssetImplement => "RegisterAssetImplement";

    /// <summary>
    /// ObjectType.EmptyDelegate method name.
    /// </summary>
    public virtual string ObjectType_EmptyDelegate => "EmptyDelegate";

    /// <summary>
    /// ObjectType.ParseEnum method name.
    /// </summary>
    public virtual string ObjectType_ParseEnum => "ParseEnum";

    /// <summary>
    /// ObjectType.CloneObject method name.
    /// </summary>
    public virtual string ObjectType_CloneObject => "CloneObject";

    /// <summary>
    /// TypeFamily type name.
    /// </summary>
    public virtual string TypeFamily => "Suity.TypeFamily";

    /// <summary>
    /// ClassTypeInfo type name.
    /// </summary>
    public virtual string ClassTypeInfo => "Suity.ClassTypeInfo";

    /// <summary>
    /// EnumTypeInfo type name.
    /// </summary>
    public virtual string EnumTypeInfo => "Suity.EnumTypeInfo";

    /// <summary>
    /// DataStorage type name.
    /// </summary>
    public virtual string DataStorage => "Suity.DataStorage";

    /// <summary>
    /// DataStorage.AddCollection method name.
    /// </summary>
    public virtual string DataStorage_AddCollection => "AddCollection";

    /// <summary>
    /// DataStorage.AddData method name.
    /// </summary>
    public virtual string DataStorage_AddData => "AddData";

    /// <summary>
    /// DataStorage.GetObject method name.
    /// </summary>
    public virtual string DataStorage_GetObject => "GetObject";

    /// <summary>
    /// DataCollection type name.
    /// </summary>
    public virtual string DataCollection => "Suity.DataCollection";

    /// <summary>
    /// DataCollection.AddDataObject method name.
    /// </summary>
    public virtual string DataCollection_AddDataObject => "AddDataObject";

    /// <summary>
    /// DataObject type name.
    /// </summary>
    public virtual string DataObject => "Suity.DataObject";

    /// <summary>
    /// IDataReader interface name.
    /// </summary>
    public virtual string IDataReader => "Suity.IDataReader";

    /// <summary>
    /// IDataWriter interface name.
    /// </summary>
    public virtual string IDataWriter => "Suity.IDataWriter";

    /// <summary>
    /// IDataArrayWriter interface name.
    /// </summary>
    public virtual string IDataArrayWriter => "Suity.IDataArrayWriter";


    /// <summary>
    /// ISaveData interface name.
    /// </summary>
    public virtual string ISaveData => "Suity.Gaming.ISaveData";

    /// <summary>
    /// IGameManager interface name.
    /// </summary>
    public virtual string IGameManager => "Suity.Gaming.IGameManager";

    /// <summary>
    /// IGameManager.GetEntity method name.
    /// </summary>
    public virtual string IGameManager_GetEntity => "GetEntity";

    /// <summary>
    /// Array instance type name.
    /// </summary>
    public virtual string ArrayInstance => "List";

    /// <summary>
    /// Array instance Add method name.
    /// </summary>
    public virtual string ArrayInstance_Add => "Add";

    /// <summary>
    /// Array instance Length property name.
    /// </summary>
    public virtual string ArrayInstance_Length => "Count";

    /// <summary>
    /// Trigger type name.
    /// </summary>
    public virtual string Trigger => "Suity.Controlling.Trigger";

    /// <summary>
    /// Trigger.AddEvent method name.
    /// </summary>
    public virtual string Trigger_AddEvent => "AddEvent";

    /// <summary>
    /// TriggerCollection type name.
    /// </summary>
    public virtual string TriggerCollection => "Suity.Controlling.TriggerCollection";

    /// <summary>
    /// TriggerCollection.AddTrigger method name.
    /// </summary>
    public virtual string TriggerCollection_AddTrigger => "AddTrigger";

    /// <summary>
    /// Controller type name.
    /// </summary>
    public virtual string Controller => "Suity.Controlling.Controller";

    /// <summary>
    /// Controller.GetController method name.
    /// </summary>
    public virtual string Controller_GetController => "GetController";

    /// <summary>
    /// Controller.GetTrigger method name.
    /// </summary>
    public virtual string Controller_GetTrigger => "GetTrigger";

    /// <summary>
    /// Controller.GetStateMachine method name.
    /// </summary>
    public virtual string Controller_GetStateMachine => "GetStateMachine";

    /// <summary>
    /// Controller.GetState method name.
    /// </summary>
    public virtual string Controller_GetState => "GetState";

    /// <summary>
    /// Component type name.
    /// </summary>
    public virtual string Component => "Suity.Server.Component";

    /// <summary>
    /// StateMachine type name.
    /// </summary>
    public virtual string StateMachine => "Suity.Controlling.StateMachine";

    /// <summary>
    /// StateMachine.AddState method name.
    /// </summary>
    public virtual string StateMachine_AddState => "AddState";

    /// <summary>
    /// StateMachine.ChangeState method name.
    /// </summary>
    public virtual string StateMachine_ChangeState => "ChangeState";

    /// <summary>
    /// StateMachine.ChangeStateDelayed method name.
    /// </summary>
    public virtual string StateMachine_ChangeStateDelayed => "ChangeStateDelayed";

    /// <summary>
    /// StateMachine.ChangeToNextState method name.
    /// </summary>
    public virtual string StateMachine_ChangeToNextState => "ChangeToNextState";

    /// <summary>
    /// StateMachine.ChangeToNextStateDelayed method name.
    /// </summary>
    public virtual string StateMachine_ChangeToNextStateDelayed => "ChangeToNextStateDelayed";

    /// <summary>
    /// StateMachine.ChangeToPreviousState method name.
    /// </summary>
    public virtual string StateMachine_ChangeToPreviousState => "ChangeToPreviousState";

    /// <summary>
    /// StateMachine.ChangeToPreviousStateDelayed method name.
    /// </summary>
    public virtual string StateMachine_ChangeToPreviousStateDelayed => "ChangeToPreviousStateDelayed";

    /// <summary>
    /// StateMachine.ResetState method name.
    /// </summary>
    public virtual string StateMachine_ResetState => "ResetState";

    /// <summary>
    /// StateMachine.GetStateByIndex method name.
    /// </summary>
    public virtual string StateMachine_GetStateByIndex => "GetStateByIndex";

    /// <summary>
    /// StateMachine.GetStateByName method name.
    /// </summary>
    public virtual string StateMachine_GetStateByName => "GetStateByName";

    /// <summary>
    /// StateMachine.CurrentState property name.
    /// </summary>
    public virtual string StateMachine_CurrentState => "CurrentState";

    /// <summary>
    /// StateMachine.AutoChangeToFirstState property name.
    /// </summary>
    public virtual string StateMachine_AutoChangeToFirstState => "AutoChangeToFirstState";

    /// <summary>
    /// State type name.
    /// </summary>
    public virtual string State => "Suity.Controlling.State";

    /// <summary>
    /// State.AddTrigger method name.
    /// </summary>
    public virtual string State_AddTrigger => "AddTrigger";

    /// <summary>
    /// State.AddController method name.
    /// </summary>
    public virtual string State_AddController => "AddController";

    /// <summary>
    /// State.AddStateMachine method name.
    /// </summary>
    public virtual string State_AddStateMachine => "AddStateMachine";

    /// <summary>
    /// State.NextState property name.
    /// </summary>
    public virtual string State_NextState => "NextState";

    /// <summary>
    /// State.Index property name.
    /// </summary>
    public virtual string State_Index => "Index";

    /// <summary>
    /// State.Name property name.
    /// </summary>
    public virtual string State_Name => "Name";

    /// <summary>
    /// State.IsEntered property name.
    /// </summary>
    public virtual string State_IsEntered => "IsEntered";

    /// <summary>
    /// State.StateMachine property name.
    /// </summary>
    public virtual string State_StateMachine => "StateMachine";

    /// <summary>
    /// State.AutoChangeToNextState property name.
    /// </summary>
    public virtual string State_AutoChangeToNextState => "AutoChangeToNextState";

    /// <summary>
    /// Entry.IsEnabled property name.
    /// </summary>
    public virtual string Entry_IsEnabled => "IsEnabled";

    /// <summary>
    /// Entry.Start method name.
    /// </summary>
    public virtual string Entry_Start => "Start";

    /// <summary>
    /// Entry.Stop method name.
    /// </summary>
    public virtual string Entry_Stop => "Stop";

    /// <summary>
    /// Entry.Enter method name.
    /// </summary>
    public virtual string Entry_Enter => "Enter";

    /// <summary>
    /// Entry.Exit method name.
    /// </summary>
    public virtual string Entry_Exit => "Exit";

    /// <summary>
    /// Entry.Update method name.
    /// </summary>
    public virtual string Entry_Update => "Update";

    /// <summary>
    /// Entry.DoAction method name.
    /// </summary>
    public virtual string Entry_DoAction => "DoAction";

    /// <summary>
    /// BaseComponent.Name property name.
    /// </summary>
    public virtual string BaseComponent_Name => "Name";

    /// <summary>
    /// BaseComponent.AddComponent method name.
    /// </summary>
    public virtual string BaseComponent_AddComponent => "AddComponent";

    /// <summary>
    /// NotImplementedException type name.
    /// </summary>
    public virtual string NotImplementedException => "System.NotImplementedException";
}