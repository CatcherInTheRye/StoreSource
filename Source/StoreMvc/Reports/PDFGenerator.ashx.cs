using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace PCS.Reports
{
    /// <summary>
    /// Summary description for PDFGenerator
    /// </summary>
    public class PDFGenerator : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            string json = new StreamReader(context.Request.InputStream).ReadToEnd();
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}