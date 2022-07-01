using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static AudictiveMusicUWP.Gui.UC.CircleImage;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class WebServiceButton : UserControl
    {
        public event RoutedEventHandler Click;

        public enum WebService
        {
            LocalFiles,
            LoggedUser,
            LastFm,
            Spotify
        }

        public enum Context
        {
            UserProfile,
            WebService
        }

        private WebService service = WebService.LocalFiles;
        public WebService Service
        {
            get { return service; }
            set 
            { service = value;
                SetService(value);
            }
        }

        private void SetService(WebService service)
        {
            BitmapImage bmp = new BitmapImage();
            serviceLogo.Source = bmp;
            if (service == WebService.LastFm)
            {
                bmp.UriSource = new Uri("ms-appx:///Assets/lastfm_circlelogo.png");
                //background.Opacity = 1;
            }
            else if (service == WebService.Spotify)
            {
                bmp.UriSource = new Uri("ms-appx:///Assets/spotify_circlelogo.png");
                //background.Opacity = 1;
            }
            else if (service == WebService.LocalFiles)
            {
                bmp.UriSource = new Uri("ms-appx:///Assets/localmediabutton.png");
                //background.Opacity = 1;
            }
            else
            {
                //background.Opacity = 0.5;
            }
        }

        private string text = "";
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                SetText(value);
            }
        }

        private void SetText(string value)
        {
            button.Content = value;
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(WebServiceButton), null);

        public Uri ImageUri
        {
            get { return (Uri)GetValue(ImageUriProperty); }
            set
            {
                SetValue(ImageUriProperty, value);
            }
        }

        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(Uri), typeof(WebServiceButton), null);

        public WebServiceButton()
        {
            this.Loaded += WebServiceButton_Loaded;
            this.InitializeComponent();

            SetService(Service);
        }

        private void WebServiceButton_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SetService(this.Service);
        }

        public void SetImageSource(Uri uri)
        {
            image.SetSource(uri, ImageType.LastFmUser);
        }

        public void RemoveImage()
        {
            image.RemoveSource();
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
