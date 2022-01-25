using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Entities.LastFmHelper
{
    public class LastFmFriendsResponse
    {
        public Friends friends { get; set; }
    }

    public class Friends
    {
        public Attr attr { get; set; }
        public User[] user { get; set; }
    }

    public class Attr
    {
        public string user { get; set; }
        public string totalPages { get; set; }
        public string page { get; set; }
        public string total { get; set; }
        public string perPage { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string url { get; set; }
        public string country { get; set; }
        public string playlists { get; set; }
        public string playcount { get; set; }
        public Image[] image { get; set; }
        public Registered registered { get; set; }
        public string realname { get; set; }
        public string subscriber { get; set; }
        public string bootstrap { get; set; }
        public string type { get; set; }
    }

    public class Registered
    {
        public string unixtime { get; set; }
        public string text { get; set; }
    }

    public class Image
    {
        public string size { get; set; }
        public string text { get; set; }
    }

}
