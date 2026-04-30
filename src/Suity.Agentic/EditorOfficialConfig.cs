using Suity.Collections;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor;

public class EditorOfficialConfig
{
    public static readonly string[] OfficialUrls =
    [
        "https://storage.suitylab.com/SuityEditor5",
        "https://pub-aadb6559fa5649d4b3e9439b79c5f4cd.r2.dev/SuityEditor5",
        "https://storage.suity.tech/SuityEditor5",
    ];

    public const string Url_MainConfig = "EditorConfig.json";
    public const string Url_TemplateConfig = "ProjectTemplates.json";

    private Dictionary<Type, object> _startupConfigs = [];

    /// <summary>
    /// Directory where templates are located
    /// </summary>
    public string ProjectTemplateDirectory { get; private set; }

    public string ProjectTemplateImageDirectory { get; private set; }
    public string LandingImageDirectory { get; private set; }
    public string ExtensionsDirectory { get; private set; }


    public string StorageBaseUrl { get; private set; }
    public string EditorDownloadBaseUrl { get; private set; }
    public string ProjectTemplateBaseUrl { get; private set; }
    public string ProjectTemplateImageBaseUrl { get; private set; }
    public string LandingImageBaseUrl { get; private set; }
    public string ExtensionsBaseUrl { get; private set; }

    public string ServiceUrl { get; private set; }

    public EditorOfficialConfig()
    {
        ProjectTemplateDirectory = AppDomain.CurrentDomain.BaseDirectory.PathAppend(@"Templates");
        ProjectTemplateImageDirectory = AppDomain.CurrentDomain.BaseDirectory.PathAppend(@"Templates\Images");
        LandingImageDirectory = AppDomain.CurrentDomain.BaseDirectory.PathAppend(@"Landings");
        ExtensionsDirectory = AppDomain.CurrentDomain.BaseDirectory.PathAppend(@"Extensions");
    }

    public T GetConfig<T>()
        where T : class
    {
        return _startupConfigs.GetValueSafe(typeof(T)) as T;
    }

    public bool HasConfig => false;
}