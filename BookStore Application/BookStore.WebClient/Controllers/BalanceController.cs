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

            double balance = ExternalServiceFactory.Instance.TransferService.ShowBalance(user.Id);
            ViewBag.Message = balance;
       
            return View(ViewBag);
        }
    }
}