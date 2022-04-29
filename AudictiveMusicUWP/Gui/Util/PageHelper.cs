using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudictiveMusicUWP.Gui.Pages;
using Windows.Foundation;

namespace AudictiveMusicUWP.Gui.Util
{
    public class PageHelper
    {
        public delegate void SearchBarLayoutHandler(bool isCompact);
        public static event SearchBarLayoutHandler LayoutChangeRequested;

        public delegate void SearchBarOffsetHandler(double offset);
        public static event SearchBarOffsetHandler OffsetChangeRequested;

        public static void SetSearchBarCompactMode(bool isCompact) => LayoutChangeRequested?.Invoke(isCompact);

        public static void SetSearchBarOffset(double offset) => OffsetChangeRequested?.Invoke(offset);

        public static Size SearchBoxSize
        {
            get;
            set;
        }
    }
}
