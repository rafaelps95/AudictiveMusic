﻿using ClassLibrary.Helpers;
using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudictiveMusicUWP.Gui.Pages
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class About : Page
    {
        private NavigationMode NavMode
        {
            get;
            set;
        }

        public About()
        {
            this.Loaded += About_Loaded;
            this.SizeChanged += About_SizeChanged;
            this.InitializeComponent();
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (this.RenderSize.Width >= 500)
                //    contentGrid.Width = double.NaN;
                //else
                    contentGrid.Width = this.RenderSize.Width;
            }
            catch
            {

            }
        }

        private void About_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                contentGrid.Width = e.NewSize.Width;
            }
            catch
            {

            }

            layoutRoot.Margin = new Thickness(0, 0, 0, ApplicationInfo.Current.FooterHeight);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //appColor.Color = ApplicationSettings.CurrentThemeColor;

            try
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;
                appName.Text = package.DisplayName;
                appVersion.Text = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

                NavMode = e.NavigationMode;

                base.OnNavigatedTo(e);
                //await Task.Delay(400);
            }
            catch
            {

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.SizeChanged -= About_SizeChanged;

            base.OnNavigatedFrom(e);
        }

        private async void rateButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(
    new Uri($"ms-windows-store://review/?PFN={Package.Current.Id.FamilyName}"));
        }
    }
}
