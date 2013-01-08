using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class ExternalLoginsListModel
    {
        public IEnumerable<string> ProviderNames
        {
            get;
            set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }
    }
}