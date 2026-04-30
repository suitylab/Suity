using Suity.Editor.CodeRender;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// Context for expression generation containing naming, body stacks, and rendering options.
/// </summary>
public class ExpressionContext
{
    private readonly SystemNamingOption _naming;

    private Stack<SystemNamingOption> _namings;
    private readonly Stack<ExprListItem> _classBodyStack = new();
    private readonly Stack<ExprListItem> _statementBodyStack = new();
    private int _localVarAllocate = 0;

    /// <summary>
    /// Creates a new expression context with the specified naming option.
    /// </summary>
    /// <param name="naming">The naming option to use.</param>
    public ExpressionContext(SystemNamingOption naming)
    {
        _naming = naming;
    }

    /// <summary>
    /// Gets or sets the indent spacing in spaces.
    /// </summary>
    public int IndentSpacing { get; set; } = 4;

    #region Naming

    /// <summary>
    /// Gets the current naming option, accounting for pushed overrides.
    /// </summary>
    public SystemNamingOption Naming
    {
        get
        {
            if (_namings?.Count > 0)
            {
                return _namings.Peek();
            }

            return _naming;
        }
    }

    /// <summary>
    /// Pushes a naming option onto the stack.
    /// </summary>
    /// <param name="naming">The naming option to push.</param>
    public void PushNaming(SystemNamingOption naming)
    {
        if (naming is null)
        {
            throw new ArgumentNullException(nameof(naming));
        }

        _namings ??= new();

        _namings.Push(naming);
    }

    /// <summary>
    /// Pops the most recent naming option from the stack.
    /// </summary>
    /// <returns>The popped naming option.</returns>
    public SystemNamingOption PopNaming()
    {
        return _namings.Pop();
    }

    #endregion

    #region Basic Parameters

    /// <summary>
    /// Material ID
    /// </summary>
    public Guid MaterialId { get; set; }

    /// <summary>
    /// Render type ID
    /// </summary>
    public Guid RenderTypeId { get; set; }

    /// <summary>
    /// Current renderable asset key
    /// </summary>
    public Guid RenderableId { get; set; }

    /// <summary>
    /// Namespace of the file
    /// </summary>
    public string NameSpace { get; set; }

    /// <summary>
    /// Class name
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// Use full name
    /// </summary>
    public bool UseFullName { get; set; }

    /// <summary>
    /// Try to use short name
    /// </summary>
    public bool TryUseShortName { get; set; }

    /// <summary>
    /// Use native array type
    /// </summary>
    public bool UseNativeArray { get; set; }

    /// <summary>
    /// Render entire body
    /// </summary>
    public bool WithBody { get; set; }

    /// <summary>
    /// Is static
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether the expression is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether user code is enabled.
    /// </summary>
    public bool UserCodeEnabled { get; set; } = true;

    #endregion

    #region Stack

    /// <summary>
    /// Gets the current class body list, or null if not in a class body context.
    /// </summary>
    public IList<ExpressionNode> ClassBody => _classBodyStack.Count > 0 ? _classBodyStack.Peek().List : null;

    /// <summary>
    /// Gets the current statement body list, or null if not in a statement body context.
    /// </summary>
    public IList<ExpressionNode> StatementBody => _statementBodyStack.Count > 0 ? _statementBodyStack.Peek().List : null;

    /// <summary>
    /// Pushes a new class body context.
    /// </summary>
    /// <param name="owner">The owner of the class body.</param>
    public void PushClassBody(object owner)
    {
        if (owner == null) throw new ArgumentNullException();
        _classBodyStack.Push(new ExprListItem(owner));
    }

    /// <summary>
    /// Pushes a new statement body context.
    /// </summary>
    /// <param name="owner">The owner of the statement body.</param>
    public void PushStatementBody(object owner)
    {
        if (owner == null) throw new ArgumentNullException();
        _statementBodyStack.Push(new ExprListItem(owner));
    }

    /// <summary>
    /// Pops the class body context and returns the block expression.
    /// </summary>
    /// <param name="owner">The owner that should match the top of the stack.</param>
    /// <returns>The block expression containing the class body.</returns>
    public ExpressionNode PopClassBody(object owner)
    {
        if (owner == null) throw new ArgumentNullException();
        if (_classBodyStack.Peek().Owner == owner)
        {
            _localVarAllocate = 0;
            PreClassBody = null;
            PostClassBody = null;
            PreConstructor = null;
            PostConstructor = null;
            PreEntryCall = null;
            PostEntryCall = null;
            return ExpressionNode.Builder.Block(_classBodyStack.Pop().List);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Pops the statement body context and returns the block expression.
    /// </summary>
    /// <param name="owner">The owner that should match the top of the stack.</param>
    /// <returns>The block expression containing the statement body.</returns>
    public ExpressionNode PopStatementBody(object owner)
    {
        if (owner == null) throw new ArgumentNullException();
        if (_statementBodyStack.Peek().Owner == owner)
        {
            return ExpressionNode.Builder.Block(_statementBodyStack.Pop().List);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    #endregion

    #region Callbacks

    /// <summary>
    /// Callback invoked before rendering the class body.
    /// </summary>
    public Action PreClassBody;

    /// <summary>
    /// Callback invoked at the main entry point of the class body.
    /// </summary>
    public Action ClassBodyMainEntry;

    /// <summary>
    /// Callback invoked after rendering the class body.
    /// </summary>
    public Action PostClassBody;

    /// <summary>
    /// Callback invoked before rendering the constructor.
    /// </summary>
    public Action PreConstructor;

    /// <summary>
    /// Callback invoked after rendering the constructor.
    /// </summary>
    public Action PostConstructor;

    /// <summary>
    /// Callback invoked before an entry call.
    /// </summary>
    public Action<string> PreEntryCall;

    /// <summary>
    /// Callback invoked after an entry call.
    /// </summary>
    public Action<string> PostEntryCall;

    #endregion

    #region Condition

    /// <summary>
    /// Gets or sets the condition for conditional rendering.
    /// </summary>
    public ICondition Condition { get; set; }

    #endregion

    /// <summary>
    /// Creates a clone of this context.
    /// </summary>
    /// <returns>A new ExpressionContext with copied properties.</returns>
    public ExpressionContext Clone()
    {
        return new ExpressionContext(_naming)
        {
            MaterialId = MaterialId,
            RenderTypeId = RenderTypeId,
            RenderableId = RenderableId,
            NameSpace = NameSpace,
            ClassName = ClassName,
            UseFullName = UseFullName,
            TryUseShortName = TryUseShortName,
            UseNativeArray = UseNativeArray,
            WithBody = WithBody,
            IsStatic = IsStatic,
            Disabled = Disabled,
            UserCodeEnabled = UserCodeEnabled,
            PreClassBody = PreClassBody,
            ClassBodyMainEntry = ClassBodyMainEntry,
            PostClassBody = PostClassBody,
            PreConstructor = PreConstructor,
            PostConstructor = PostConstructor,
            PreEntryCall = PreEntryCall,
            PostEntryCall = PostEntryCall,
            Condition = Condition,
        };
    }

    /// <summary>
    /// Allocates a unique local variable name.
    /// </summary>
    /// <returns>A unique local variable name.</returns>
    public string AllocateLocalVariable()
    {
        _localVarAllocate++;
        return "__var_" + _localVarAllocate;
    }

    private class ExprListItem
    {
        public readonly object Owner;
        public readonly List<ExpressionNode> List = [];

        public ExprListItem(object owner)
        {
            Owner = owner;
        }
    }
}