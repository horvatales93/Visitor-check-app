using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetingPoint.Classes;

namespace MeetingPoint.Classes
{
    public class QRResponse
    {
        public bool Success { get; set; }
        public Conference Conference { get; set; }
        public Participant Participant { get; set; }
        public string Error { get; set; }
        public int ErrorID { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1}",Participant.FirstName,Participant.LastName);
        }
    }
}
