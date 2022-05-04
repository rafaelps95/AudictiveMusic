using ClassLibrary.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ClassLibrary.Entities
{
    public class Song : MediaItem, INotifyPropertyChanged
    {
        public string Artist
        {
            get;
            set;
        }

        public string Album
        {
            get;
            set;
        }

        public string Genre
        {
            get;
            set;
        }

        public string Year
        {
            get;
            set;
        }

        public string Track
        {
            get;
            set;
        }

        public int TrackNumber
        {
            get
            {
                return Convert.ToInt32(Track);
            }
        }

        public string AlbumID
        {
            get;
            set;
        }

        public string SongURI
        {
            get;
            set;
        }

        public string HexColor
        {
            get;
            set;
        }

        public Color Color
        {
            get
            {
                return ImageHelper.GetColorFromHex(HexColor);
            }
        }

        public bool IsFavorite 
        {
            get; 
            set; 
        }

        private bool isSelected = false;

        public bool IsPlaying
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateAdded 
        { 
            get; 
            set; 
        }

        public ImageSource Image
        {
            get
            {
                return new BitmapImage(new Uri("ms-appdata:///local/Covers/cover_" + AlbumID + ".jpg", UriKind.Absolute));
            }
        }

        public Song()
        {
            Name = string.Empty;
            Artist = string.Empty;
            Album = string.Empty;
            AlbumID = string.Empty;
            ID = string.Empty;
            Year = string.Empty;
            Track = string.Empty;
            Genre = string.Empty;
            SongURI = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
