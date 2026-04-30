using System;

namespace Suity.Parser;

public class ParserException : Exception
{
    public int Column;
    public string Description;
    public int Index;
    public int LineNumber;
    public string SourceCode;

    public ParserException(string message) : base(message)
    {
    }
}