using System;

namespace Suity.Synchonizing.Core;

[Flags]
public enum VisitFlag
{
    None = 0x0,
    IgnoreElement = 0x1,
}