using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wallet
{
    public class WalletModel
    {
        private int responseCode;
        private string errorText;
        private decimal balance;
        private string loginToken;
        private int clientId;
        private decimal amount;
        private int adminId;

        public int ResponseCode
        {
            get { return responseCode; }
            set { responseCode = value; }
        }

        public string ErrorText
        {
            get { return errorText; }
            set { errorText = value; }
        }

        public decimal Balance
        {
            get { return balance; }
            set { balance = value; }
        }

        public decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public string LoginToken
        {
            get { return loginToken; }
            set { loginToken = value; }
        }

        public int ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

        public int AdminId
        {
            get { return adminId; }
            set { adminId = value; }
        }
    }
}