using System;
using System.Collections.Generic;
using HelloBanking.entity;
using HelloBanking.model;
using HelloBanking.utility;
using MySqlX.XDevAPI;
using static HelloBanking.Program;

namespace HelloBanking.controller
{
    public class AccountController
    {
        private AccountModel model = new AccountModel();

        public bool Register()
        {
            Account account = GetAccountInformation();
            Dictionary<string, string> errors = account.CheckValidate();
            if (errors.Count > 0)
            {
                Console.WriteLine("Please fix errors below and try agian");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                return false;
            }
            else
            {
                account.EncryptPassword();
                model.Save(account);
                return true;
            }
        }

        public bool Login()
        {
            Console.WriteLine("--------------------LOGIN INFORMATION-------------------------");
            Console.WriteLine("Username : ");
            var username = Console.ReadLine();
            Console.WriteLine("Password : ");
            var password = Console.ReadLine();
            Account existingAccount = model.GetByUsername(username);
            if (existingAccount == null)
            {
                return false;
            }

            if (!existingAccount.CheckEncryPassword(password))
            {
                return false;
            }

            currentLoggedInAccount = existingAccount;
            return true;
        }

        public void Deposit()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to deposit: ");
            var amount = Utility.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.DEPOSIT,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedIn, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedIn = model.GetByUsername(Program.currentLoggedIn.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedIn.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }

        public void Transfer()
        {
            Console.WriteLine("Transfer.");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Please enter your account number");
            var receiverAccountNumber = Console.ReadLine();
            Console.WriteLine("Please enter the amount to be transferred");
            var amount = Utility.GetDecimalNumber();
            Console.WriteLine("Please enter message content");
            var content = Console.ReadLine();
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.TRANSFER,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.DONE
            };
            if (model.TransferBalance(Program.currentLoggedInAccount.AccountNumber, receiverAccountNumber,
                historyTransaction))
            {
                Console.WriteLine("Transaction Success!");
            }
            else
            {
                Console.WriteLine("Transaction fail,Please try agian");
            }

            Program.currentLoggedInAccount = model.GetByAccountNumber(Program.currentLoggedInAccount.AccountNumber);
            Console.WriteLine("Current balance : " + Program.currentLoggedInAccount.Balance);

        }

        public void TransactionHistory()
        {
            List<Transaction> listTransactions = model.TransactionHistory();
            foreach (var t in listTransactions)
            {
                Console.WriteLine("Id : " + t.Id + " - " + "Amount : " + t.Amount + " - " + "Content : " + t.Content +
                                  " - " + "SenderAccountNumber : " + t.SenderAccountNumber + " - " +
                                  "ReceiAccountNumber : " + t.ReceiverAccountNumber + " - " + 
                                  "Type : " + t.Type + " - " + "Created : " + t.CreatedAt + " - " +
                                  "Status : " + t.Status + " - ");
            }
        }

        public bool CheckAccountNumber()
        {
            Console.WriteLine("-------------------Check Account Number--------------------");
            Console.WriteLine("Nhập Account Number cần chuyển : ");
            var accountNumber = Console.ReadLine();
            Account existAccountNumber = model.GetByAccountNumber(accountNumber);
            if (existAccountNumber == null)
            {
                return false;
            }

            currentLoggedInAccount = existAccountNumber;
            return true;
        }

        private Account GetAccountInformation()
        {
            Console.WriteLine("----------------------REGISTER INFORMATION---------------------");
            Console.WriteLine("Username : ");
            var username = Console.ReadLine();
            Console.WriteLine("Password : ");
            var password = Console.ReadLine();
            Console.WriteLine("Confirm Password : ");
            var cpassword = Console.ReadLine();
            Console.WriteLine("Balance : ");
            var balance = Utility.GetDecimalNumber();
            Console.WriteLine("Identity Card : ");
            var identityCard = Console.ReadLine();
            Console.WriteLine("Full Name : ");
            var fullName = Console.ReadLine();
            Console.WriteLine("Birthday : ");
            var birthday = Console.ReadLine();
            Console.WriteLine("Gender : (1 | 2 | 3) ");
            var gender = Utility.GetInt32Number();
            Console.WriteLine("Email : ");
            var email = Console.ReadLine();
            Console.WriteLine("Phone Number : ");
            var phoneNumber = Console.ReadLine();
            Console.WriteLine("Address : ");
            var address = Console.ReadLine();
            var account = new Account
            {
                Username = username,
                Password = password,
                Cpassword = cpassword,
                IdentityCard = identityCard,
                Gender = gender,
                Balance = balance,
                Address = address,
                Dob = birthday,
                Fullname = fullName,
                Email = email,
                PhoneNumber = phoneNumber
            };
            return account;
        }

        
    }
}