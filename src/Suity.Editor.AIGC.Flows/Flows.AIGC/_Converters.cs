using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Services;

namespace Suity.Editor.Flows.AIGC;

#region Converters

/// <summary>
/// Type converter that converts XML tags to their string representation.
/// </summary>
public class XmlTagToTextConverter : TypeToTextConverter<LooseXmlTag>
{
    /// <inheritdoc/>
    public override string Convert(LooseXmlTag objFrom)
    {
        return objFrom.ToString();
    }
}

#endregion