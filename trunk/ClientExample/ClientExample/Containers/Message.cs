using System;
using System.Collections.Generic;
using System.Text;

namespace ClientExample.Containers
{
    public class Message
    {
        public string GroupName
        {
            get;
            set;
        }

        public string GroupId
        {
            get;
            set;
        }

        public string From
        {
            get;
            set;
        }

        public string To
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }
    }
}
