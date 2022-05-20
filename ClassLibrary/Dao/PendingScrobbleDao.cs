using ClassLibrary.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Dao
{
    internal class PendingScrobbleDao
    {
        internal static bool Add(PendingScrobble pendingScrobble)
        {
            bool result = false;

            SqliteConnection db =
        new SqliteConnection("Filename=database.db");

            try
            {
                db.Open();

                SqliteCommand command = db.CreateCommand();
                command.CommandText = @"INSERT INTO pendingscrobbles (URI,Title,Artist,Album,Time) VALUES (@URI, @TITLE, @ARTIST, @ALBUM, @TIME);";

                SqliteParameter uri = command.CreateParameter();
                uri.ParameterName = "@URI";
                command.Parameters.Add(uri);

                SqliteParameter title = command.CreateParameter();
                title.ParameterName = "@TITLE";
                command.Parameters.Add(title);

                SqliteParameter artist = command.CreateParameter();
                artist.ParameterName = "@ARTIST";
                command.Parameters.Add(artist);

                SqliteParameter album = command.CreateParameter();
                album.ParameterName = "@ALBUM";
                command.Parameters.Add(album);

                SqliteParameter time = command.CreateParameter();
                time.ParameterName = "@TIME";
                command.Parameters.Add(time);

                if (pendingScrobble == null)
                    return false;

                if (pendingScrobble.Song != null)
                {
                    uri.Value = pendingScrobble.Song.SongURI;
                    title.Value = pendingScrobble.Song.Name;
                    artist.Value = pendingScrobble.Song.Artist;
                    album.Value = pendingScrobble.Song.Album;
                    time.Value = pendingScrobble.Time;

                    command.ExecuteNonQuery();

                    result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!!! " + ex.Message);
                result = false;
            }
            finally
            {
                db.Close();
            }

            return result;
        }

        internal static List<PendingScrobble> GetPendingScrobbles()
        {
            List<PendingScrobble> list = new List<PendingScrobble>();
            PendingScrobble pendingScrobble;

            SqliteConnection db =
new SqliteConnection("Filename=database.db");
            try
            {
                db.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = db;

                command.CommandText = @"SELECT pendingscrobbles.Title, pendingscrobbles.Artist, pendingscrobbles.Album, songs.AlbumID, songs.URI, pendingscrobbles.ID, pendingscrobbles.Time FROM pendingscrobbles INNER JOIN songs ON songs.URI = pendingscrobbles.URI;";

                SqliteDataReader query = command.ExecuteReader();

                while (query.Read())
                {
                    Song song = new Song();
                    song.Name = query.GetString(0);
                    song.Artist = query.GetString(1);
                    song.Album = query.GetString(2);
                    song.AlbumID = query.GetString(3);
                    song.SongURI = query.GetString(4);
                    int id = query.GetInt32(5);
                    DateTime time = DateTime.Parse(query.GetString(6));
                    pendingScrobble = new PendingScrobble(song, time);
                    pendingScrobble.ID = id;

                    list.Add(pendingScrobble);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }

            return list;
        }

        internal static bool ClearList()
        {
            bool result = false;

            SqliteConnection db =
        new SqliteConnection("Filename=database.db");
            try
            {
                db.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = db;
                command.CommandText = "DELETE FROM pendingscrobbles;";
                command.ExecuteNonQuery();

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                db.Close();
            }

            return result;
        }

        internal static bool Remove(PendingScrobble pendingScrobble)
        {
            bool result = false;

            SqliteConnection db =
        new SqliteConnection("Filename=database.db");
            try
            {
                db.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = db;

                command.CommandText = "DELETE FROM pendingscrobbles WHERE ID = @ID;";
                command.Parameters.AddWithValue("@ID", pendingScrobble.ID);

                command.ExecuteNonQuery();

                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                db.Close();
            }

            return result;
        }
    }
}
