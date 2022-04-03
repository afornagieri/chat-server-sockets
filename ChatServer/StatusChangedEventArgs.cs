using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    //Handle with status changed events
    public class StatusChangedEventArgs : EventArgs
    {
        //Event message
        private string EventMsg;

        //Property to return and define a event message
        public string EventMessage
        {
            get { return EventMessage; }
            set { EventMsg = value; }
        }

        //Constructor to define a event message
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }
}
