using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.SqlClient;

namespace Wallet
{
    public class Service1 : WalletService
    {
        /// <summary>
        /// User Story 1 - Registration and Login
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        public WalletModel Registration(string name, string surname, string username, string password, int age)
        {
            WalletModel wm = WalletController.Registration(name, surname, username, password, age);

            if (wm.ResponseCode > 0)
            {
                wm = WalletController.Login(username, password, wm);
            }
            return wm;
        }

        public WalletModel Login(string username, string password)
        {
            return WalletController.Login(username, password, new WalletModel());
        }

        /// <summary>
        /// User Story 2 - Get user balance
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public WalletModel GetBalance(int clientId, string token)
        {
            WalletModel wm = new WalletModel();
            wm.ClientId = clientId;
            wm.LoginToken = token;
            return WalletController.GetBalance(wm);
        }

        /// <summary>
        /// User Story 3 - Withdraw from client balance
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="amount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public WalletModel Withdraw(int clientId, decimal amount, string token)
        {
            WalletModel wm = new WalletModel();
            wm.ClientId = clientId;
            wm.LoginToken = token;
            wm.Amount = amount;
            return WalletController.Transaction(wm, true);
        }

        /// <summary>
        /// User Story 4 - Deposit to client balance
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="amount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public WalletModel Deposit(int clientId, decimal amount, string token)
        {
            WalletModel wm = new WalletModel();
            wm.ClientId = clientId;
            wm.LoginToken = token;
            wm.Amount = amount;
            return WalletController.Transaction(wm, false);
        }

        /// <summary>
        /// User Story 5 - Disactivate client account
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="token"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public WalletModel Disactivate(int clientId, string token, int adminId)
        {
            WalletModel wm = new WalletModel();
            wm.ClientId = clientId;
            wm.LoginToken = token;
            wm.AdminId = adminId;
            return WalletController.Disactivate(wm);
        }

    }
}
