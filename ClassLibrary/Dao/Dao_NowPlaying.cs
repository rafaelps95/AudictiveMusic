using BackgroundAudioShared;
using ClassLibrary.Control;
using ClassLibrary.Db;
using ClassLibrary.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClassLibrary.Dao
{
    public static class Dao_NowPlaying
    {
        public static List<string> LoadPlaylist()
        {
            List<string> list = new List<string>();
            SqliteConnection db =
        new SqliteConnection("Filename=database.db");

            try
            {
                db.Open();

                if (NowPlaying.Current.Songs.Count == 0)
                {
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = db;
                    //command.CommandText = @"SELECT songs.Title, songs.Artist, songs.Album, songs.Genre, songs.Year, songs.Track, songs.AlbumID, songs.URI, songs.HexColor, songs.Star, songs.DateAdded FROM lastplaylist_songs INNER JOIN songs ON songs.URI = lastplaylist_songs.SongUri;";
                    command.CommandText = @"SELECT SongUri FROM lastplaylist_songs;";

                    SqliteDataReader query = command.ExecuteReader();

                    while (query.Read())
                    {
                        list.Add(query.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }

            return list;
        }

        public static bool SavePlaylist()
        {
            bool result = false;

            if (NowPlaying.Current.Songs.Count == 0)
                return false;

            SqliteConnection db =
        new SqliteConnection("Filename=database.db");


            // DROP TABLE
            try
            {
                db.Open();


                SqliteCommand command = db.CreateCommand();
                command.CommandText = @"DELETE FROM lastplaylist_songs;";
                command.ExecuteNonQuery();

                result = true;
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


            try
            {
                db.Open();

                using (var trans = db.BeginTransaction())
                {
                    SqliteCommand command = db.CreateCommand();
                    command.CommandText = @"INSERT INTO lastplaylist_songs (SongUri) VALUES (@SONGURI);";

                    SqliteParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@SONGURI";
                    command.Parameters.Add(parameter);

                    for (int i = 0; i < NowPlaying.Current.Songs.Count; i++)
                    {
                        string song = NowPlaying.Current.Songs[i];
                        parameter.Value = song;
                        command.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                result = false;
            }
            finally
            {
                db.Close();
            }

            ApplicationData.Current.LocalSettings.Values["ExistsLastPlayback"] = result;

            return result;
        }

        public static bool ImportLegacyXmlPlaylistToDatabase(List<string> list)
        {
            bool result = false;

            if (list.Count == 0)
                return false;

            SqliteConnection db =
        new SqliteConnection("Filename=database.db");


            // DROP TABLE
            try
            {
                db.Open();


                SqliteCommand command = db.CreateCommand();
                command.CommandText = @"DELETE FROM lastplaylist_songs;";
                command.ExecuteNonQuery();

                result = true;
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


            try
            {
                db.Open();

                using (var trans = db.BeginTransaction())
                {
                    SqliteCommand command = db.CreateCommand();
                    command.CommandText = @"INSERT INTO lastplaylist_songs (SongUri) VALUES (@SONGURI);";

                    SqliteParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@SONGURI";
                    command.Parameters.Add(parameter);

                    for (int i = 0; i < list.Count; i++)
                    {
                        string song = list[i];
                        parameter.Value = song;
                        command.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                result = false;
            }
            finally
            {
                db.Close();
            }

            ApplicationData.Current.LocalSettings.Values["ExistsLastPlayback"] = result;

            return result;
        }

    }
}
