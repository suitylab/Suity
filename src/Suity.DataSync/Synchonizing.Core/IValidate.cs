namespace Suity.Synchonizing.Core;

/// <summary>
/// Interface for objects that support validation
/// </summary>
public interface IValidate
{
    void Find(ValidationContext context, string findStr, SearchOptions findOption);

    void Validate(ValidationContext context);
}