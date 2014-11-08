using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PCSMvc.Models.ViewModels
{
    public class LoginView
    {
        [Required(ErrorMessage = "Enter username")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Enter password")]
        public string Password { get; set; }
        public bool IsPersistent { get; set; }
    }
}