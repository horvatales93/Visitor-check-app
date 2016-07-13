using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPoint.Classes
{
    public class ResponseObject
    {
        public bool Success { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string ConferenceName { get; set; }
        public string ConferenceSlug { get; set; }
        public string ConferenceLogo { get; set; }
        public string Error { get; set; }
        public int ErrorId { get; set; }

        public override string ToString()
        {
            return String.Format("Uporabniško : {0} , {1} {2} {3} {4}", UserName, UserId, ConferenceName, ConferenceSlug, ConferenceLogo);
        }
    }

}
