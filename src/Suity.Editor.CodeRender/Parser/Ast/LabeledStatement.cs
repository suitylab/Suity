namespace Suity.Parser.Ast;

public class LabelledStatement : StatementNode
{
    public Identifier Label;
    public StatementNode Body;
}