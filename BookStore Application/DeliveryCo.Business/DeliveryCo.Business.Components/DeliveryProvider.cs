using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeliveryCo.Business.Components.Interfaces;
using System.Transactions;
using DeliveryCo.Business.Entities;
using System.Threading;
using DeliveryCo.Services.Interfaces;


namespace DeliveryCo.Business.Components
{
    public class DeliveryProvider : IDeliveryProvider
    {
        public List<DeliveryInfo> getAllDelivery()
        {
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                return lContainer.DeliveryInfo.Where((delivery) => delivery.Status != 2).ToList();
            }
        }

        public Guid SubmitDelivery(DeliveryCo.Business.Entities.DeliveryInfo pDeliveryInfo)
        {
            using(TransactionScope lScope = new TransactionScope())
            using(DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                pDeliveryInfo.DeliveryIdentifier = Guid.NewGuid();
                pDeliveryInfo.Status = 0;
                lContainer.DeliveryInfo.Add(pDeliveryInfo);
                lContainer.SaveChanges();
                ThreadPool.QueueUserWorkItem(new WaitCallback((pObj) => ScheduleDelivery(pDeliveryInfo)));
                lScope.Complete();
            }
            return pDeliveryInfo.DeliveryIdentifier;
        }

        private void ScheduleDelivery(DeliveryInfo pDeliveryInfo)
        {
            Console.WriteLine("Delivering to " + pDeliveryInfo.DestinationAddress);
            Thread.Sleep(5000);
            //notifying of delivery completion
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                var item = from d in lContainer.DeliveryInfo
                           where d.DeliveryIdentifier == pDeliveryInfo.DeliveryIdentifier
                           select d;

                var i = item.First();
                if (i != null && i.Status != 2)
                {
                    i.Status = 1;
                    IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(i.DeliveryNotificationAddress);
                    lService.NotifyDeliveryCompletion(i.DeliveryIdentifier, DeliveryInfoStatus.Delivered);
                    lContainer.SaveChanges();
                    lScope.Complete();
                }
            }

        }

        public Guid RefundDelivery(String pDeliveryInfo) 
        {
            Console.WriteLine("Order " + pDeliveryInfo+" is refund.");
            using (TransactionScope lScope = new TransactionScope())
            using (DeliveryCoEntityModelContainer lContainer = new DeliveryCoEntityModelContainer())
            {
                Guid value = new Guid(pDeliveryInfo);

                var item = from d in lContainer.DeliveryInfo
                           where d.DeliveryIdentifier == value
                           select d;

                var i = item.First();
                i.Status = 2;
                lContainer.SaveChanges();
                lScope.Complete();
                IDeliveryNotificationService lService = DeliveryNotificationServiceFactory.GetDeliveryNotificationService(i.DeliveryNotificationAddress);
                lService.NotifyDeliveryCompletion(i.DeliveryIdentifier, DeliveryInfoStatus.Failed);

                //do the bank account refund
                return i.DeliveryIdentifier;


            }
        }

    }
}
