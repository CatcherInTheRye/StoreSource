using System;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using StoreLib.Modules.Helpers;

namespace StoreLib.Modules.Performance
{
    public class CompressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;
            HttpResponseBase response = filterContext.HttpContext.Response;

            if (response.StatusCode == 302)
            {
                return;
            }

            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (acceptEncoding.IsNullOrEmpty())
            {
                return;
            }

            acceptEncoding = acceptEncoding.ToUpperInvariant();
            if (acceptEncoding.Contains("GZIP"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            }
        }
    }
}
