using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Services.MessageTypes
{
    public class Delivery : MessageType
    {
        public int Id { get; set; }
        private int Status { get; set; }
        public string DestinationAddress { get; set; }
        public System.Guid ExternalDeliveryIdentifier { get; set; }
        public string SourceAddress { get; set; }

        public virtual Order Order { get; set; }
    }
}
