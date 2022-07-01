﻿using AudictiveMusicUWP.Gui.Util;
using ClassLibrary.Control;
using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Graphics.Canvas.Effects;
using RPSToolkit;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AudictiveMusicUWP.Gui.UC
{
    public sealed partial class PlaylistPicker : UserControl
    {
        private Playlist Favorites;

        private enum SaveMode
        {
            Existing,
            New
        }

        private SaveMode modeaux;
        private SaveMode Mode
        {
            get
            {
                return modeaux;
            }
            set
            {
                modeaux = value;
                if (value == SaveMode.Existing)
                {
                    playlistsList.IsEnabled = true;
                    createButton.Visibility = Visibility.Visible;
                    playlistName.Visibility = Visibility.Collapsed;
                    tapToSelectOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    playlistsList.SelectedItem = null;
                    playlistsList.IsEnabled = false;
                    createButton.Visibility = Visibility.Collapsed;
                    playlistName.Visibility = Visibility.Visible;
                    tapToSelectOverlay.Visibility = Visibility.Visible;
                }

                playlistName.Text = string.Empty;
            }
        }

        public List<string> Songs;

        public PlaylistPicker()
        {
            this.Loaded += PlaylistPicker_Loaded;
            this.SizeChanged += PlaylistPicker_SizeChanged;
            this.InitializeComponent();
            Mode = SaveMode.Existing;
            Songs = new List<string>();
        }

        private void PlaylistPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void PlaylistPicker_Loaded(object sender, RoutedEventArgs e)
        {
            border.ApplyShadow();
        }

        public async void Set(List<Playlist> playlists, List<string> songs)
        {
            if (songs.Count == 1)
            {
                Song s = Ctr_Song.Current.GetSong(new Song() { SongURI = songs[0] });
                if (s.IsFavorite == true)
                    addToFavoritesButton.Visibility = Visibility.Collapsed;
            }

            Songs.AddRange(songs);
            playlistsList.ItemsSource = playlists;
            Favorites = await CustomPlaylistsHelper.GetFavorites();
        }

        private void blur_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            NavigationService.Back(this);
        }

        private void playlistsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playlistsList.SelectedItem != null)
            {
                doneButton.IsEnabled = true;
            }
            else
            {
                doneButton.IsEnabled = false;
            }
        }

        private void tapToSelectOverlay_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Mode = SaveMode.Existing;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            Mode = SaveMode.New;
            playlistName.Focus(FocusState.Keyboard);
        }

        private async void doneButton_Click(object sender, RoutedEventArgs e)
        {
            Playlist playlist = null;

            if (Mode == SaveMode.Existing)
            {
                playlist = playlistsList.SelectedItem as Playlist;
                playlist.Songs.AddRange(Songs);
            }
            else
            {
                DateTime now = DateTime.Now;
                string fileName = String.Format("{0}{1}{2}_{3}{4}{5}.xml", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

                playlist = new Playlist(playlistName.Text, fileName, Songs);
            }

            bool result = await CustomPlaylistsHelper.SaveToPlaylist(playlist);

            if (result == true)
            {
                NavigationService.Back(this);
            }
            else
            {

            }
        }

        private void playlistName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Mode == SaveMode.New)
            {
                if (string.IsNullOrWhiteSpace(playlistName.Text) == false)
                {
                    doneButton.IsEnabled = true;
                }
                else
                {
                    doneButton.IsEnabled = false;
                }
            }
        }

        private void addToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            Favorites.Songs.AddRange(Songs);

            foreach (string str in Songs)
            {
                Song s = Ctr_Song.Current.GetSong(new Song() { SongURI = str });
                Ctr_Song.Current.SetFavoriteState(s, true);
            }

            NavigationService.Back(this);
        }
    }
}
