using Suity.Editor.ProjectGui;
using Suity.Editor.WorkSpaces;
using System;

namespace Suity.Editor;

public class NavigateVReq
{
    public object Target { get; set; }
    public bool Successful { get; set; }
}

public class LocateInProjectVReq
{
    public string FileName { get; set; }
    public object Item { get;set; }
    public bool Successful { get; set; }
}

public class LocateProjectNodeVReq
{
    public IProjectViewNode ViewNode { get; set; }
    public bool Successful { get; set; }
}

public class LocateInPublishVReq
{
    public string FileName { get; set; }
    public bool Successful { get; set; }
}

public class LocateWorkSpaceVReq
{
    public WorkSpace WorkSpace { get; set; }
    public string RelativeFileName { get; set; }
    public bool Successful { get; set; }
}

public class LocateInCanvasVReq
{
    public Guid Id { get; set; }

    public bool Successful { get; set; }
}

public class ShowNotifyVReq
{
    public string Title { get; set; }
    public string ButtonText { get; set; }
    public Action Action { get; set; }
}