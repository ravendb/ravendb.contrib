using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class RemoveExternalLoginModel : ExternalLoginModel
    {
        public IEnumerable<ExternalLoginModel> ExternalLogins
        {
            get;
            set;
        }

        public bool ShowRemoveButton
        {
            get;
            set;
        }
    }
}