namespace PenChecksTest.Server.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AccountInfo> Accounts { get; set; }
        public Customer(int id)
        {
            Id = id;
            Name = "default";
            Accounts = new List<AccountInfo>();
        }
    }
}