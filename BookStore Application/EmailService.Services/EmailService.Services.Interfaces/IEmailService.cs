using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using EmailService.MessageTypes;
using System.Timers;

namespace EmailService.Services.Interfaces
{
    [ServiceContract]
    public interface IEmailService
    {
        [OperationContract]
        void SendEmail(EmailMessage pMessage);
        void startTimer(int interval); // in milliseconds
        void TimedEvent(Object source, ElapsedEventArgs e);
    }
}
