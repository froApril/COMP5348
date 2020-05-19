using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using BookStore.Services.MessageTypes;
using System.ServiceModel.Channels;

namespace DeliveryCo.Services.Interfaces
{
    public enum DeliveryInfoStatus { Submitted, Delivered, Failed }

    [ServiceContract]
    public interface IDeliveryNotificationService
    {
        [OperationContract]
        void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status);

        [OperationContract]

        Order RetrieveDeliveryOrder(Guid pDeliveryId);
    }
}
