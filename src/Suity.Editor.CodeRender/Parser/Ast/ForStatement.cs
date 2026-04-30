namespace Suity.Parser.Ast;

public class ForStatement : StatementNode
{
    // can be a Statement (var i) or an Expression (i=0)
    public SyntaxNode Init;
    public Expression Test;
    public Expression Update;
    public StatementNode Body;
}