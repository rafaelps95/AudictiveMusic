using ClassLibrary.Entities.LastFmHelper;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Entities
{
    public class CustomLastUser
    {
        public CustomLastUser(User u)
        {
            SetData(u);
            
        }

        private void SetData(User u)
        {
            this.Name = u.name;
            this.RealName = u.realname;
            try
            {
                this.ProfilePic = new Uri(u.image[1].text);
            }
            catch
            {

            }
        }

        public string Name
        {
            get; set;
        }

        public string RealName
        {
            get; set;
        }

        public Uri ProfilePic
        {
            get; set;
        }


    }
}
