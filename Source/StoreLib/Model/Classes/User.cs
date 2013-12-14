using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Modules.Helpers;

namespace StoreLib.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Statuses Status { get; set; }
        public UserTypes UserType { get; set; }

        public string FullName { get { return StringHelper.FullName(FirstName, LastName); } }
    }
}
