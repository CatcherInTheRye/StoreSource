using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using StoreLib.Modules.Application;

namespace StoreLib.Modules.Route
{
    public class RequireSslFilter : ActionFilterAttribute
    {
        public string IsRequired;

        public overrIde void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ApplicationHelper.IsSsl)
            {
                bool isRequer = (!String.IsNullOrEmpty(IsRequired)) ? Convert.ToBoolean(IsRequired) : true;
                //StringBuilder sb = new StringBuilder(filterContext.HttpContext.Request.Url.ToString());
                UriBuilder builder = new UriBuilder(filterContext.HttpContext.Request.Url);
                bool changed = false;
                if (isRequer && builder.Scheme != Uri.UriSchemeHttps)
                //filterContext.HttpContext.Request.Url.ToString().ToLower().Contains("https")
                {
                    //sb.Replace("http", "https");
                    //if (sb.ToString().IndexOf(Consts.SiteAddress + ":" + Consts.Port) > 0)
                    //  sb.Replace(Consts.SiteAddress + ":" + Consts.Port, Consts.SiteAddress + ":" + Consts.PortSsl);
                    //else
                    //  sb.Replace(Consts.SiteAddress, Consts.SiteAddress + ":" + Consts.PortSsl);
                    builder.Scheme = Uri.UriSchemeHttps;
                    builder.Port = Convert.ToInt32(ApplicationHelper.PortSsl);
                    changed = true;
                }
                else if (!isRequer && builder.Scheme != Uri.UriSchemeHttp)
                // !filterContext.HttpContext.Request.Url.ToString().ToLower().Contains("http:")
                {
                    //sb.Replace("https", "http");
                    //if (sb.ToString().IndexOf(Consts.SiteAddress + ":" + Consts.PortSsl) > 0)
                    //  sb.Replace(Consts.SiteAddress + ":" + Consts.PortSsl, Consts.SiteAddress + ":" + Consts.Port);
                    //else
                    //  sb.Replace(Consts.SiteAddress, Consts.SiteAddress + ":" + Consts.Port);
                    builder.Scheme = Uri.UriSchemeHttp;
                    builder.Port = Convert.ToInt32(ApplicationHelper.Port);
                    changed = true;
                }
                if (changed)
                {
                    //filterContext.Result = new RedirectResult(sb.ToString());          
                    filterContext.Result = new RedirectResult(builder.Uri.ToString());
                    filterContext.Result.ExecuteResult(filterContext);
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// ProvIdes helper extensions for turning strings into fully-qualified and SSL-enabled Urls.
    /// </summary>
    public static class UrlStringExtensions
    {
        /// <summary>
        /// Takes a relative or absolute url and returns the fully-qualified url path.
        /// </summary>
        /// <param name="text">The url to make fully-qualified. Ex: Home/About</param>
        /// <returns>The absolute url plus protocol, server, & port. Ex: http://localhost:1234/Home/About</returns>
        public static string ToFullyQualifiedUrl(this string text)
        {
            //### the VirtualPathUtility doesn"t handle querystrings, so we break the original url up
            var oldUrl = text;
            var oldUrlArray = (oldUrl.Contains("?") ? oldUrl.Split('?') : new[] { oldUrl, "" });

            //### we"ll use the Request.Url object to recreate the current server request"s base url
            //### requestUri.AbsoluteUri = "http://domain.com:1234/Home/Index"
            //### requestUri.LocalPath = "/Home/Index"
            //### subtract the two and you get "http://domain.com:1234", which is urlBase
            var requestUri = HttpContext.Current.Request.Url;
            //### fix for Mike Hadlow's reported issue regarding extraneous link elements when a querystring is present
            //var urlBase = requestUri.AbsoluteUri.Substring( 0, requestUri.AbsoluteUri.Length - requestUri.LocalPath.Length );

            var localPathAndQuery = requestUri.LocalPath + requestUri.Query;
            string urlBase = String.Empty;
            string[] str = localPathAndQuery.Split('/');
            if (str.Length > 3)
            {
                int index = requestUri.AbsoluteUri.IndexOf(String.Format("{0}/{1}/{2}", str[0], str[1], str[2]));
                urlBase = requestUri.AbsoluteUri.Substring(0, index);
            }
            else
                urlBase = requestUri.AbsoluteUri.Substring(0, requestUri.AbsoluteUri.Length - localPathAndQuery.Length);

            //### convert the request url into an absolute path, then reappend the querystring, if one was specified
            var newUrl = VirtualPathUtility.ToAbsolute(oldUrlArray[0]);
            if (!string.IsNullOrEmpty(oldUrlArray[1]))
                newUrl += "?" + oldUrlArray[1];

            //### combine the old url base (protocol + server + port) with the new local path
            return urlBase + newUrl;
        }

        /// <summary>
        /// Looks for Html links in the passed string and turns each relative or absolute url and returns the fully-qualified url path.
        /// </summary>
        /// <param name="text">The url to make fully-qualified. Ex: <a href="Home/About">Blah</a></param>
        /// <returns>The absolute url plus protocol, server, & port. Ex: <a href="http://localhost:1234/Home/About">Blah</a></returns>
        public static string ToFullyQualifiedLink(this string text)
        {
            var regex = new Regex(
                "(?<Before><a.*href=\")(?!http)(?<Url>.*?)(?<After>\".+>)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            return regex.Replace(text, (Match m) =>
                                       m.Groups["Before"].Value +
                                       ToFullyQualifiedUrl(m.Groups["Url"].Value) +
                                       m.Groups["After"].Value
                );
        }

        /// <summary>
        /// Takes a relative or absolute url and returns the fully-qualified url path using the Https protocol.
        /// </summary>
        /// <param name="text">The url to make fully-qualified. Ex: Home/About</param>
        /// <returns>The absolute url plus server, & port using the Https protocol. Ex: https://localhost:1234/Home/About</returns>
        public static string ToSslUrl(this string text)
        {
            if (!ApplicationHelper.IsSsl) return text;
            StringBuilder sb = new StringBuilder(ToFullyQualifiedUrl(text));
            if (sb.ToString().IndexOf("http:") > 0)
            {
                sb.Replace("http:", "https:");
                if (!ApplicationHelper.IsStandartPortsSsl)
                    sb.Replace(
                        (sb.ToString().IndexOf(ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port) > 0)
                            ? ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port
                            : ApplicationHelper.SiteAddress,
                        ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl);
            }
            return sb.ToString();
        }

        public static string ToSslLink(this string text)
        {
            if (!ApplicationHelper.IsSsl) return text;
            StringBuilder sb = new StringBuilder(ToFullyQualifiedLink(text));
            if (sb.ToString().IndexOf("http:") > 0)
            {
                sb.Replace("http:", "https:");
                if (!ApplicationHelper.IsStandartPortsSsl)
                    sb.Replace(
                        (sb.ToString().IndexOf(ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port) > 0)
                            ? ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port
                            : ApplicationHelper.SiteAddress,
                        ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl);
            }
            return sb.ToString();
        }

        public static string ToNonSslLink(this string text)
        {
            if (!ApplicationHelper.IsSsl) return text;
            StringBuilder sb = new StringBuilder(ToFullyQualifiedLink(text));
            if (sb.ToString().IndexOf("https:") > 0)
            {
                sb.Replace("https:", "http:");
                if (!ApplicationHelper.IsStandartPortsSsl)
                    sb.Replace(
                        (sb.ToString().IndexOf(ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl) > 0)
                            ? ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl
                            : ApplicationHelper.SiteAddress,
                        ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port);
            }
            return sb.ToString();
        }

        public static MvcHtmlString ToSslLink(this MvcHtmlString text)
        {
            if (!ApplicationHelper.IsSsl) return text;
            StringBuilder sb = new StringBuilder(ToFullyQualifiedLink(text.ToString()));
            if (sb.ToString().IndexOf("http:") > 0)
            {
                sb.Replace("http:", "https:");
                if (!ApplicationHelper.IsStandartPortsSsl)
                    sb.Replace(
                        (sb.ToString().IndexOf(ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port) > 0)
                            ? ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port
                            : ApplicationHelper.SiteAddress,
                        ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl);
            }
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString ToNonSslLink(this MvcHtmlString text)
        {
            if (!ApplicationHelper.IsSsl) return text;
            StringBuilder sb = new StringBuilder(ToFullyQualifiedLink(text.ToString()));
            if (sb.ToString().IndexOf("https:") > 0)
            {
                sb.Replace("https:", "http:");
                if (!ApplicationHelper.IsStandartPortsSsl)
                    sb.Replace(
                        (sb.ToString().IndexOf(ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl) > 0)
                            ? ApplicationHelper.SiteAddress + ":" + ApplicationHelper.PortSsl
                            : ApplicationHelper.SiteAddress,
                        ApplicationHelper.SiteAddress + ":" + ApplicationHelper.Port);
            }
            return new MvcHtmlString(sb.ToString());
        }
    }
}
