namespace Suity.Editor.Services;

class CliLicenseService : LicenseService
{
    public static CliLicenseService Instance { get; } = new();

    private CliLicenseService()
    { }

    public override string ProductName => SuityCliApp.ProductName;

    public override string ProductVersion => SuityCliApp.VersionCode;

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
