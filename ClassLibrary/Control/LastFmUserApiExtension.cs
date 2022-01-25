using ClassLibrary.Entities;
using ClassLibrary.Entities.LastFmHelper;
using ClassLibrary.Helpers;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ClassLibrary.Control
{
    public static class LastFmUserApiExtension
    {
        public static async Task<LastFmFriendsResponse> GetFollowedUsers(this UserApi api, LastUser user, int page = 1, int resultsPerPage = 50)
        {
            LastFmFriendsResponse friends = null;
            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(string.Format("http://ws.audioscrobbler.com/2.0/?method=user.getfriends&user={0}&api_key={1}&format=json&page={2}&limit={3}", user.Name, ApplicationSettings.LastFmAPIKey, page, resultsPerPage));

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                var response = await httpClient.SendRequestAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    responseContent = responseContent.Replace("#", "").Replace("@", "");
                    friends = JsonConvert.DeserializeObject<LastFmFriendsResponse>(responseContent);

                    Debug.WriteLine(responseContent);
                }
                else
                {
                    Debug.WriteLine(response.StatusCode.ToString());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return friends;
        }

        public static async Task<List<LastTrack>> GetTopTracks(this UserApi api, LastUser user, int limit = 20)
        {
            LastFmTopTracksResponse topTracks = null;
            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();
                Uri uri = new Uri(string.Format("http://ws.audioscrobbler.com/2.0/?method=user.getTopTracks&user={0}&api_key={1}&limit={2}&format=json", user.Name, ApplicationSettings.LastFmAPIKey, limit));

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                var response = await httpClient.SendRequestAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    responseContent = responseContent.Replace("#", "");
                    topTracks = JsonConvert.DeserializeObject<LastFmTopTracksResponse>(responseContent);

                    Debug.WriteLine(responseContent);
                }
                else
                {
                    Debug.WriteLine(response.StatusCode.ToString());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (topTracks != null)
            {
                List<LastTrack> list = new List<LastTrack>();
                foreach (Track t in topTracks.toptracks.track)
                {
                    LastTrack lt = new LastTrack()
                    {
                        ArtistName = t.artist.name,
                        Name = t.name,
                        PlayCount = Convert.ToInt32(t.playcount),
                        Url = new Uri(t.url),
                    };
                    list.Add(lt);
                }

                return list;
            }

            return null;
        }


    }
}
