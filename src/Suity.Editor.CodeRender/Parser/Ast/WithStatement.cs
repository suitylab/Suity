namespace Suity.Parser.Ast;

public class WithStatement : StatementNode
{
    public Expression Object;
    public StatementNode Body;
}