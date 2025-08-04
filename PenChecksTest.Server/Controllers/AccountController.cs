using Microsoft.AspNetCore.Mvc;
using PenChecksTest.Server.Models;
using PenChecksTest.Server.DataAccess;
using PenChecksTest.Server.Business;
using System.Transactions;

namespace PenChecksTest.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("GetCustomerInfo")]
        public Customer GetCustomerInfo([FromBody] CustomerInfoRequest request) { 
            int customerId = request.CustomerId;
            return AccountDataAccess.RetrieveCustomerInfo(customerId);
        }

        [HttpPost]
        [Route("GetAccountBalance")]
        public AccountInfo GetAccountBalance([FromBody] AccountInfoRequest request)
        {
            int accountId = request.AccountId;

            //get account data from localdb
            return AccountDataAccess.RetrieveAccountInfo(accountId);

        }

        [HttpPost]
        [Route("ProcessTransaction")]
        public RequestStatus DepositMoneyInAccount([FromBody] TransactionRequest request)
        {
            PendingTransaction p = request;
            RequestStatus status = new RequestStatus();

            if (request.Type == TransactionType.Default)
            {
                status.Error = true;
                status.ErrorInformation += "Please select a valid transaction type. ";
            }

            if (!BusinessRules.ValidAmount(p.Amount))
            {
                status.Error = true;
                status.ErrorInformation += "An Invalid Amount was entered, please try again with a valid amount. ";
            }
            if (request.Type == TransactionType.Withdrawl || request.Type == TransactionType.TransferTo)
            {
                if (!BusinessRules.HasEnoughFunds(request))
                {
                    status.Error = true;
                    status.ErrorInformation += "Not enough funds for purchase! Please deposit funds or select an amount less than or equal to available balance. ";
                }
            }

            if (!status.Error)
            {
                bool processStatus = processStatus = AccountDataAccess.ProcessTransaction(p);
                if (!processStatus)
                {
                    status.Error = true;
                    status.ErrorInformation += "Error saving transaction, please contact support. ";
                }
            }            
            return status;
        }
    }
}
