using ClassLibrary.Dao;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace ClassLibrary.Db
{
    public class DB
    {
        private static SqliteConnection db;

        public static async void InitializeDatabase()
        {
            db = new SqliteConnection("Filename=database.db");

            CreateSongsTable();
            CreatePlaylistsTable();
            CreatePlaylistsSongsTable();
            CreateLastPlaylistSongsTable();
            CreatePendingScrobblesTable();
        }

        public static bool CreateSongsTable()
        {
            bool result = false;
            // CREATE SONGS TABLE
            try
            {

                db.Open();

                String tableCommand = "CREATE TABLE IF NOT EXISTS songs" +
                    "(ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Title VARCHAR(255) NULL, " +
                    "Artist VARCHAR(255) NULL, " +
                    //"AlbumArtist VARCHAR(255) NULL, " +
                    "Album VARCHAR(255) NULL, " +
                    "Genre VARCHAR(255) NULL, " +
                    "Year VARCHAR(255) NULL, " +
                    "Track VARCHAR(255) NULL, " +
                    "AlbumID VARCHAR(255) NULL, " +
                    "URI VARCHAR(255) NULL, " +
                    "HexColor VARCHAR(9) NULL, " +
                    "Star INTEGER NULL, " +
                    "DateAdded DateTime)";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }


            return result;
        }

        public static bool CreatePlaylistsTable()
        {
            bool result = false;

            // CREATE PLAYLISTS TABLE
            try
            {

                db.Open();

                String tableCommand = "CREATE TABLE IF NOT EXISTS playlists" +
                    "(ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name VARCHAR(255), " +
                    "ModifiedDate DateTime)";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }

            return result;
        }

        public static bool CreatePlaylistsSongsTable()
        {
            bool result = false;

            // CREATE PLAYLISTS_SONGS TABLE
            try
            {

                db.Open();

                String tableCommand = "CREATE TABLE IF NOT EXISTS playlists_songs" +
                    "(ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "SongUri VARCHAR(255), " +
                    "Position INTEGER)";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }


            return result;
        }

        public static bool CreateLastPlaylistSongsTable()
        {
            bool result = false;

            // RECREATE LASTPLAYLIST_SONGS TABLE
            try
            {

                db.Open();

                String tableCommand = "CREATE TABLE IF NOT EXISTS lastplaylist_songs" +
                    "(SongUri VARCHAR(255))";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }

            ImportLastPlaylistXMLToDatabase();

            return result;
        }

        private static bool CreatePendingScrobblesTable()
        {
            bool result = false;
            // CREATE SONGS TABLE
            try
            {

                db.Open();

                String tableCommand = "CREATE TABLE IF NOT EXISTS pendingscrobbles" +
                    "(ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "URI VARCHAR(255) NULL, " +
                    "Title VARCHAR(255) NULL, " +
                    "Artist VARCHAR(255) NULL, " +
                    "Album VARCHAR(255) NULL, " +
                    "Time DateTime)";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO!!! " + ex.Message);
            }
            finally
            {
                db.Close();
            }


            return result;
        }

        private static async void ImportLastPlaylistXMLToDatabase()
        {
            IStorageItem lastPlaylistItem = await ApplicationData.Current.LocalFolder.TryGetItemAsync("LastPlayback.xml");

            List<string> list = new List<string>();

            if (lastPlaylistItem != null)
            {
                XmlDocument doc = new XmlDocument();

                string content = await FileIO.ReadTextAsync(lastPlaylistItem as StorageFile);

                if (content != null && string.IsNullOrWhiteSpace(content) == false)
                {
                    doc.LoadXml(content);

                    var elements = doc.GetElementsByTagName("Song");

                    for (int i = 0; i < elements.Count; i++)
                    {
                        XmlElement element = elements[i] as XmlElement;
                        list.Add(element.InnerText);
                    }
                }
            }

            if (Dao_NowPlaying.ImportLegacyXmlPlaylistToDatabase(list))
            {
                await lastPlaylistItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }
}
