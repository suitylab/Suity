using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Suity.Properties
{
    internal static class IconCache
    {
        public static readonly Image FalseHidden = Resources.FalseHidden.ToBitmap();
        public static readonly Image TrueHidden = Resources.TrueHidden.ToBitmap();

        public static readonly Image cross = Resources.cross.ToBitmap();
        public static readonly Image inspector = Resources.inspector.ToBitmap();
    }
}
