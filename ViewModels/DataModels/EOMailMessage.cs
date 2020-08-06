using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{
    /// <summary>
    ///    message.From = new MailAddress("FromMailAddress");    
    ///    message.To.Add(new MailAddress("ToMailAddress"));
    ///    message.Subject = "Test";
    ///    message.IsBodyHtml = true; //to make message body as html  
    ///    message.Body = htmlString;
    /// </summary>
    public class EOMailMessage
    {
        public EOMailMessage()
        {

        }

        public EOMailMessage(string fromAddress, string toAddress, string subject, string mailHtml, string pwd)
        {
            MailMessage = new MailMessage();
            MailMessage.From = new MailAddress(fromAddress);
            MailMessage.To.Add(new MailAddress(toAddress));
            MailMessage.Subject = subject;
            MailMessage.IsBodyHtml = true;
            MailMessage.Body = mailHtml;
            Pwd = pwd;
        }

        public MailMessage MailMessage { get; set; }

        public String Pwd { get; set; }
    }
}
