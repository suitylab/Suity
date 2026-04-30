namespace Suity.Parser.Ast;

public class CatchClause : StatementNode
{
    public Identifier Param;
    public BlockStatement Body;
}