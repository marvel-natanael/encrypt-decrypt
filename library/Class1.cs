using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Library
{
	public class SymmetricUtils
	{
		//generates 32 byte data for symmetric key
		public string makeSymmetricKey(int size)
		{
			try
			{
				char[] chars =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
				byte[] data = new byte[4 * size];
				using (var crypto = RandomNumberGenerator.Create())
				{
					crypto.GetBytes(data);
				}
				StringBuilder result = new StringBuilder(size);
				for (int i = 0; i < size; i++)
				{
					var rnd = BitConverter.ToUInt32(data, i * 4);
					var idx = rnd % chars.Length;

					result.Append(chars[idx]);
				}

				string s = result.ToString();
				return s;
			}
			catch (Exception ae)
			{
				throw new Exception(ae.Message, ae.InnerException);
			}
		}
		//symmetric encryption
		public string encyrptSymmetric(string key, string plainText)
		{
			try
			{
				byte[] iv = new byte[16];
				byte[] array;

				using (Aes aes = Aes.Create())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.Key = Encoding.UTF8.GetBytes(key);
					aes.IV = iv;

					ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

					using (MemoryStream memoryStream = new MemoryStream())
					{
						using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
						{
							using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
							{
								streamWriter.Write(plainText);
							}
							array = memoryStream.ToArray();
						}
					}
				}
				return Convert.ToBase64String(array);
			}
			catch(Exception ae)
			{
				throw new Exception(ae.Message, ae.InnerException);
			}
		}
		//symmetric decryption
		public string decryptSymmetric(string key, string message)
		{
			try
			{
				byte[] iv = new byte[16];
				byte[] buffer = Convert.FromBase64String(message);

				using (Aes aes = Aes.Create())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.Key = Encoding.UTF8.GetBytes(key);
					aes.IV = iv;
					ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

					using (MemoryStream memoryStream = new MemoryStream(buffer))
					{
						using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
						{
							using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
							{
								return streamReader.ReadToEnd();
							}
						}
					}
				}
			}
			catch (Exception ae)
			{
				throw new Exception(ae.Message, ae.InnerException);
			}
		}
	}
	public class AssymetricUtils
	{
		RSACryptoServiceProvider rsa = null;
		private string PrivateKeyXML;
		private string PublicKeyXML;
		//get keys
		public string getPrivateKey()
		{
			return PrivateKeyXML;
		}
		public string getPublicKey()
		{
			return PublicKeyXML;
		}
		//generates RSAkey for private and public key
		public void assignNewKey()
		{
			const int PROVIDER_RSA_FULL = 1;
			const string CONTAINER_NAME = "KeyContainer";
			CspParameters cspParams;
			cspParams = new CspParameters(PROVIDER_RSA_FULL);
			cspParams.KeyContainerName = CONTAINER_NAME;
			cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
			cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
			rsa = new RSACryptoServiceProvider(cspParams);

			PrivateKeyXML = rsa.ToXmlString(true);

			PublicKeyXML = rsa.ToXmlString(false);

		}
		//assymetric encryption
		public string encrypt(string strText, string strPublicKey)
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			rsa.FromXmlString(strPublicKey);

			byte[] byteText = Encoding.UTF8.GetBytes(strText);
			byte[] byteEntry = rsa.Encrypt(byteText, false);

			return Convert.ToBase64String(byteEntry);
		}
		//assymetric decryption
		public string decrypt(string strEntryText, string strPrivateKey)
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			rsa.FromXmlString(strPrivateKey);

			byte[] byteEntry = Convert.FromBase64String(strEntryText);
			byte[] byteText = rsa.Decrypt(byteEntry, false);

			return Encoding.UTF8.GetString(byteText);
		}
	}

}
