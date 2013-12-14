using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoreLib.Modules.Application;

namespace StoreLib.Modules.Helpers
{
    public static class StringHelper
    {
        public static string FullName(string firstName, string lastName)
        {
            return ApplicationHelper.LastNameFirst
                       ? string.Format("{0} {1}", lastName, firstName)
                       : string.Format("{0} {1}", firstName, lastName);
        }

        public static string PageTitle(string title)
        {
            string companyName = ApplicationHelper.CompanyName;
            return string.Format("{0}{1}",
                                 !string.IsNullOrEmpty(title) ? title.Limit(65 - title.Length) + " | " : string.Empty,
                                 companyName);
        }


    }
}
