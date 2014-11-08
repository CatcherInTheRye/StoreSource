using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataRepository
{
    public partial class user
    {
        //TODO. Tolik. Needed for registration
        //public static string GetActivateUrl()
        //{
        //    return Guid.NewGuid().ToString("N");
        //}

        //public string ConfirmPassword { get; set; }

        //public string Captcha { get; set; }

        public int UserRole
        {
            get {
                userRole role = userRoles.FirstOrDefault();
                return role != null ? role.roleId : -1;
            }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}{2}", lastName, firstName,
                    !string.IsNullOrWhiteSpace(middleName) ? string.Format(" {0}", middleName) : "");
            }
        }

        public bool InRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
            {
                return false;
            }

            var rolesArray = roles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in rolesArray)
            {
                var hasRole = userRoles.Any(p => string.Compare(p.roleId.ToString(), role, true) == 0);
                if (hasRole)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
