using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoreLib.Modules.Application
{
    public static class ApplicationHelper
    {
        //TODO : удалить! Только для Эврики
        public static string AdminLogin { get { return ApplicationSettings.AppSettings.AdminLogin; } }
        public static string AdminPassword { get { return ApplicationSettings.AppSettings.AdminPassword; } }

        public static bool LastNameFirst { get { return ApplicationSettings.AppSettings.LastNameFirst; } }
        public static string EncryptionKey { get { return ApplicationSettings.AppSettings.EncryptionKey; } }
        public static int FormsAuthenticationTicketTime { get { return ApplicationSettings.AppSettings.FormsAuthenticationTicketTime; } }
        public static TimeSpan StatusCheckTime { get { return ApplicationSettings.AppSettings.StatusCheckTime; } }
        public static string CompanyName { get { return ApplicationSettings.AppSettings.CompanyName; } }
    }
}
