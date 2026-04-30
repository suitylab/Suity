namespace Suity.Editor.Expressions;

/// <summary>
/// Provides access to various expression service stores used in the editor code rendering system.
/// </summary>
public static class EditorExpressions
{
    private static readonly ServiceStore<IEditorObjectExpressions> _editorObjects = new();
    private static readonly ServiceStore<IInitialValueExpressions> _initials = new();
    private static readonly ServiceStore<IDTypeExpressions> _dtypes = new();
    private static readonly ServiceStore<IFormatterExpressions> _formatters = new();
    private static readonly ServiceStore<IDataExpressions> _datas = new();
    private static readonly ServiceStore<IFuntionExpressions> _funtions = new();
    private static readonly ServiceStore<IControllerExpressions> _triggers = new();
    private static readonly ServiceStore<IComponentExpressions> _components = new();

    /// <summary>
    /// Gets the editor object expressions service.
    /// </summary>
    public static IEditorObjectExpressions EditorObjects => _editorObjects.Get();

    /// <summary>
    /// Gets the initial value expressions service.
    /// </summary>
    public static IInitialValueExpressions InitialValues => _initials.Get();

    /// <summary>
    /// Gets the DType expressions service.
    /// </summary>
    public static IDTypeExpressions DTypes => _dtypes.Get();

    /// <summary>
    /// Gets the formatter expressions service.
    /// </summary>
    public static IFormatterExpressions Formatters => _formatters.Get();

    /// <summary>
    /// Gets the data expressions service.
    /// </summary>
    public static IDataExpressions Datas => _datas.Get();

    /// <summary>
    /// Gets the function expressions service.
    /// </summary>
    public static IFuntionExpressions Functions => _funtions.Get();

    /// <summary>
    /// Gets the controller (trigger) expressions service.
    /// </summary>
    public static IControllerExpressions Triggers => _triggers.Get();

    /// <summary>
    /// Gets the component expressions service.
    /// </summary>
    public static IComponentExpressions Components => _components.Get();
}