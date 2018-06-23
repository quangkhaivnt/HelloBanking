using System;
using HelloBanking.controller;
using HelloBanking.utility;

namespace HelloBanking.view
{
    public class ApplicationView
    {
        private readonly AccountController controller = new AccountController();
        //Hiển thị menu chính của chương trình
        public void GeneraDefaultMenu()
        {
            while (true)
            {
                Console.WriteLine("--------------------WELCOME TO BANKING--------------------");
                Console.WriteLine("1.Register for free");
                Console.WriteLine("2.Login");
                Console.WriteLine("3.Exit");
                Console.WriteLine("Please enter you choice (1 | 2 | 3)");
                var choice = Utility.GetInt32Number();
                switch (choice)
                {
                        case 1:
                            Console.WriteLine(controller.Register());
                            Console.WriteLine("Register success");
                            Console.WriteLine("Register fails. Please try agian later.");
                            Console.WriteLine("Press enter to continue.");
                            Console.ReadLine();
                            break;
                        case 2:
                            Console.WriteLine(controller.Login());
                            Console.WriteLine("Login success! Welcome back " + Program.currentLoggedInAccount.Fullname +  "!");
                            Console.WriteLine("Login fails.Please try again later.");
                            Console.WriteLine("Press enter to continue");
                            Console.ReadLine();
                            break;
                        case 3:
                            Console.WriteLine("See you later.");
                            Environment.Exit(1);
                            break;
                }

                if (Program.currentLoggedInAccount != null)
                {
                    break;
                }
            }
        }

        public void GenerateCustomerMenu()
        {
            while (true)
            {
                Console.WriteLine("------------------------BANKING MENU--------------------------");
                Console.WriteLine("Welcome back" + Program.currentLoggedInAccount.Fullname);
                Console.WriteLine("1.Check information.");
                Console.WriteLine("2.Withdraw");
                Console.WriteLine("3.Deposit");
                Console.WriteLine("4.Transfer");
                Console.WriteLine("5.Transaction history");
                Console.WriteLine("6.Loguot");
                Console.WriteLine("Please enter you choice (1 | 2 | 3 | 4 | 5 | 6) : ");
                var choice = Utility.GetInt32Number();
                switch (choice)
                {
                        case 1:
                            controller.CheckAccountNumber();
                            Console.WriteLine("Xác minh thông tin thành công " + "Full Name : " + Program.currentLoggedInAccount.Fullname);
                            Console.WriteLine("Press enter to continue");
                            Console.ReadLine();
                            break;
                        case 2:
                            Console.WriteLine("Press enter to continue");
                            Console.ReadLine();
                            break;
                        case 3:
                            controller.Deposit();
                            break;
                        case 4:
                            controller.Transfer();
                            Console.WriteLine("Xác mình tài khoản thành công." + "Full Name : " + Program.currentLoggedInAccount.Fullname);
                            Console.WriteLine("Xác minh tài khoản không thành công.Thử lại");                   
                            break;
                        case 5:
                            controller.TransactionHistory();
                            break;
                        case 6:
                            Console.WriteLine("See you later.");
                            Environment.Exit(1);
                            break;
                }

                if (Program.currentLoggedInAccount == null)
                {
                    break;
                }
            }
        }
    }
}