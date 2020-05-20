using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using BookStore.Business.Components.Interfaces;
using System.Messaging;

namespace BookStore.Business.Components
{
    public class EmailProvider : IEmailProvider
    {
        public void SendMessage(EmailMessage pMessage)
        {
            global::EmailService.MessageTypes.EmailMessage newmsg = new global::EmailService.MessageTypes.EmailMessage()
            {
                Message = pMessage.Message,
                ToAddresses = pMessage.ToAddress,
                Date = DateTime.Now
            };
            // Remove this dependency, send through the queue instead
            //ExternalServiceFactory.Instance.EmailService.SendEmail(newmsg);

            System.Messaging.Message msg = new System.Messaging.Message();
            msg.Body = newmsg;
            MessageQueue queue = new MessageQueue(".\\Private$\\bookstoremessage");
            queue.Send(msg);
            Console.Out.WriteLine(newmsg.Date + ": Sent email message to the queue");
        }
    }
}
