using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using ClassLibrary.Helpers.Enumerators;
using ClassLibrary.Helpers;

namespace ClassLibrary.Helpers
{
    public static class ThemeSettings
    {
        public delegate void ThemeChangedEventHandler();
        public static event ThemeChangedEventHandler BlurLevelChanged;
        public static event ThemeChangedEventHandler NowPlayingThemeChanged;
        public static event ThemeChangedEventHandler CurrentThemeColorChanged;
        public static event ThemeChangedEventHandler ThemeBackgroundPreferenceChanged;
        public static event ThemeChangedEventHandler ApplicationThemeChanged;
        public static event ThemeChangedEventHandler TransparencyEffectToggled;
        public static event ThemeChangedEventHandler PerformanceModeToggled;



        /// <summary>
        /// Retorna 0 para capa do álbum e 1 para imagem do artista
        /// </summary>
        public static ThemeBackgroundSource ThemeBackgroundPreference
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("ThemeBackgroundPreference");
                if (value == null)
                    return ThemeBackgroundSource.ArtistImage;
                else
                    return (ThemeBackgroundSource)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("ThemeBackgroundPreference", (int)value);

                ThemeBackgroundPreferenceChanged?.Invoke();
            }
        }


        /// <summary>
        /// Retorna 0 se usa a cor do álbum, 1 de usa a cor do sistema e 2 se usa cor personalizada
        /// </summary>
        public static ThemeColorSource ThemeColorPreference
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("ThemeColorPreference");
                if (value == null)
                    return ThemeColorSource.NoColor;
                else
                    return (ThemeColorSource)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("ThemeColorPreference", (int)value);

                NowPlayingThemeChanged?.Invoke();
            }
        }

        public static Color CurrentThemeColor
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("CurrentThemeColor");
                if (value == null)
                    return ApplicationInfo.Current.CurrentAppThemeColor(AppTheme == PageTheme.Dark);
                else
                    return ImageHelper.GetColorFromHex(value.ToString());
            }
            set
            {
                if (value.IsDarkColor())
                    CurrentForegroundColor = Colors.White;
                else
                    CurrentForegroundColor = Colors.Black;


                ApplicationSettings.SaveSettingsValue("CurrentThemeColor", ImageHelper.GetHexFromColor(value));

                CurrentThemeColorChanged?.Invoke();
            }
        }

        public static Color CurrentForegroundColor
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("CurrentForegroundColor");
                if (value == null)
                {
                    if (CurrentThemeColor.IsDarkColor())
                        return Colors.White;
                    else
                        return Colors.Black;
                }
                else
                    return ImageHelper.GetColorFromHex(value.ToString());
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("CurrentForegroundColor", ImageHelper.GetHexFromColor(value));
            }
        }

        public static Color CustomThemeColor
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("CustomThemeColor");
                if (value == null)
                    return ImageHelper.GetColorFromHex("#FFDC572E");
                else
                    return ImageHelper.GetColorFromHex(value.ToString());
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("CustomThemeColor", ImageHelper.GetHexFromColor(value));

                CurrentThemeColor = value;
            }
        }

        public static PageTheme AppTheme
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("AppTheme");
                if (value == null)
                    return PageTheme.Dark;
                else
                    return (PageTheme)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("AppTheme", (int)value);
                ApplicationThemeChanged?.Invoke();
            }
        }

        public static bool IsTransparencyEnabled
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("TransparencyEnabled");
                if (value == null)
                    return true;
                else
                    return (bool)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("TransparencyEnabled", value);
                TransparencyEffectToggled?.Invoke();
            }
        }

        public static bool IsPerformanceModeEnabled
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("PerformanceModeEnabled");
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("PerformanceModeEnabled", value);
                PerformanceModeToggled?.Invoke();
            }
        }

        public static bool IsBackgroundAcrylicEnabled
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("BackgroundAcrylic");
                if (value == null)
                    return false;
                else
                    return (bool)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("BackgroundAcrylic", value);
                TransparencyEffectToggled?.Invoke();
            }
        }

        public static Theme NowPlayingTheme
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("NowPlayingTheme");
                if (value == null)
                    return Theme.Modern;
                else
                {
                    Theme t = (Theme)Enum.Parse(typeof(Theme), value.ToString());
                    return t;
                }
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("NowPlayingTheme", value.ToString());

                NowPlayingThemeChanged?.Invoke();
            }
        }

        public static float NowPlayingBlurAmount
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("NowPlayingBlurAmount");
                if (value == null)
                    return 10;
                else
                    return (float)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("NowPlayingBlurAmount", value);

                BlurLevelChanged?.Invoke();
            }
        }

        public static float NowPlayingGrayscale
        {
            get
            {
                object value = ApplicationSettings.ReadSettingsValue("NowPlayingGrayscale");
                if (value == null)
                    return 10;
                else
                    return (float)value;
            }
            set
            {
                ApplicationSettings.SaveSettingsValue("NowPlayingGrayscale", value);
            }
        }
    }
}
