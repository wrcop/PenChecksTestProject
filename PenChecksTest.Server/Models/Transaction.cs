using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json.Serialization;

namespace PenChecksTest.Server.Models
{
    public class TransactionItem
    {
        public int? Id { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType Type { get; set; }
        public int? TransferAccount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    public class PendingTransaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public int TransferAccount { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
    public enum TransactionType
    {
        Default,
        Withdrawl,
        Deposit,
        TransferTo,
        TransferFrom
    }
}