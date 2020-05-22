using BookStore.Business.Components;
using BookStore.Services.MessageTypes;
using BookStore.WebClient.ClientModels;
using DeliveryCo.MessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace BookStore.WebClient.Controllers
{
    public class BalanceController : Controller
    {
        // GET: Balance
        public ActionResult Index(String UserId)
        {
            User user = ServiceFactory.Instance.UserService.ReadUserById(int.Parse(UserId));

            // The transfer service may not be available, show an error message if it is not
            double balance = -1;
            try { balance = ExternalServiceFactory.Instance.TransferService.ShowBalance(user.Id); }
            catch {}
            if (balance == -1) ViewBag.Message = "not currently available as the Bank Service is down.";
            else ViewBag.Message = "$" + balance;
       
            return View(ViewBag);
        }
    }
}