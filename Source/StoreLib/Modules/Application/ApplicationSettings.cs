using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace StoreLib.Modules.Application
{
    public class ApplicationSettings
    {
        //TODO : удалить! Только для Эврики
        public string AdminLogin { get; private set; }
        public string AdminPassword { get; private set; }

        public bool LastNameFirst { get; private set; }
        public string EncryptionKey { get; private set; }
        public int FormsAuthenticationTicketTime { get; private set; }
        public TimeSpan StatusCheckTime { get; private set; }
        public string CompanyName { get; private set; }
        public string Sitename { get; private set; }

        public bool IsSsl { get; private set; }
        public bool IsStandartPortsSsl { get; private set; }
        public string Port { get; private set; }
        public string PortSsl { get; private set; }
        public string SiteAddress { get; private set; }
        public bool UniqueEmail { get; private set; }
        public string SiteEmail { get; private set; }

        public static ApplicationSettings AppSettings
        {
            get
            {
                if (HttpContext.Current.Application["ApplicationSettings"] == null)
                {
                    HttpContext.Current.Application["ApplicationSettings"] = new ApplicationSettings();
                }
                return (ApplicationSettings) HttpContext.Current.Application["ApplicationSettings"];
            }
        }

        public ApplicationSettings()
        {
            //TODO : удалить! Только для Эврики
            string value = ConfigurationManager.AppSettings["AdminLogin"];
            AdminLogin = !string.IsNullOrEmpty(value) ? value : string.Empty;
            value = ConfigurationManager.AppSettings["AdminPassword"];
            AdminPassword = !string.IsNullOrEmpty(value) ? value : string.Empty;
            
            value = ConfigurationManager.AppSettings["LastNameFirst"];
            LastNameFirst = false;
            if (!string.IsNullOrEmpty(value))
            {
                bool flag = false;
                bool.TryParse(value, out flag);
                LastNameFirst = flag;
            }
            EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
            value = ConfigurationManager.AppSettings["FormsAuthenticationTicketTime"];
            FormsAuthenticationTicketTime = 30;
            if (!string.IsNullOrEmpty(value))
            {
                int i;
                if (int.TryParse(value, out i))
                {
                    FormsAuthenticationTicketTime = i;
                }
            }
            value = ConfigurationManager.AppSettings["AuthorizeStatusCheckTime"];
            StatusCheckTime = TimeSpan.FromMinutes(!string.IsNullOrEmpty(value) ? Convert.ToInt32(value) : 90);
            value = ConfigurationManager.AppSettings["CompanyName"];
            CompanyName = !string.IsNullOrEmpty(value) ? value : "Store";
            value = ConfigurationManager.AppSettings["Sitename"];
            Sitename = !string.IsNullOrEmpty(value) ? value : "www.store.com";
            value = ConfigurationManager.AppSettings["IsSsl"];
            IsSsl = !string.IsNullOrEmpty(value) && Convert.ToBoolean(value);
            value = ConfigurationManager.AppSettings["IsStandartPortsSsl"];
            IsStandartPortsSsl = !string.IsNullOrEmpty(value) && Convert.ToBoolean(value);
            value = ConfigurationManager.AppSettings["Port"];
            Port = !string.IsNullOrEmpty(value) ? value : "80";
            value = ConfigurationManager.AppSettings["Sitename"];
            PortSsl = !string.IsNullOrEmpty(value) ? value : "443";
            value = ConfigurationManager.AppSettings["Sitename"];
            SiteAddress = !string.IsNullOrEmpty(value) ? value : string.Empty;
            value = ConfigurationManager.AppSettings["UniqueEmail"];
            UniqueEmail = !string.IsNullOrEmpty(value) && Convert.ToBoolean(value);
            value = ConfigurationManager.AppSettings["SiteEmail"];
            SiteEmail = !string.IsNullOrEmpty(value) ? value : string.Empty;
        }
    }
}
