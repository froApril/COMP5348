using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStore.Business.Components.Interfaces;
using BookStore.Business.Entities;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using DeliveryCo.MessageTypes;

namespace BookStore.Business.Components
{
    public class OrderProvider : IOrderProvider
    {
        public IEmailProvider EmailProvider
        {
            get { return ServiceLocator.Current.GetInstance<IEmailProvider>(); }
        }

        public IUserProvider UserProvider
        {
            get { return ServiceLocator.Current.GetInstance<IUserProvider>(); }
        }

        public void SubmitOrder(Entities.Order pOrder)
        {      
            using (TransactionScope lScope = new TransactionScope())
            {
                //LoadBookStocks(pOrder);
                //MarkAppropriateUnchangedAssociations(pOrder);

                using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
                {
                    try
                    {
                        // First ping each service to ensure they respond, may cause money or books to disappear otherwise
                        Console.Out.WriteLine("(" + DateTime.Now + ") Attempting connection to Delivery service.");
                        ExternalServiceFactory.Instance.DeliveryService.Ping();
                    }
                    catch
                    {
                        Console.Out.WriteLine("(" + DateTime.Now + ") Connection to Delivery service failed. It may be unavailable.");
                        throw new Exception("The Delivery service may be unavailable.");
                    }
                    Console.Out.WriteLine("(" + DateTime.Now + ") Connection to Delivery service succeeded.");
                    try
                    {
                        // First ping each service to ensure they respond, may cause money or books to disappear otherwise
                        Console.Out.WriteLine("(" + DateTime.Now + ") Attempting connection to Bank service.");
                        ExternalServiceFactory.Instance.TransferService.Ping();
                    }
                    catch
                    {
                        Console.Out.WriteLine("(" + DateTime.Now + ") Connection to Bank service failed. It may be unavailable.");
                        throw new Exception("The Bank service may be unavailable.");
                    }
                    Console.Out.WriteLine("(" + DateTime.Now + ") Connection to Bank service succeeded.");
                    try
                    {
                        pOrder.OrderNumber = Guid.NewGuid();
                        pOrder.Store = "OnLine";

                        // Book objects in pOrder are missing the link to their Stock tuple (and the Stock GUID field)
                        // so fix up the 'books' in the order with well-formed 'books' with 1:1 links to Stock tuples
                        foreach (OrderItem lOrderItem in pOrder.OrderItems)
                        {
                            int bookId = lOrderItem.Book.Id;
                            lOrderItem.Book = lContainer.Books.Where(book => bookId == book.Id).First();
                            System.Guid stockId = lOrderItem.Book.Stock.Id;
                            lOrderItem.Book.Stock = lContainer.Stocks.Where(stock => stockId == stock.Id).First();
                        }
                        // and update the stock levels
                        pOrder.UpdateStockLevels();

                        // add the modified Order tree to the Container (in Changed state)
                        lContainer.Orders.Add(pOrder);

                        // ask the Bank service to transfer fundss
                        TransferFundsFromCustomer(UserProvider.ReadUserById(pOrder.Customer.Id).BankAccountNumber, pOrder.Total ?? 0.0);

                        // ask the delivery service to organise delivery
                        PlaceDeliveryForOrder(pOrder);

                        // and save the order
                        lContainer.SaveChanges();
                        lScope.Complete();                    
                    }
                    catch (Exception lException)
                    {
                        SendOrderErrorMessage(pOrder, lException);
                        IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> entries =  lContainer.ChangeTracker.Entries();
                        throw;
                    }
                }
            }
            SendOrderPlacedConfirmation(pOrder);
        }

        //private void MarkAppropriateUnchangedAssociations(Order pOrder)
        //{
        //    pOrder.Customer.MarkAsUnchanged();
        //    pOrder.Customer.LoginCredential.MarkAsUnchanged();
        //    foreach (OrderItem lOrder in pOrder.OrderItems)
        //    {
        //        lOrder.Book.Stock.MarkAsUnchanged();
        //        lOrder.Book.MarkAsUnchanged();
        //    }
        //}

        private void LoadBookStocks(Order pOrder)
        {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                foreach (OrderItem lOrderItem in pOrder.OrderItems)
                {
                    lOrderItem.Book.Stock = lContainer.Stocks.Where((pStock) => pStock.Book.Id == lOrderItem.Book.Id).FirstOrDefault();    
                }
            }
        }

        private void SendOrderErrorMessage(Order pOrder, Exception pException)
        {
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = pOrder.Customer.Email,
                Message = "There was an error in processsing your order " + pOrder.OrderNumber + ": "+ pException.Message + ". Please contact Book Store"
            });
        }

        private void SendOrderPlacedConfirmation(Order pOrder)
        {
            EmailProvider.SendMessage(new EmailMessage()
            {
                ToAddress = pOrder.Customer.Email,
                Message = "Your order " + pOrder.OrderNumber + " has been placed"
            });
        }

        private void PlaceDeliveryForOrder(Order pOrder)
        {
            try
            {
                Console.Out.WriteLine("(" + DateTime.Now + ") Attempting to deliver from " + pOrder.Customer.Address);
                Delivery lDelivery = new Delivery() { DeliveryStatus = DeliveryStatus.Submitted, SourceAddress = "Book Store Address", DestinationAddress = pOrder.Customer.Address, Order = pOrder };

                Guid lDeliveryIdentifier = ExternalServiceFactory.Instance.DeliveryService.SubmitDelivery(new DeliveryInfo()
                {
                    OrderNumber = lDelivery.Order.OrderNumber.ToString(),
                    SourceAddress = lDelivery.SourceAddress,
                    DestinationAddress = lDelivery.DestinationAddress,
                    DeliveryNotificationAddress = "net.tcp://localhost:9010/DeliveryNotificationService"
                });

                lDelivery.ExternalDeliveryIdentifier = lDeliveryIdentifier;
                pOrder.Delivery = lDelivery;
            }
            catch
            {
                Console.Out.WriteLine("(" + DateTime.Now + ") Delivery from " + pOrder.Customer.Address + " failed. Delivery service may be unavailable.");
                throw new Exception("Error connecting to delivery service.");
            } 
        }

        private void TransferFundsFromCustomer(int pCustomerAccountNumber, double pTotal)
        {
            try
            {
                Console.Out.WriteLine("(" + DateTime.Now + ") Requesting transfer of $" + pTotal + " from customer " + pCustomerAccountNumber);
                ExternalServiceFactory.Instance.TransferService.Transfer(pTotal, pCustomerAccountNumber, RetrieveBookStoreAccountNumber());
            }
            catch
            {
                Console.Out.WriteLine("(" + DateTime.Now + ") Request failed. Error when transferring funds for order. Bank Services may be unavailable");
                throw new Exception("Error when transferring funds for order.");
            }
        }


        private int RetrieveBookStoreAccountNumber()
        {
            return 123;
        }

        public void RefundOrder(Guid ordernumber) {
            using (BookStoreEntityModelContainer lContainer = new BookStoreEntityModelContainer())
            {
                try
                {
                    Delivery lDelivery = lContainer.Deliveries.Include("Order.Customer").Where((pDel) => pDel.ExternalDeliveryIdentifier == ordernumber).FirstOrDefault();
                    double total = (double)lDelivery.Order.Total;
                    ICollection<OrderItem> orderItems = lDelivery.Order.OrderItems;
                    int accountNumber = lDelivery.Order.Customer.BankAccountNumber;
                    ExternalServiceFactory.Instance.TransferService.Transfer(total, 123, accountNumber);
                    foreach (OrderItem lItem in orderItems)
                    {
                        lItem.Book.Stock.Quantity += lItem.Quantity;

                    }
                    lContainer.SaveChanges();
                }
                catch
                {
                    Console.Out.WriteLine("(" + DateTime.Now + ") Request failed. Error when refunding order. Bank Services may be unavailable");
                    throw new Exception("Error when refunding order.");
                }
            }
        }

        


    }
}
