using BookStore.Services.MessageTypes;
using DeliveryCo.MessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Services.Interfaces
{
    [ServiceContract]
    public interface IDeliveryService
    {
        [OperationContract]
        List<DeliveryInfo> getAllDelivery();

        
    }
}
