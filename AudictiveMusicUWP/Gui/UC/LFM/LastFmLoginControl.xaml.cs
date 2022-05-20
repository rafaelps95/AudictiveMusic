using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using RPSToolkit;
using System;
using System.Numerics;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class LastFmLoginControl : UserControl
    {
        public LastFmLoginControl()
        {
            this.Loaded += LastFmLoginControl_Loaded;
            this.SizeChanged += LastFmLoginControl_SizeChanged;
            this.InitializeComponent();
        }

        private void LastFmLoginControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void LastFmLoginControl_Loaded(object sender, RoutedEventArgs e)
        {
            border.ApplyShadow();
        }


        private void blur_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            NavigationHelper.Back(this);
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private async void Login()
        {
            bool result = await LastFm.Current.Login(username.Text, password.Password);

            if (result)
            {
                NavigationHelper.Back(this);
            }
            else
            {
                loginError.Visibility = Visibility.Visible;
            }
        }

        private async void signUpButon_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationInfo.Current.Language == "EN")
                await Launcher.LaunchUriAsync(new Uri("https://www.last.fm/join", UriKind.Absolute));
            else
                await Launcher.LaunchUriAsync(new Uri("https://www.last.fm/" + ApplicationInfo.Current.Language.ToLower() + "/join", UriKind.Absolute));
        }

        private void textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                Login();
        }
    }
}
