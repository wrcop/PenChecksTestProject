namespace PenChecksTest.Server.Models
{
    public class CustomerInfoRequest
    {
        public int CustomerId { get; set; }
    }
    public class AccountInfoRequest
    {
        public int AccountId { get; set; }
    }
    public class TransactionRequest : PendingTransaction
    {

    }
    public class RequestStatus
    {
        public bool Error { get; set; }
        public string? ErrorInformation { get; set; }
        public TransactionItem? Transaction { get; set; }
    }

}