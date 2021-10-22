using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

namespace Client
{

	static class KeyRepo
	{
		public static string publicKey = "santhosh";
		public static string privateKey = "sblw-3hn8-sqoy19";
		public static string symmetricKey = "";
	}
	class EncryptDecrypt
	{
		public string Encrypt(string message, string key)
		{
			byte[] rawPlaintext = System.Text.Encoding.Unicode.GetBytes(message);
			try
			{
				using (Aes aes = new AesManaged())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.KeySize = 128;          // in bits
					aes.Key = new byte[128 / 8];  // 16 bytes for 128 bit encryption
					aes.IV = new byte[128 / 8];   // AES needs a 16-byte IV
												  // Should set Key and IV here.  Good approach: derive them from 
												  // a password via Cryptography.Rfc2898DeriveBytes 
					byte[] cipherText = null;
					byte[] plainText = null;

					using (MemoryStream ms = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
						{
							cs.Write(rawPlaintext, 0, rawPlaintext.Length);
						}

						cipherText = ms.ToArray();
					}


					using (MemoryStream ms = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
						{
							cs.Write(cipherText, 0, cipherText.Length);
						}

						plainText = ms.ToArray();
					}
					string s = System.Text.Encoding.Unicode.GetString(plainText);
					return s;
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + "a", ex.InnerException);
			}
		}
		public string Encrypt(string message, string publicKey, string privateKey)
		{
			try
			{
				string textToEncrypt = message;
				string ToReturn = "";
				byte[] secretkeyByte = new byte[128 / 8];
				secretkeyByte = System.Text.Encoding.UTF8.GetBytes(privateKey);
				byte[] publickeybyte = new byte[128 / 8];
				publickeybyte = System.Text.Encoding.UTF8.GetBytes(publicKey);
				MemoryStream ms = null;
				CryptoStream cs = null;
				byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
				using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
				{
					ms = new MemoryStream();
					cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
					ms.Close();
					cs.Close();
					cs.Write(inputbyteArray, 0, inputbyteArray.Length);
					cs.FlushFinalBlock();
					ToReturn = Convert.ToBase64String(ms.ToArray());
				}
				return ToReturn;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + "f", ex.InnerException);
			}
		}
		public string Decrypt(string message, string key)
		{
			byte[] rawPlaintext = System.Text.Encoding.Unicode.GetBytes(message);
			try
			{
				using (Aes aes = new AesManaged())
				{
					aes.Padding = PaddingMode.PKCS7;
					aes.KeySize = 128;          // in bits
					aes.Key = System.Text.Encoding.UTF8.GetBytes(key);  // 16 bytes for 128 bit encryption
					aes.IV = new byte[128 / 8];   // AES needs a 16-byte IV
												  // Should set Key and IV here.  Good approach: derive them from 
												  // a password via Cryptography.Rfc2898DeriveBytes 
					byte[] cipherText = null;
					byte[] plainText = null;

					using (MemoryStream ms = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
						{
							cs.Write(rawPlaintext, 0, rawPlaintext.Length);
						}

						cipherText = ms.ToArray();
					}


					using (MemoryStream ms = new MemoryStream())
					{
						using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
						{
							cs.Write(cipherText, 0, cipherText.Length);
						}

						plainText = ms.ToArray();
					}
					string s = System.Text.Encoding.Unicode.GetString(plainText);
					return s;
				}
			}
			catch (Exception ae)
			{
				throw new Exception(ae.Message + "g", ae.InnerException);
			}
		}
		public string Decrypt(string message, string publickey, string privatekey)
		{
			try
			{
				string textToDecrypt = message;
				string ToReturn = "";
				byte[] privatekeyByte = new byte [128/8];
				privatekeyByte = System.Text.Encoding.UTF8.GetBytes(privatekey);
				byte[] publickeybyte = new byte [128/8];
				publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
				MemoryStream ms = null;
				CryptoStream cs = null;
				byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
				inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
				using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
				{
					ms = new MemoryStream();
					cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
					cs.Write(inputbyteArray, 0, inputbyteArray.Length);
					cs.FlushFinalBlock();
					Encoding encoding = Encoding.UTF8;
					ToReturn = encoding.GetString(ms.ToArray());
				}
				return ToReturn;
			}
			catch (Exception ae)
			{
				throw new Exception(ae.Message + "h", ae.InnerException);
			}
		}
	}

	public class Read
	{
		EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
		public void ReadMessage(object obj)
		{
			TcpClient tcpClient = (TcpClient)obj;
			StreamReader streamReader = new StreamReader(tcpClient.GetStream());

			while (true)
			{
				try
				{
					//read incoming message 
					string message = streamReader.ReadLine();
					string decryptedMessage = encryptDecrypt.Decrypt(message, KeyRepo.symmetricKey);
					Console.WriteLine("Incoming message : " + decryptedMessage);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					break;
				}
			}
		}
	}
	class Program
	{
		static void Main(string[] args)
		{
			Read read = new Read();
			EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
			try
			{
				//this computer  
				TcpClient tcpClient = new TcpClient("127.0.0.1", 5000);
				Console.WriteLine("Connected to server.");

				Thread thread = new Thread(read.ReadMessage);
				thread.Start(tcpClient);

				StreamWriter streamWriter = new StreamWriter(tcpClient.GetStream());
				StreamReader streamReader = new StreamReader(tcpClient.GetStream());

				string message = KeyRepo.publicKey;
				string encryptedMessage = encryptDecrypt.Encrypt(message, AppDomain.CurrentDomain.BaseDirectory + "//server-public-key.txt");
				streamWriter.WriteLine(encryptedMessage);
				Console.WriteLine("Client public key sent");

				//message = streamReader.ReadLine();
				//string decryptedMessage = encryptDecrypt.Decrypt(message, AppDomain.CurrentDomain.BaseDirectory, KeyRepo.privateKey);
				KeyRepo.symmetricKey = "engineer";
				Console.WriteLine("SymmetricKey : " + KeyRepo.symmetricKey);

				Console.WriteLine("What's your name?");
				while (true)
				{
					if (tcpClient.Connected)
					{
						//send message
						string name = Console.ReadLine();
						string input = System.IO.File.ReadAllText("message.txt") + name;
						encryptedMessage = encryptDecrypt.Encrypt(input, KeyRepo.symmetricKey);
						streamWriter.WriteLine(encryptedMessage);
						streamWriter.Flush();
					}
				}
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
			Console.ReadKey();
		}
	}
}