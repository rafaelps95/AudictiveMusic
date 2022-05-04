using ClassLibrary.Entities;
using ClassLibrary.Helpers;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Dao
{
    internal class AlbumDao
    {
        internal static Album GetAlbum(Album album)
        {
            SqliteConnection db =
        new SqliteConnection("Filename=database.db");
            try
            {
                db.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = db;

                command.CommandText = "SELECT Artist, Album, Genre, Year, HexColor FROM songs WHERE AlbumID = @ALBUMID ";
                command.Parameters.AddWithValue("@ALBUMID", album.ID);

                SqliteDataReader query = command.ExecuteReader();

                while (query.Read())
                {
                    album.Artist = query.GetString(0);
                    album.Name = query.GetString(1);
                    album.Genre = query.GetString(2);
                    album.Year = Convert.ToInt32(query.GetString(3));
                    album.HexColor = query.GetString(5);
                }


            }
            catch
            {

            }
            finally
            {
                db.Close();
            }

            return album;
        }

        internal static List<Album> GetAlbumsByArtist(Artist artist)
        {
            List<Album> list = new List<Album>();
            List<Song> listOfSongs = SongDao.GetSongsByArtist(artist);
            Album aux;
            if (ApplicationSettings.IsCollectionLoaded)
            {
                foreach (Song s in listOfSongs)
                {
                    if (list.Exists(a => a.ID == s.AlbumID) == false)
                    {
                        aux = GetAlbum(new Album() { ID = s.AlbumID });
                        list.Add(aux);
                    }
                }
            }

            list = list.OrderBy(s => s.Year).Reverse().ToList();

            return list;
        }

    }
}
