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
            set
            {
                accentColor = value;
                AccentColorLowBrush = new SolidColorBrush(value.Color) { Opacity = 0.6 };
                AccentColorMediumBrush = new SolidColorBrush(value.Color) { Opacity = 0.8 };
                AccentColorDark10PercentBrush = new SolidColorBrush(value.Color.ChangeColorBrightness(-0.1f));
                AccentColorDark30PercentBrush = new SolidColorBrush(value.Color.ChangeColorBrightness(-0.3f));
                AccentColorDark50PercentBrush = new SolidColorBrush(value.Color.ChangeColorBrightness(-0.5f));
                OnPropertyChanged();
            }
        }

        private SolidColorBrush accentColorLowBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorLowBrush
        {
            get { return accentColorLowBrush; }
            private set { accentColorLowBrush = value; OnPropertyChanged(); }
        }

        private SolidColorBrush accentColorMediumBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorMediumBrush
        {
            get { return accentColorMediumBrush; }
            private set { accentColorMediumBrush = value; OnPropertyChanged(); }
        }

        #region Dark Accent Color

        private SolidColorBrush accentColorDark10PercentBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorDark10PercentBrush
        {
            get{ return accentColorDark10PercentBrush; }
            private set { accentColorDark10PercentBrush = value; OnPropertyChanged(); }
        }


        private SolidColorBrush accentColorDark30PercentBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorDark30PercentBrush
        {
            get { return accentColorDark30PercentBrush; }
            private set { accentColorDark30PercentBrush = value; OnPropertyChanged(); }
        }


        private SolidColorBrush accentColorDark50PercentBrush = new SolidColorBrush(ApplicationSettings.CurrentThemeColor);

        public SolidColorBrush AccentColorDark50PercentBrush
        {
            get { return accentColorDark50PercentBrush; }
            private set { accentColorDark50PercentBrush = value; OnPropertyChanged(); }
        }

        #endregion


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
