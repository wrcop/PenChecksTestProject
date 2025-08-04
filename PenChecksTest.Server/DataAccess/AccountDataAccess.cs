using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using PenChecksTest.Server.Models;


namespace PenChecksTest.Server.DataAccess
{
    public class AccountDataAccess
    {
        
        public static Customer RetrieveCustomerInfo(int customerId)
        {
            Customer cust = new Customer(customerId);
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                SqlParameter p = new SqlParameter("@CustomerId", customerId);
                SqlCommand cmd = db.CreateCommand();
                cmd.Parameters.Add(p);
                cmd.CommandText = "SELECT CUSTOMER.NAME,ACCOUNT.ID,ACCOUNT.TYPE FROM CUSTOMER INNER JOIN ACCOUNT ON CUSTOMER.ID = ACCOUNT.CUSTOMERID WHERE CUSTOMERID = @CustomerId";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cust.Name = reader.GetString(0);
                        AccountInfo ai = new AccountInfo();
                        ai.AccountId = reader.GetInt32(1);
                        ai.Type = reader.GetString(2);
                        cust.Accounts.Add(ai);
                    }
                }                
                db.Close();
            }
            return cust;
        }

        public static AccountInfo RetrieveAccountInfo(int accountId)
        {
            AccountInfo acct = new AccountInfo();
            acct.AccountId = accountId;
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                SqlParameter p = new SqlParameter("@accountId", accountId);
                SqlCommand cmd = db.CreateCommand();
                cmd.Parameters.Add(p);
                cmd.CommandText = "SELECT A.BALANCE, A.[TYPE] AS ACCOUNTTYPE, T.ID AS TRANSACTIONID, T.[TYPE] AS TRANSACTIONTYPE, T.TRANSFERACCOUNTID, T.AMOUNT, T.ENDINGBALANCE, T.[TIMESTAMP] FROM ACCOUNT A LEFT JOIN [TRANSACTION] T ON A.ID = T.ACCOUNTID WHERE A.ID = @accountid ORDER BY T.[TIMESTAMP] DESC";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        acct.Balance = (decimal)reader["BALANCE"];
                        acct.Type = (string)reader["ACCOUNTTYPE"];
                        TransactionItem t = new TransactionItem();
                        if (!reader.IsDBNull(2))
                        {
                            t.Id = (int)reader["TRANSACTIONID"];
                            switch ((int)reader["TRANSACTIONTYPE"])
                            {
                                case 1: t.Type = TransactionType.Withdrawl; break;
                                case 2: t.Type = TransactionType.Deposit; break;
                                case 3: t.Type = TransactionType.TransferTo; break;
                                case 4: t.Type = TransactionType.TransferFrom; break;
                            }
                            t.TransferAccount = reader["TRANSFERACCOUNTID"] as int?;
                            t.Amount = (decimal)reader["AMOUNT"];
                            t.Balance = (decimal)reader["ENDINGBALANCE"];
                            t.TimeStamp = (DateTime)reader["TIMESTAMP"];
                            acct.TransactionHistory.Add(t);
                        }
                    }
                }
                db.Close();
            }
            return acct;
        }
        public static bool ProcessTransaction(PendingTransaction transaction)
        {
            bool status = false;
            //get current balance of account from server
            transaction.Balance = getAccountBalance(transaction.Id); //use this to check for invalid transactions without sufficient balance to cover it

                switch(transaction.Type)
                {
                    case TransactionType.Withdrawl:
                        status = WithdrawFunds(transaction.Id, transaction.Amount);
                        break;
                    case TransactionType.Deposit:
                        status = DepositFunds(transaction.Id, transaction.Amount);
                        break;
                    case TransactionType.TransferTo:
                        status = WithdrawFunds(transaction.Id, transaction.Amount) && DepositFunds(transaction.TransferAccount, transaction.Amount);
                    break;
                    //transfer from cannot be commited by user, it is just for display in transaction log
                }
            if (status) {
                //update balance before saving log
                transaction.Balance = getAccountBalance(transaction.Id);
                //if transaction successfully updated balances insert log of transaction (ideally this would be lumped togethre in an actual SQL transaction with rollback
                //but due to time constraints it's being done in two parts which introduces some risk in reality)
                InsertTransactionLog(transaction);
            }
        return status;
        }

        public static bool DepositFunds(int accountId, decimal amount) {
            bool status = false;
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                SqlParameter acct = new SqlParameter("@accountId", accountId);
                SqlParameter amt = new SqlParameter("@amount", amount);
                SqlCommand cmd = db.CreateCommand();
                cmd.Parameters.Add(acct);
                cmd.Parameters.Add(amt);
                cmd.CommandText = "UPDATE ACCOUNT SET BALANCE = BALANCE + @amount WHERE ID = @accountId";
                status = cmd.ExecuteNonQuery() == 1; //will evaluate to true if number of rows updated equals 1, otherwise is false
                db.Close();
            }
            return status;
        }
        public static bool WithdrawFunds(int accountId, decimal amount)
        {
            bool status = false;
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                SqlParameter acct = new SqlParameter("@accountId", accountId);
                SqlParameter amt = new SqlParameter("@amount", amount);
                SqlCommand cmd = db.CreateCommand();
                cmd.Parameters.Add(acct);
                cmd.Parameters.Add(amt);
                cmd.CommandText = "UPDATE ACCOUNT SET BALANCE = BALANCE - @amount WHERE ID = @accountId";
                status = cmd.ExecuteNonQuery() == 1; //will evaluate to true if number of rows updated equals 1, otherwise is false
                db.Close();
            }
            return status;
        }
        public static bool InsertTransactionLog(PendingTransaction transaction)
        {
            bool status = false;
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                //setup parameters and query for insert
                SqlParameter acct = new SqlParameter("@accountId", transaction.Id);
                SqlParameter amt = new SqlParameter("@amount", transaction.Amount);
                SqlParameter balance = new SqlParameter("@newBalance", transaction.Balance);
                SqlParameter transactionType = new SqlParameter("@transactionType", transaction.Type);
                SqlParameter transferAcct = new SqlParameter("@transferAcct", transaction.TransferAccount == 0 ? DBNull.Value : transaction.TransferAccount);
                SqlCommand cmd = db.CreateCommand();
                cmd.Parameters.Add(acct);
                cmd.Parameters.Add(amt);
                cmd.Parameters.Add(balance);
                cmd.Parameters.Add(transactionType);
                cmd.Parameters.Add(transferAcct);
                cmd.CommandText = "INSERT INTO [TRANSACTION] (ACCOUNTID, [TYPE], TRANSFERACCOUNTID, AMOUNT, ENDINGBALANCE,TIMESTAMP) VALUES (@accountId, @transactionType, @transferAcct, @amount, @newBalance,CURRENT_TIMESTAMP)";
                status = cmd.ExecuteNonQuery() == 1; //will evaluate to true if number of rows updated equals 1, otherwise is false
                db.Close();

            }
            //if account transfer will need to add counter transfer item to destination account
            if (transaction.Type == TransactionType.TransferTo)
            {
                decimal secondaryAccountBalance = getAccountBalance(transaction.TransferAccount); //need to get secondary account balance since it's not included in original request
                PendingTransaction secondaryTransaction = new PendingTransaction();
                secondaryTransaction.Id = transaction.TransferAccount;
                secondaryTransaction.Amount = transaction.Amount;
                secondaryTransaction.Type = TransactionType.TransferFrom;
                secondaryTransaction.TransferAccount = transaction.Id;
                secondaryTransaction.Balance = secondaryAccountBalance;
                status = status && InsertTransactionLog(secondaryTransaction); //little bit of recursion to save time, should immediately fall out since transaction type is changed
            }

            return status;
        }

        public static decimal getAccountBalance(int accountId)
        {
            decimal acctBalance = 0;
            using (SqlConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LocalDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                db.Open();
                //first get current balance of account
                SqlParameter acct = new SqlParameter("@accountId", accountId);
                SqlCommand getNewBalance = db.CreateCommand();
                getNewBalance.CommandText = "SELECT BALANCE FROM ACCOUNT WHERE ID = @accountId";
                getNewBalance.Parameters.Add(acct);
                acctBalance = (decimal)getNewBalance.ExecuteScalar(); //sloppy casting but limited time here, would fix this later
                db.Close();
            }
            return acctBalance;
        }
    }   
}