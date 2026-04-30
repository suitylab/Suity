using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Suity.Editor.ProjectGui.Properties
{
    internal static class IconCache
    {
        public static readonly Image Delete = Resources.Delete.ToBitmap();
        public static readonly Image Rendering = Resources.Rendering.ToBitmap();
        public static readonly Image Project = Resources.project.ToBitmap();
        public static readonly Image RenderingBunch = Resources.RenderingVolume.ToBitmap();

    }
}
