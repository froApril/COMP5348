using DeliveryCo.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using BookStore.Business.Components;

namespace BookStore.Services
{
    public class DeliveryNotificationService : IDeliveryNotificationService
    {
        IDeliveryNotificationProvider Provider
        {
            get { return ServiceLocator.Current.GetInstance<IDeliveryNotificationProvider>(); }
        }

        public void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status)
        {
            Provider.NotifyDeliveryCompletion(pDeliveryId, GetDeliveryStatusFromDeliveryInfoStatus(status));
        }



        private DeliveryStatus GetDeliveryStatusFromDeliveryInfoStatus(DeliveryInfoStatus status)
        {
            if (status == DeliveryInfoStatus.Delivered)
            {
                return DeliveryStatus.Delivered;
            }
            else if (status == DeliveryInfoStatus.Failed)
            {
                return DeliveryStatus.Failed;
            }
            else if (status == DeliveryInfoStatus.Submitted)
            {
                return DeliveryStatus.Submitted;
            }
            else
            {
                throw new Exception("Unexpected delivery status received");
            }
        }

        public MessageTypes.Order RetrieveDeliveryOrder(Guid pDeliveryId)
        {
            DeliveryNotificationProvider dn = new DeliveryNotificationProvider();

            var pOrder = dn.RetrieveDeliveryOrder(pDeliveryId);
            MessageTypes.Order externalResult = MessageTypeConverter.Instance.Convert<
                BookStore.Business.Entities.Order,
                BookStore.Services.MessageTypes.Order>(pOrder);
            return externalResult;
        }
    }
}
