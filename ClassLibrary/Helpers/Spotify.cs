using BackgroundAudioShared;
using ClassLibrary.Entities;
using SpotifyApiUniversal;
using SpotifyApiUniversal.Control;
using SpotifyApiUniversal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Net.Http;

namespace ClassLibrary.Helpers
{
    public static class Spotify
    {
        public delegate void RoutedEventArgs(Artist artist);
        public static event RoutedEventArgs DownloadCompleted;

        public static void Connect(string id, string secret)
        {
            SpotifyConnect.Current.Initialize(id, secret);
        }

        public static async Task DownloadArtistImage(Artist artist)
        {
            SpotifyArtist spotifyArtist = await searchArtist(artist);

            if (spotifyArtist == null)
                return;

            string artistNameWithOutSpaces = StringHelper.RemoveSpecialChar(artist.Name);
            StorageFolder artistImagesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artists", CreationCollisionOption.OpenIfExists);

            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage message = await client.GetAsync(new Uri(spotifyArtist.images[0].url));

                StorageFile file = await artistImagesFolder.CreateFileAsync("artist_" + artistNameWithOutSpaces + ".jpg", CreationCollisionOption.OpenIfExists);// this line throws an exception
                byte[] bytes = await message.Content.ReadAsByteArrayAsync();

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var outputStream = stream.GetOutputStreamAt(0))
                    {
                        DataWriter writer = new DataWriter(outputStream);
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }

                DownloadCompleted?.Invoke(artist);

                //ImageHelper.BlurArtistImage(name);
            }
            catch (Exception ex)
            {
                DownloadCompleted?.Invoke(artist);
            }


        }

        private static async Task<SpotifyArtist> searchArtist(Artist artist)
        {
            return await SpotifyArtistControl.GetSpotifyArtistByQuery(artist.Name);
        }
    }
}
