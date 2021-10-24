using Microsoft.VisualStudio.TestTools.UnitTesting;
using Library;
using System;
using System.Text;

namespace libraryTest
{
	[TestClass]
	public class UnitTest1
	{
		SymmetricUtils symmetricUtils = new SymmetricUtils();
		AssymetricUtils assymetricUtils = new AssymetricUtils();
		[TestMethod]
		public void testSymmetricDecryption()
		{
			//check if data is decrypted
			string dummyValue = "water";
			string dummyKey = symmetricUtils.makeSymmetricKey(32);
			string encryptedMessage = symmetricUtils.encyrptSymmetric(dummyKey, dummyValue);
			Assert.IsTrue(dummyValue == symmetricUtils.decryptSymmetric(dummyKey, encryptedMessage));
		}

		[TestMethod]
		public void testSymmetricEncryption()
		{
			//check if data is changed	
			string dummyValue = "water";
			string dummyKey = symmetricUtils.makeSymmetricKey(32);
			Assert.IsTrue(dummyValue != symmetricUtils.encyrptSymmetric(dummyKey, dummyValue));
		}

		[TestMethod]
		public void testAssymetricEncryption()
		{
			//check if data is changed	
			assymetricUtils.assignNewKey();
			string dummyValue = "water";
			string encryptedMessage = assymetricUtils.encrypt(dummyValue, assymetricUtils.getPublicKey());
			Assert.IsTrue(dummyValue != encryptedMessage);
		}

		[TestMethod]
		public void testAssymetricDecryption()
		{
			//check id data is decrypted
			assymetricUtils.assignNewKey();
			string dummyValue = "water";
			string encryptedMessage = assymetricUtils.encrypt(dummyValue, assymetricUtils.getPublicKey());
			Assert.IsTrue(dummyValue == assymetricUtils.decrypt(encryptedMessage, assymetricUtils.getPrivateKey()));
		}

        [TestMethod]
        public void testKeyGeneration()
        {
            //check if data length is right
            int size = 32;
            string dummyKey = symmetricUtils.makeSymmetricKey(size);
            Assert.IsTrue(size == dummyKey.Length);
        }
	}
}
