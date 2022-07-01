using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using RPSToolkit;
using ClassLibrary.Helpers.Enumerators;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class Card : UserControl
    {
        private MediaItemType ContextMode;
        private MediaItem _mediaItem;

        public Card()
        {
            this.Loaded += Card_Loaded;
            this.SizeChanged += Card_SizeChanged;
            this.InitializeComponent();
        }

        private void Card_Loaded(object sender, RoutedEventArgs e)
        {
            shadow.ApplyShadow();
        }

        public void SetContext(MediaItem context)
        {
            this.DataContext = context;
            _mediaItem = context;

            if (context.GetType() == typeof(Song))
            {
                this.ContextMode = MediaItemType.Song;
                Song song = context as Song;
                textRow1.Text = song.Name;
                textRow2.Text = song.Artist;

                Color color = ImageHelper.GetColorFromHex(song.HexColor);
                gradientStop1.Color = color.ChangeColorBrightness(-0.3f);

                gradientStop2.Color = color;
                playButton.Background = new SolidColorBrush(color);

                if (color.IsDarkColor())
                    playButton.RequestedTheme = ElementTheme.Dark;
                else
                    playButton.RequestedTheme = ElementTheme.Light;

                BitmapImage bmp = new BitmapImage();
                bmp.ImageFailed += Bmp_ImageFailed;
                backgroundImage.ImageSource = bmp;

                bmp.UriSource = new Album() { ID = song.AlbumID }.GetCoverUri();
            }
            else if (context.GetType() == typeof(Album))
            {
                this.ContextMode = MediaItemType.Album;

            }
            else if (context.GetType() == typeof(Artist))
            {
                this.ContextMode = MediaItemType.Artist;

            }
        }

        private void Bmp_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (this.ContextMode == MediaItemType.Song || this.ContextMode == MediaItemType.Album)
                ((BitmapImage)sender).UriSource = new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute);
        }

        private void Card_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ContextMode == MediaItemType.Song)
            {
                Song song = this.DataContext as Song;

                PlayerController.Play(song);
            }
            else if (this.ContextMode == MediaItemType.Album)
            {

            }
            else if (this.ContextMode == MediaItemType.Artist)
            {

            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            if (this.ContextMode == MediaItemType.Song)
            {
                Song song = this.DataContext as Song;
                list.Add(song.SongURI);
            }
            else if (this.ContextMode == MediaItemType.Album)
            {

            }
            else if (this.ContextMode == MediaItemType.Artist)
            {

            }
            PlaylistHelper.RequestPlaylistPicker(this, list);
        }

        private void DotsButton_Click(object sender, RoutedEventArgs e)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(_mediaItem);
        }
    }
}
