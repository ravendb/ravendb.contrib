using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class ManageModel : LocalPasswordModel
    {
        public string UserName
        {
            get;
            set;
        }

        public string StatusMessage
        {
            get;
            set;
        }

        public bool HasLocalPassword
        {
            get;
            set;
        }
    }
}