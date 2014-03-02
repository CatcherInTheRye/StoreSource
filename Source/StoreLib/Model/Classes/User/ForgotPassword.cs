using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using StoreLib.Modules.Application;
using StoreLib.Modules.ValIdation;
using StoreLib.Services.Interface;

namespace StoreLib.Model.Classes
{
    [MetadataType(typeof(ForgotPassword))]
    public class ForgotPassword
    {
        [RegularExpression(RegularExpressions.Email, ErrorMessage = "Email is invalId.")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(150, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 150 characters long.")]
        [RegularExpression(RegularExpressions.UserNameNonSpace, ErrorMessage = "Special characters are not allowed in Username.")]
        public string Login { get; set; }

        private static bool UniqueEmail { get { return ApplicationHelper.UniqueEmail; } }

        public void ValIdate(ModelStateDictionary modelState, IUserService userService)
        {
            ValIdationCheck.CheckErrors(this, modelState);

            if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Login))
            {
                modelState.AddModelError("Login", "Field 'Login' is required. Please correct information and try again.");
                modelState.AddModelError("Email", "Field 'E-mail' is required. Please correct information and try again.");
            }

            if (UniqueEmail)
            {
                if (!ValIdationCheck.IsEmpty(Email) && ValIdationCheck.IsEmail(Email))
                {
                    User user = userService.GetUserByEmail(Email, false, true);
                    if (user == null)
                    {
                        modelState.AddModelError("Email", "Sorry, the e-mail address entered was not found.  Please try again.");
                        return;
                    }
                    if (user.Status == Statuses.Locked) modelState.AddModelError("Email", string.Format("Your account with {0} has been locked do to a violation in our site policly.<br /> If you feel that this is a mistake, please contact support <a href='mailto:{1}'>{1}</a>", ApplicationHelper.SiteName, ApplicationHelper.SiteEmail));
                }
            }
            else
            {
                if (!ValIdationCheck.IsEmpty(Login))
                {
                    User user = userService.GetUser(Login, false, true);
                    if (user == null)
                    {
                        modelState.AddModelError("Login", "Sorry, the login entered was not found.  Please try again.");
                        return;
                    }
                    if (user.Status == Statuses.Locked) modelState.AddModelError("Login", string.Format("Your account with {0} has been locked do to a violation in our site policly.<br /> If you feel that this is a mistake, please contact support <a href='mailto:{1}'>{1}</a>.", ApplicationHelper.SiteName, ApplicationHelper.SiteEmail));
                }
            }
        }
    }
}
