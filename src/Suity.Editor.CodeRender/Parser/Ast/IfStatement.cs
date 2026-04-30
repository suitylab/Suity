namespace Suity.Parser.Ast;

public class IfStatement : StatementNode
{
    public Expression Test;
    public StatementNode Consequent;
    public StatementNode Alternate;
}