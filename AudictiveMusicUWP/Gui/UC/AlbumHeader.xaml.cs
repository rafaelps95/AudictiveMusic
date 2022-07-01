using AudictiveMusicUWP.Gui.Pages;
using AudictiveMusicUWP.Gui.Util;
using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class AlbumHeader : UserControl
    {
        private Album Album
        {
            get;
            set;
        }

        public AlbumHeader()
        {
            this.Loaded += AlbumHeader_Loaded;
            this.SizeChanged += AlbumHeader_SizeChanged;
            this.InitializeComponent();
        }

        private void AlbumHeader_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void AlbumHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 420)
            {
                this.Height = 250;
                border.Margin = new Thickness(25, 0, 10, 0);
                border.Height = border.Width = 150;
                secondRow.Height = thirdRow.Height = new GridLength(75);
            }
            else
            {
                this.Height = 230;
                border.Margin = new Thickness(10, -2, 0, 2);
                border.Height = border.Width = 130;
                secondRow.Height = thirdRow.Height = new GridLength(65);
            }
        }

        public void ClearContext()
        {
            this.DataContext = null;
            borderBrush.ImageSource = null;
            rootBrush.ImageSource = null;
            countOfSongs.Text = "";
        }

        public void SetContext(Album album)
        {
            this.DataContext = Album = album;

            BitmapImage bmp = new BitmapImage();
            borderBrush.ImageSource = bmp;
            bmp.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + Album.ID + ".jpg", UriKind.Absolute);


            BitmapImage blurbmp = new BitmapImage();
            rootBrush.ImageSource = blurbmp;
            //blurbmp.UriSource = new Uri("ms-appdata:///local/Artists/artist_" + StringHelper.RemoveSpecialChar(ALB.Artist) + ".jpg", UriKind.Absolute);
            blurbmp.UriSource = new Uri("ms-appdata:///local/Covers/cover_" + Album.ID + ".jpg", UriKind.Absolute);

            albumName.Text = Album.Name.ToUpper();
        }

        public void UpdateNumberOfItems(int nSongs)
        {
            if (nSongs > 1)
                // BUSCA A STRING PLURAL JÁ QUE HÁ MAIS DE UMA MÚSICA NA LISTA
                countOfSongs.Text = nSongs + " " + ApplicationInfo.Current.Resources.GetString("SongPlural").ToLower();
            else
                // BUSCA A STRING SINGULAR JÁ QUE HÁ APENAS UMA MÚSICA NA LISTA
                countOfSongs.Text = nSongs + " " + ApplicationInfo.Current.Resources.GetString("SongSingular").ToLower();
        }

        private void border_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 1, 200, border, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void border_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            borderBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/cover-error.png", UriKind.Absolute));
        }

        private void Blurbmp_ImageOpened(object sender, RoutedEventArgs e)
        {
            Animation animation = new Animation();
            animation.AddDoubleAnimation(0, 0.4, 300, background, "Opacity", Animation.GenerateEasingFunction(EasingFunctionType.CircleEase, EasingMode.EaseOut));

            animation.Begin();
        }

        private void moreButton_Click(object sender, RoutedEventArgs e)
        {
            PopupHelper.GetInstance(sender).ShowPopupMenu(Album, true, new Point(0, 0));
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerController.Play(this.Album);
        }

        private void artistName_Click(object sender, RoutedEventArgs e)
        {
            Artist art = new Artist()
            {
                Name = Album.Artist
            };

            NavigationService.Navigate(this, typeof(ArtistPage), art);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();

            var songs = Ctr_Song.Current.GetSongsByAlbum(Album);

            foreach (Song song in songs)
                list.Add(song.SongURI);

            PlaylistHelper.RequestPlaylistPicker(this, list);
        }
    }
}
