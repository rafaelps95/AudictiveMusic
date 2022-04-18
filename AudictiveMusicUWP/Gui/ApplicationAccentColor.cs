using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace AudictiveMusicUWP.Gui
{
    public class ApplicationAccentColor : INotifyPropertyChanged
    {
        private SolidColorBrush accentColor = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColor
        {
            get { return accentColor; }
            set { accentColor = AccentColorLowBrush = AccentColorMediumBrush = value; OnPropertyChanged(); }
        }

        private SolidColorBrush accentColorLowBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorLowBrush
        {
            get { accentColorLowBrush.Opacity = 0.6; return accentColorLowBrush; }
            set { accentColorLowBrush = value; OnPropertyChanged(); }
        }

        private SolidColorBrush accentColorMediumBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorMediumBrush
        {
            get { accentColorMediumBrush.Opacity = 0.8; return accentColorMediumBrush; }
            set { accentColorMediumBrush = value; OnPropertyChanged(); }
        }

        private SolidColorBrush foregroundColor = new SolidColorBrush(Colors.White);

        public SolidColorBrush ForegroundColor
        {
            get { return foregroundColor; }
            set { foregroundColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
