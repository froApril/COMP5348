using BookStore.Business.Components;
using BookStore.Services;
using BookStore.Services.MessageTypes;
using BookStore.WebClient.ClientModels;
using DeliveryCo.MessageTypes;
using DeliveryCo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.WebClient.Controllers
{
    public class DeliveryController : Controller
    {
        // GET: Delivery
        public ActionResult Index()
        {
            // See if the service is available, else handle
            List<DeliveryInfo> deliveryList = null;
            try {
                deliveryList = ExternalServiceFactory.Instance.DeliveryService.getAllDelivery();
            }
            catch
            {
                deliveryList = new List<DeliveryInfo>();
            }
            DeliveryList list = new DeliveryList();
            list.deliveryList = deliveryList;

            return View(list);
        }

        public RedirectToRouteResult RefundDelivery(String delivery)
        {
            // See if the service is available, else handle
            try
            {
                Guid orderNumber = ExternalServiceFactory.Instance.DeliveryService.RefundDelivery(delivery);
                ServiceFactory.Instance.OrderService.RefundOrder(orderNumber);
            }
            catch
            {
                RedirectToAction("ErrorPage");
            }


            return RedirectToAction("Index");
        }
        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}