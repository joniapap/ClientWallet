using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Web;

namespace Wallet
{
    public class WalletController
    {

        private static string connectionString = "Data Source=JON-PC;Initial Catalog=Backoffice;Integrated Security=True";

        /// <summary>
        /// Method to register new clients.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        public static WalletModel Registration(string name, string surname, string username, string password, int age)
        {
            WalletModel wm = new WalletModel();
            wm.ResponseCode = 0;
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(surname) || String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password) || age == 0)
            {
                wm.ErrorText = "Kindly fill in all the values.";
                return wm;
            }
            if (password.Length < 8)
            {
                wm.ErrorText = "The password does not contain enough characters.";
                return wm;
            }
            if (age < 18)
            {
                wm.ErrorText = "You are under the legal age to play.";
                return wm;
            }

            SqlConnection conn = null;
            SqlCommand insertClient = null;
            SqlCommand insertBalance = null;
            int clientId = 0;
            int effectedRows = 0;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                try
                {
                    insertClient = new SqlCommand("INSERT INTO CLIENT (Name, Surname, Username, Password, Age) VALUES (@Name, @Surname, @Username, @Password, @Age); SELECT CLientId FROM CLIENT WHERE ClientId = SCOPE_IDENTITY()", conn);//
                    insertClient.Parameters.Add(new SqlParameter("Name", name));
                    insertClient.Parameters.Add(new SqlParameter("Surname", surname));
                    insertClient.Parameters.Add(new SqlParameter("Username", username));
                    insertClient.Parameters.Add(new SqlParameter("Password", CalculateMD5Hash(password)));
                    insertClient.Parameters.Add(new SqlParameter("Age", age));
                    clientId = (int)insertClient.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    wm.ResponseCode = 0;
                    wm.ErrorText = "The username already exists. Kindly select another username.";
                }

                if (clientId > 0)
                {
                    insertBalance = new SqlCommand("INSERT INTO BALANCE (ClientId, WalletId, Amount) VALUES (@ClientId, 1, 0)", conn);
                    insertBalance.Parameters.Add(new SqlParameter("ClientId", clientId));
                    effectedRows = insertBalance.ExecuteNonQuery();

                    if (effectedRows > 0)
                    {
                        wm.ResponseCode = 1;
                        wm.ClientId = clientId;
                    }
                    else
                    {
                        wm.ResponseCode = 0;
                        wm.ErrorText = "An error occurred while trying to create the account. Kindly retry";
                    }
                }
            }
            catch (Exception ex)
            {
                wm.ResponseCode = 0;
                wm.ErrorText = "System Error.";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return wm;
        }

        /// <summary>
        /// Method to login an already registered client.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="wm"></param>
        /// <returns></returns>
        internal static WalletModel Login(string username, string password, WalletModel wm)
        {
            SqlConnection conn = null;
            SqlCommand verifyLogin = null;
            SqlCommand insertLogin = null;
            int effectedRows = 0;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                verifyLogin = new SqlCommand("SELECT ClientId, Password, IsActive FROM Client WHERE Username = @Username", conn);
                verifyLogin.Parameters.Add(new SqlParameter("Username", username));
                verifyLogin.Parameters.Add(new SqlParameter("Password", CalculateMD5Hash(password)));

                using (SqlDataReader reader = verifyLogin.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.GetString(1).Equals(CalculateMD5Hash(password)))
                        {
                            wm.ResponseCode = 0;
                            wm.ErrorText = "You entered the wrong password.";
                            break;
                        }
                        if (reader.GetByte(2) == 0)
                        {
                            wm.ResponseCode = 0;
                            wm.ErrorText = "This account is not active.";
                        }
                        wm.ClientId = reader.GetInt32(0);
                    }
                }

                if (wm.ClientId > 0)
                {
                    byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                    byte[] key = Guid.NewGuid().ToByteArray();
                    string token = Convert.ToBase64String(time.Concat(key).ToArray());

                    insertLogin = new SqlCommand("DELETE FROM Login WHERE ClientId = @ClientId; INSERT INTO Login (ClientId, Token) VALUES (@ClientId, @Token)", conn);
                    insertLogin.Parameters.Add(new SqlParameter("ClientId", wm.ClientId));
                    insertLogin.Parameters.Add(new SqlParameter("Token", token));
                    effectedRows = (int)insertLogin.ExecuteNonQuery();

                    if (effectedRows > 0)
                    {
                        wm.ResponseCode = 1;
                        wm.LoginToken = token;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    wm.ResponseCode = 0;
                    wm.ErrorText = "The requested account does not exist.";
                }
            }
            catch (Exception ex)
            {
                wm.ResponseCode = 0;
                wm.ErrorText = "System Error.";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return wm;
        }

        /// <summary>
        /// Method to get the balance of a client.
        /// </summary>
        /// <param name="wm"></param>
        /// <returns></returns>
        internal static WalletModel GetBalance(WalletModel wm)
        {
            SqlConnection conn = null;
            SqlCommand getBalance = null;
            decimal balance = 0;

            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                getBalance = new SqlCommand(@"SELECT ISNULL((SELECT Amount FROM Balance b
                    INNER JOIN Login l ON b.ClientId = l.ClientId 
                    WHERE b.ClientId = @ClientId AND b.WalletId = 1 AND l.Token = @Token), -1) AS CurrBalance", conn);
                getBalance.Parameters.Add(new SqlParameter("ClientId", wm.ClientId));
                getBalance.Parameters.Add(new SqlParameter("Token", wm.LoginToken));

                using (SqlDataReader reader = getBalance.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balance = reader.GetDecimal(0);
                    }
                }

                if (balance >= 0)
                {
                    wm.ResponseCode = 1;
                    wm.Balance = balance;
                }
                else
                {
                    wm.ResponseCode = 0;
                    wm.ErrorText = "The balance is not available.";
                }
            }
            catch (Exception ex)
            {
                wm.ResponseCode = 0;
                wm.ErrorText = "System Error.";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return wm;
        }

        /// <summary>
        /// Method to deposit or withdraw from a client's balance.
        /// </summary>
        /// <param name="wm"></param>
        /// <param name="isWithraw"></param>
        /// <returns></returns>
        internal static WalletModel Transaction(WalletModel wm, bool isWithraw)
        {
            SqlConnection conn = null;
            SqlCommand setBalance = null;
            decimal newBalance = 0;


            try
            {
                wm = GetBalance(wm);
                if (isWithraw)
                {
                    newBalance = wm.Balance - wm.Amount;
                } else {
                    newBalance = wm.Balance + wm.Amount;
                }

                if (newBalance >= 0)
                {
                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    setBalance = new SqlCommand(@"UPDATE Balance SET Amount = @Amount OUTPUT INSERTED.Amount WHERE ClientId = @ClientId AND WalletId = 1", conn);
                    setBalance.Parameters.Add(new SqlParameter("Amount", newBalance));
                    setBalance.Parameters.Add(new SqlParameter("ClientId", wm.ClientId));
                    newBalance = (decimal)setBalance.ExecuteScalar();

                    wm.ResponseCode = 1;
                    wm.Balance = newBalance;
                }
                else
                {
                    wm.ResponseCode = 0;
                    wm.ErrorText = "The amount requested is greater than the available balance.";
                }
            }
            catch (Exception ex)
            {
                wm.ResponseCode = 0;
                wm.ErrorText = "System Error.";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return wm;
        }

        /// <summary>
        /// Method to disactivate a client account.
        /// </summary>
        /// <param name="wm"></param>
        /// <returns></returns>
        internal static WalletModel Disactivate(WalletModel wm)
        {
            SqlConnection conn = null;
            SqlCommand setBalance = null;
            SqlCommand getLogin = null;
            bool isLoggedIn = false;
            bool isSuccessful = false;


            try
            {
                conn = new SqlConnection(connectionString);
                conn.Open();

                getLogin = new SqlCommand(@"SELECT COUNT(*) FROM LOGIN WHERE ClientId = @ClientId AND Token = @Token", conn);
                getLogin.Parameters.Add(new SqlParameter("ClientId", wm.AdminId));
                getLogin.Parameters.Add(new SqlParameter("Token", wm.LoginToken));
                isLoggedIn = ((int)getLogin.ExecuteScalar() ==1) ? true : false;

                if (isLoggedIn)
                {
                    wm = GetBalance(wm);
                    if (wm.Balance == 0)
                    {
                        setBalance = new SqlCommand(@"UPDATE Client SET IsActive = 0 WHERE ClientId = @ClientId", conn);
                        setBalance.Parameters.Add(new SqlParameter("ClientId", wm.ClientId));
                        isSuccessful = ((int)setBalance.ExecuteNonQuery() == 1) ? true : false;

                        if (isSuccessful)
                        {
                            wm.ResponseCode = 1;
                            wm.ErrorText = "The account was deactivated successfully.";
                        }
                        else
                        {
                            wm.ResponseCode = 0;
                            wm.ErrorText = "The account was already deactivated.";
                        }
                    }
                    else
                    {
                        wm.ResponseCode = 0;
                        wm.ErrorText = "The account was not deactivated because the client still has balance.";
                    }
                }
                else
                {
                    wm.ResponseCode = 0;
                    wm.ErrorText = "No login was found. Kindly login before requesting this operation.";
                }
            }
            catch (Exception ex)
            {
                wm.ResponseCode = 0;
                wm.ErrorText = "System Error.";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return wm;
        }

        /// <summary>
        /// Method to generate a hash string from the supplied password.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

    }
}