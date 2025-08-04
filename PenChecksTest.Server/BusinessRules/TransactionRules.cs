using PenChecksTest.Server.Models;
using PenChecksTest.Server.DataAccess;
using PenChecksTest.Server.Business.Constants;

namespace PenChecksTest.Server.Business
{
    public class BusinessRules
    {
        public static bool ValidAmount(decimal amount)
        {
            //negative values are not valid
            if (amount <= 0) { return false; }
            //enforce a pre-set maximum amount for transfers
            if (amount > BusinessConstants.TransactionLimit) { return false; }
            //nothing smaller than .01 dollars is valid
            if(amount.Scale > 2) { return false; }
            //if amount is valid return true
            return true;
        }
        public static bool HasEnoughFunds(PendingTransaction request)
        {
            request.Balance = AccountDataAccess.getAccountBalance(request.Id); //get current balance to make sure it covers transaction
            if (request.Balance >= request.Amount) { return true; }
            else { return false; }
        }
    }
}