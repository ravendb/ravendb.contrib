using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class ExternalLoginModel
    {
        [Required]
        public string ProviderName
        {
            get;
            set;
        }

        [Required]
        public string UserIdentifier
        {
            get;
            set;
        }
    }
}