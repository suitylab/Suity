namespace Suity.Editor.Services;

class AvaLicenseService : LicenseService
{
    public static readonly AvaLicenseService Instance = new();

    private AvaLicenseService()
    { }

    public override string ProductName => SuityApp.ProjectName;

    public override string ProductVersion => SuityApp.VersionCode;

    public override string UserId => string.Empty;

    public override LicenseTypes LicenseType => LicenseTypes.Community;

    public override bool CheckLogin() => true;

    public override bool GetFeature(EditorFeatures capability) => true;

    public override bool GetFeatureEx(string name) => true;

    public override bool GetMaxUsageReach() => true;

    public override string GetFailedMessage(EditorFeatures capability) => string.Empty;

    public override string GetUsageFailedMessage() => string.Empty;

    public override int LimitedEntryCount => int.MaxValue;

    public override int MaxDiagramCount => int.MaxValue;

    public override int MaxNodeCount => int.MaxValue;

    public override int EditorPoint => int.MaxValue;

    public override int AigcPoint => int.MaxValue;
}
