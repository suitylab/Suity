using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class FunctionDeclaration : StatementNode, IFunctionDeclaration
{
    public FunctionDeclaration()
    {
        VariableDeclarations = new List<VariableDeclaration>();
    }

    public Identifier Id { get; set; }
    public IEnumerable<Identifier> Parameters { get; set; }
    public StatementNode Body { get; set; }
    public bool Strict { get; set; }

    public IList<VariableDeclaration> VariableDeclarations { get; set; }

    #region ECMA6
    
    public IEnumerable<Expression> Defaults;
    public SyntaxNode Rest;
    public bool Generator;
    public bool Expression;
    
    #endregion

    public IList<FunctionDeclaration> FunctionDeclarations { get; set; }
}