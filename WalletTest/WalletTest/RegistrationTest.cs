using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wallet;

namespace WalletTest
{
    [TestClass]
    public class RegistrationTest
    {
        /// <summary>
        /// Method to test a successful registration.
        /// </summary>
        [TestMethod]
        public void PassTest()
        {
            WalletModel wm = new WalletModel();

            wm = WalletController.Registration("John", "Smith", "jsmith4", "Test12345", 28);
            Assert.AreEqual(1, wm.ResponseCode, 1, wm.ErrorText);

        }

        /// <summary>
        /// Method to test a failed registration due to a non unique username.
        /// </summary>
        [TestMethod]
        public void FailTestNotUnique()
        {
            WalletModel wm = new WalletModel();

            wm = WalletController.Registration("John", "Smith", "jsmith4", "Test12345", 28);
            Assert.AreEqual(1, wm.ResponseCode, 1, wm.ErrorText);

        }

        /// <summary>
        /// Method to test a failed registration due to the underage check.
        /// </summary>
        [TestMethod]
        public void FailTestUnderAge()
        {
            WalletModel wm = new WalletModel();

            wm = WalletController.Registration("John", "Smith", "jsmith5", "Test12345", 15);
            Assert.AreEqual(1, wm.ResponseCode, 1, wm.ErrorText);

        }
    }
}
