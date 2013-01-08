using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Raven.Contrib.AspNet.Demo.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName
        {
            get;
            set;
        }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get;
            set;
        }

        [Display(Name = "Remember me?")]
        public bool RememberMe
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