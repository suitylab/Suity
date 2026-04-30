using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class ObjectExpression : Expression
{
    public IEnumerable<Property> Properties;
}