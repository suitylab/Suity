namespace Suity.Parser.Ast;

public class ForInStatement : StatementNode
{
    public SyntaxNode Left;
    public Expression Right;
    public StatementNode Body;
    public bool Each;
}