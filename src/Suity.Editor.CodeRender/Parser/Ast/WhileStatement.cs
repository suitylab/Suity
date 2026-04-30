namespace Suity.Parser.Ast;

public class WhileStatement : StatementNode
{
    public Expression Test;
    public StatementNode Body;
}