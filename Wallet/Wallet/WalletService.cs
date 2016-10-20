using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Wallet
{
    [ServiceContract]
    public interface WalletService
    {

        [OperationContract]
        WalletModel Registration(string name, string surname, string username, string password, int age);

        [OperationContract]
        WalletModel Login(string username, string password);

        [OperationContract]
        WalletModel GetBalance(int clientId, string token);

        [OperationContract]
        WalletModel Withdraw(int clientId, decimal amount, string token);

        [OperationContract]
        WalletModel Deposit(int clientId, decimal amount, string token);

        [OperationContract]
        WalletModel Disactivate(int clientId, string token, int adminId);
    }
}
