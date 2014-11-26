using System;
using System.Linq;
using DataRepository.Common;

namespace DataRepository
{
    public partial class User
    {
        //TODO. Tolik. Needed for registration
        //public static string GetActivateUrl()
        //{
        //    return Guid.NewGuid().ToString("N");
        //}

        //public string ConfirmPassword { get; set; }

        //public string Captcha { get; set; }

        public UserType UserRole
        {
            get { return (UserType) UserRoleId; }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}{2}", LastName, FirstName,
                    !string.IsNullOrWhiteSpace(MiddleName) ? string.Format(" {0}", MiddleName) : string.Empty);
            }
        }

        public bool InRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
            {
                return false;
            }

            var rolesArray = roles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return rolesArray.Any(p => string.Compare(p, UserRole.ToString(), StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
