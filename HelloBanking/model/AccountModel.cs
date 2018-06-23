using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Google.Protobuf.WellKnownTypes;
using HelloBanking.entity;
using HelloBanking.error;
using MySql.Data.MySqlClient;

namespace HelloBanking.model
{
    public class AccountModel
    {
        public Boolean Save(Account account)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "insert into account " +
                                 "(accountNumber, username, password, balance, identityCard, fullName, " +
                                 "email, phoneNumber, address, dob, gender, salt) value" +
                                 "(@accountNumebr, @username, @password, @balance, @identityCard, @fullName, @email," +
                                 "@phoneNumber, @address, @dob, @gender, @salt) ";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", account.AccountNumber);
            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@password", account.Password);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@identityCard", account.IdentityCard);
            cmd.Parameters.AddWithValue("@fullName", account.Fullname);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@phoneNumber", account.PhoneNumber);
            cmd.Parameters.AddWithValue("@address", account.Address);
            cmd.Parameters.AddWithValue("@dob", account.Dob);
            cmd.Parameters.AddWithValue("@gender", account.Gender);
            cmd.Parameters.AddWithValue("@salt", account.Salt);
            DbConnection.Instance().CloseConnection();
            return true;
        }

        public Boolean CheckExistUsername(string username)
        {
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from account where username = @username";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            DbConnection.Instance().CloseConnection();
            return isExist;
        }

        public Account GetByUsername(string username)
        {
            Account account = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from account where username = @username";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (isExist)
            {
                account = new Account
                {
                    AccountNumber = reader.GetString("accountNumber"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Salt = reader.GetString("salt"),
                    Fullname = reader.GetString("fullName"),
                    Balance = reader.GetDecimal("balance")
                };
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }

        public Account GetByAccountNumber(string AccountNumber)
        {
            Account account = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from account where accountNumber = @accountNumber and status = 1";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", AccountNumber);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (isExist)
            {
                account = new Account
                {
                    AccountNumber = reader.GetString("accountNumber"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Salt = reader.GetString("salt"),
                    Fullname = reader.GetString("fullName"),
                    Balance = reader.GetDecimal("balance")
                };
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }
        
        public bool UpdateBalance(Account account, Transaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.

            try
            {
                /**
                 * 1. Lấy thông tin số dư mới nhất của tài khoản.
                 * 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw.
                 *     2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.                 
                 * 3. Update số dư vào tài khoản.
                 *     3.1. Tính toán lại số tiền trong tài khoản.
                 *     3.2. Update số tiền vào database.
                 * 4. Lưu thông tin transaction vào bảng transaction.
                 */

                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `account` where username = @username and status = @status";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (historyTransaction.Type != (Transaction.TransactionType) Transaction.TransactionType.DEPOSIT
                    && historyTransaction.Type != (Transaction.TransactionType) Transaction.TransactionType.WITHDRAW)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (historyTransaction.Type == (Transaction.TransactionType) Transaction.TransactionType.WITHDRAW &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (historyTransaction.Type == (Transaction.TransactionType) Transaction.TransactionType.DEPOSIT)
                {
                    currentBalance += historyTransaction.Amount;
                }
                else
                {
                    currentBalance -= historyTransaction.Amount;
                }

                // 3.2. Update số dư vào database.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `account` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        public bool TransferBalance(string senderAccountNumber,string receiAccountNumber, Transaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); //Đảm bảo đã kết nối đến database thành công
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); //Khởi tạo transaction
            try
            {
               //1.Kiểm tra số dư tài khoản
                var queryBalance = "select balance from account where accountNumber = @accountNumber and status = 1";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@accountNumber", senderAccountNumber);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                //Không tìm thấy tài khoản thì throw lỗi
                if (!balanceReader.Read())
                {
                    throw  new SpringHeroTransactionException("Invalid username");
                }
                //đảm bảo sẽ có bản ghi
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                if ( historyTransaction.Type != Transaction.TransactionType.TRANSFER)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }
                //Update số dư vào tài khoản
                if (historyTransaction.Type == Transaction.TransactionType.TRANSFER &&
                    currentBalance > historyTransaction.Amount)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                var updatesenderAcountNumber = 0;
                //Lấy thông tin tài khỏan một lần nữa.Đảm bảo thông tin mới nhất
                var updateSender  = "select account from set balance - @balance where accountNumber = @senderAccountNumber";
                MySqlCommand querysenderBalanceCommand = new MySqlCommand(updateSender, DbConnection.Instance().Connection);
                querysenderBalanceCommand.Parameters.AddWithValue("@senderAccountNumber",senderAccountNumber);
                querysenderBalanceCommand.Parameters.AddWithValue("@balance", historyTransaction.Amount);
                updatesenderAcountNumber = querysenderBalanceCommand.ExecuteNonQuery();

                var updatereceiAccountNumber = 0;
                var updateRecei = "select account from set balance + @balance where accountNumber = @receiAccountNumber";
                MySqlCommand queryReceiBalanceCommand = new MySqlCommand(updateRecei, DbConnection.Instance().Connection);
                queryReceiBalanceCommand.Parameters.AddWithValue("receiAccountNumber", receiAccountNumber);
                queryReceiBalanceCommand.Parameters.AddWithValue("balance", historyTransaction.Amount);
                updatereceiAccountNumber = queryReceiBalanceCommand.ExecuteNonQuery();
                
                //Lưu thông transaction vào bảng transaction
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updatesenderAcountNumber == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }
            DbConnection.Instance().CloseConnection();
            return false;    
        }

        public List<Transaction> TransactionHistory()
        {
            List<Transaction> listTransaction = new List<Transaction>();
            DbConnection.Instance().OpenConnection(); //Đảm bảo đã kết nối đến database thành công
            var transaction = DbConnection.Instance().Connection.BeginTransaction();
            var listTransactionHistory = 0;
            var queryTransactionHistory = "select * from transaction where accountNumber = @accountNumber and status = 1";
            MySqlCommand queryReceiBalanceCommand = new MySqlCommand(queryTransactionHistory, DbConnection.Instance().Connection);
            var reader = queryReceiBalanceCommand.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString("id");
                var amount = reader.GetDecimal("amount");
                var content = reader.GetString("content");
                var senderAccountNumber = reader.GetString("senderAccountNumber");
                var receiAccountNumber = reader.GetString("receiAccountNumber");
                var type = reader.GetInt32("type");
                Transaction.TransactionType typeTransaction = Transaction.TransactionType.DEPOSIT;
                if (type == 1)
                {
                    typeTransaction = Transaction.TransactionType.DEPOSIT;
                }else if (type == 2)
                {
                    typeTransaction = Transaction.TransactionType.WITHDRAW;
                }else if (type == 3)
                {
                    typeTransaction = Transaction.TransactionType.TRANSFER;
                }

                var createdAt = reader.GetString("createdAt");

                var status = reader.GetInt32("status");
                Transaction.ActiveStatus statusTransaction = Transaction.ActiveStatus.REJECT;
                if (status == 0)
                {
                    statusTransaction = Transaction.ActiveStatus.REJECT;
                }else if (status == 1)
                {
                    statusTransaction = Transaction.ActiveStatus.PROCESSING;
                }else if (status == 2)
                {
                    statusTransaction = Transaction.ActiveStatus.DONE;
                }else if (status == -1)
                {
                    statusTransaction = Transaction.ActiveStatus.DELETED;
                }

                Transaction tran = new Transaction(id, amount, content, senderAccountNumber, receiAccountNumber,
                    typeTransaction, createdAt, statusTransaction);
                listTransaction.Add(tran);
            }

            return listTransaction;
        }
    }
}