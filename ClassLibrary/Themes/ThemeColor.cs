using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ClassLibrary.Themes
{
    public class ThemeColor : INotifyPropertyChanged
    {
        public ThemeColor()
        {

        }

        public Color Color { get; set; }

        public bool? IsSelected
        {
            get
            {
                return ApplicationSettings.CustomThemeColor == this.Color;
            }
            set
            {
                if (value == true)
                    ApplicationSettings.CustomThemeColor = this.Color;

                OnPropertyChanged(this, "IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(object sender, string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(Property));
            }
        }
    }
}
