using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using HelloBanking.entity;
using HelloBanking.model;
using HelloBanking.view;

namespace HelloBanking
{
    class Program
    {
        public static Account currentLoggedInAccount;
        private static Transaction _transaction;
        private static AccountModel _model;
        private static Account _account;
        public static Account currentLoggedIn;

        static void Main(string[] args)
        {
            ApplicationView view = new ApplicationView();
            while (true)
            {
                if (Program.currentLoggedInAccount != null)
                {
                    view.GenerateCustomerMenu();

                }
                else
                {
                    view.GeneraDefaultMenu();
                }

            }

            
        }
    }
}