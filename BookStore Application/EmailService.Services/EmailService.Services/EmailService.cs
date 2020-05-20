using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailService.Services.Interfaces;
using EmailService.Business.Components.Interfaces;
using EmailService.MessageTypes;
using Microsoft.Practices.ServiceLocation;
using System.Timers;
using System.Net.NetworkInformation;
using System.Messaging;

namespace EmailService.Services
{
    public class EmailService : IEmailService
    {
        public IEmailProvider EmailProvider
        {
            get
            {
                return ServiceFactory.GetService<IEmailProvider>();
            }
        }

        private MessageQueue queue = new MessageQueue(".\\Private$\\bookstoremessage"); // This is the queue from which we extract messages

        public void startTimer(int interval) // in milliseconds
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += TimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        public void TimedEvent(Object source, ElapsedEventArgs e)
        {
            // On every timer fire, try to extract a message from the queue
            //Console.Out.WriteLine("heartbeat");
            EmailMessage email = new EmailMessage();
            Object o = new Object();
            System.Type[] arrTypes = new System.Type[2];
            arrTypes[0] = email.GetType();
            arrTypes[1] = o.GetType();
            queue.Formatter = new XmlMessageFormatter(arrTypes);
            email = ((EmailMessage)queue.Receive().Body);
            Console.Out.WriteLine(email.Date + ": Retrieved message from the queue and sent to " + email.ToAddresses);
            Console.Out.WriteLine("To: " + email.ToAddresses + " From: " + email.FromAddresses + "\n" + email.Message);
        }

        public void SendEmail(EmailMessage pMessage)
        {
            EmailProvider.SendEmail(
                    MessageTypeConverter.Instance.Convert<
                    global::EmailService.MessageTypes.EmailMessage,
                    global::EmailService.Business.Entities.EmailMessage>(pMessage)
                );
        }
    }
}
