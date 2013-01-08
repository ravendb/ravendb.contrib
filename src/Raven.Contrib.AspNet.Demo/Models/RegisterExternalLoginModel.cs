using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class RegisterExternalLoginModel : ExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName
        {
            get;
            set;
        }
    }
}