using ActionMailer.Net.Mvc;
using PCSMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCSMvc.Controllers
{
    public class EmailController : MailerBase
    {
        //
        // GET: /Email/

        public EmailResult SendEmail(EmailModel model)
        {
            To.Add(model.To);

            From = model.From;

            Subject = model.Subject;

            return Email("SendEmail", model);
        }

    }
}
