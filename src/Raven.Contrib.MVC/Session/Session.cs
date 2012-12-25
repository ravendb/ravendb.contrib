using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.SessionState;

namespace Raven.Contrib.MVC.Session
{
    internal class Session
    {
        public Session(string sessionId, string applicationName)
        {
            Id              = sessionId;
            ApplicationName = applicationName;
            Created         = DateTime.UtcNow;
            SessionItems    = String.Empty;
        }

        public string Id { get; set; }

        public string ApplicationName { get; set; }

        public DateTime Created  { get; set; }
        public DateTime Expires  { get; set; }
        public DateTime LockDate { get; set; }

        public int  LockId { get; set; }
        public bool Locked { get; set; }

        public string SessionItems { get; set; }

        public SessionStateActions Flags { get; set; }
    }
}
