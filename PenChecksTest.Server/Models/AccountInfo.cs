namespace PenChecksTest.Server.Models
{
    public class AccountInfo
    {
        public int AccountId { get; set; } //account Id from database
        public string CustomerName { get; set; } //customer name, flavor text, not strictly necessary here
        public decimal Balance { get; set; } //account balance
        public string Type { get; set; } //flavor text, checking/savings etc, not strictly necessary here
        public List<TransactionItem> TransactionHistory { get; set; } //list of previous transactions for account
        public AccountInfo() {
            TransactionHistory = new List<TransactionItem>();
        }
    }

}