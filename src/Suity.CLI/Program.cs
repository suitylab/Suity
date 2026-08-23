using Suity.Editor;
using Suity.Editor.Services;


EditorServices.SystemLog = ConsleSystemLog.Instance;
return CliCommandRouter.Instance.Route(args);
