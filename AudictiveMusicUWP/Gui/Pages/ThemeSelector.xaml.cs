using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Helpers;
using ClassLibrary.Helpers.Enumerators;
using ClassLibrary.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ThemeSelector : Page
    {
        private string[] colors = new string[] { "#FFFFB900", "#FFFF8C00", "#FFF7630C", "#FFCA5010", "#FFDA3B01", "#FFEF6950", "#FFD13438", "#FFFF4343", "#FFE74856", "#FFE81123", "#FFEA005E", "#FFC30052", "#FFE3008C", "#FFBF0077", "#FFC239B3", "#FF9A0089", "#FF0078D7", "#FF0063B1", "#FF8E8CD8", "#FF6B69D6", "#FF8764B8", "#FF744DA9", "#FFB146C2", "#FF881798", "#FF0099BC", "#FF2D7D9A", "#FF00B7C3", "#FF038387", "#FF00B294", "#FF018574", "#FF00CC6A", "#FF10893E", "#FF7A7574", "#FF5D5A58", "#FF68768A", "#FF515C6B", "#FF567C73", "#FF486860", "#FF498205", "#FF107C10", "#FF767676", "#FF4C4A48", "#FF69797E", "#FF4A5459", "#FF647C64", "#FF525E54", "#FF847545", "#FF7E735F" };
        private ObservableCollection<ThemeColor> ColorsList = new ObservableCollection<ThemeColor>();

        private NavigationMode NavMode
        {
            get;
            set;
        }


        public ThemeSelector()
        {
            this.SizeChanged += ThemeSelector_SizeChanged;
            this.InitializeComponent();
        }

        private void ThemeSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            themesList.Width = e.NewSize.Width;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            pageTitle.Text = ApplicationInfo.Current.Resources.GetString("Themes");
            this.NavMode = e.NavigationMode;

            LoadThemes();
            LoadThemeSettings(ThemeSettings.NowPlayingTheme);
        }

        private void LoadThemes()
        {
            List<ThemeBase> themes = new List<ThemeBase>()
            {
                new ModernTheme(), new BlurTheme() /*, new NeonTheme(), new MaterialTheme()*/
            };

            themesList.ItemsSource = themes;
        }

        private void LoadThemeSettings(Theme theme)
        {
            switch (theme)
            {
                case Theme.Blur:

                    blurOptions.Visibility = Visibility.Visible;

                    cleanOptions.Visibility = Visibility.Collapsed;
                    materialOptions.Visibility = Visibility.Collapsed;
                    modernOptions.Visibility = Visibility.Collapsed;
                    neonOptions.Visibility = Visibility.Collapsed;


                    ThemeBlurAmount.Value = Convert.ToDouble(ThemeSettings.NowPlayingBlurAmount);

                    ThemeBlurAmount.ValueChanged -= ThemeBlurAmount_ValueChanged;
                    ThemeBlurAmount.ValueChanged += ThemeBlurAmount_ValueChanged;

                    break;

                case Theme.Clean:

                    cleanOptions.Visibility = Visibility.Visible;

                    blurOptions.Visibility = Visibility.Collapsed;
                    materialOptions.Visibility = Visibility.Collapsed;
                    modernOptions.Visibility = Visibility.Collapsed;
                    neonOptions.Visibility = Visibility.Collapsed;

                    break;

                case Theme.Material:

                    materialOptions.Visibility = Visibility.Visible;

                    blurOptions.Visibility = Visibility.Collapsed;
                    cleanOptions.Visibility = Visibility.Collapsed;
                    modernOptions.Visibility = Visibility.Collapsed;
                    neonOptions.Visibility = Visibility.Collapsed;

                    break;

                case Theme.Modern:

                    modernOptions.Visibility = Visibility.Visible;

                    blurOptions.Visibility = Visibility.Collapsed;
                    cleanOptions.Visibility = Visibility.Collapsed;
                    materialOptions.Visibility = Visibility.Collapsed;
                    neonOptions.Visibility = Visibility.Collapsed;

                    break;

                case Theme.Neon:

                    neonOptions.Visibility = Visibility.Visible;

                    blurOptions.Visibility = Visibility.Collapsed;
                    cleanOptions.Visibility = Visibility.Collapsed;
                    materialOptions.Visibility = Visibility.Collapsed;
                    modernOptions.Visibility = Visibility.Collapsed;

                    break;

            }


        }

        private void ThemeBlurAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ThemeSettings.NowPlayingBlurAmount = (float)e.NewValue;
        }

        private void themesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var theme = e.ClickedItem as ThemeBase;
            List<ThemeBase> list = new List<ThemeBase>();

            foreach (ThemeBase t in themesList.Items)
            {
                if (t.Name == theme.Name)
                    t.IsSelected = true;
                else
                    t.IsSelected = false;

                list.Add(t);
            }

            themesList.ItemsSource = list;

            LoadThemeSettings(theme.Theme);
        }
    }
}