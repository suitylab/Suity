using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class Program : StatementNode, IVariableScope, IFunctionScope
{
    public Program()
    {
        VariableDeclarations = new List<VariableDeclaration>();
    }
    public ICollection<StatementNode> Body;

    public List<Comment> Comments;
    public List<Token> Tokens;
    public List<ParserException> Errors;
    public bool Strict;

    public IList<VariableDeclaration> VariableDeclarations { get; set; }
    public IList<FunctionDeclaration> FunctionDeclarations { get; set; }
}