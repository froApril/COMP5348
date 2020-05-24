using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bank.Services.Interfaces;
using Bank.Business.Components.Interfaces;
using System.ServiceModel;
using Microsoft.Practices.ServiceLocation;

namespace Bank.Services
{
    public class TransferService : ITransferService
    {
        private ITransferProvider TransferProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransferProvider>(); }
        }

        [OperationBehavior(TransactionScopeRequired=true)]
        public void Transfer(double pAmount, int pFromAcctNumber, int pToAcctNumber)
        {
            Console.Out.WriteLine("(" + DateTime.Now + ") Transferring $" + pAmount + " from account " + pFromAcctNumber + " to account " + pToAcctNumber);
            TransferProvider.Transfer(pAmount, pFromAcctNumber, pToAcctNumber);
        }
        public double ShowBalance(int AccountNumber)
        {
            Console.Out.WriteLine("(" + DateTime.Now + ") Showing balance for account " + AccountNumber);
            return TransferProvider.ShowBalance(AccountNumber);
        }
    }
}
