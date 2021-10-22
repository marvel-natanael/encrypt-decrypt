using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
	static class KeyRepo
	{
		public static string publicKey = System.IO.File.ReadAllText("server-public-key.txt");
		public static string privateKey = System.IO.File.ReadAllText("server-private-key.txt");
		public static string symmetricKey = "engineer";
		public static string clientPublicKey = "";
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
	class HandleClient
	{
		Program program = new Program();
		EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
		public void Announce(object obj)
		{
			TcpClient tcpClient = (TcpClient)obj;
			StreamReader reader = new StreamReader(tcpClient.GetStream());

		}

		public void ClientListener(object obj)
		{
			TcpClient tcpClient = (TcpClient)obj;
			StreamReader reader = new StreamReader(tcpClient.GetStream());

			string key = reader.ReadLine();
			Console.WriteLine("X" + key);
			//key = encryptDecrypt.Decrypt(key, KeyRepo.privateKey);
			KeyRepo.clientPublicKey = key;
			Console.WriteLine("Client public key : " + KeyRepo.clientPublicKey);

			string symmetricKey = KeyRepo.symmetricKey;
			string encryptedMessage = encryptDecrypt.Encrypt(symmetricKey, KeyRepo.clientPublicKey, KeyRepo.privateKey);
			BroadCast(encryptedMessage, tcpClient, null);
			Console.WriteLine("Symmetric key sent");

			string name = reader.ReadLine();

			while (true)
			{
				try
				{
					//record chat  
					string message = reader.ReadLine();
					//string decryptedMessage = encryptDecrypt.Decrypt(message, KeyRepo.symmetricKey);
					Console.WriteLine("Decrypted message : " + message);
					encryptedMessage = encryptDecrypt.Encrypt(message, KeyRepo.symmetricKey);
					BroadCast(encryptedMessage, tcpClient, name);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					program.removeClient(tcpClient);
					break;
				}
			}
		}

		public void BroadCast(string msg, TcpClient excludeClient, string n)
		{
			foreach (TcpClient client in Program.tcpClientsList)
			{
				//broadcast to all client except sender
				StreamWriter sWriter = new StreamWriter(client.GetStream());
				string message = msg;
				if (client != excludeClient)
				{
					message = n + " : " + msg;
				}
				//string encryptedMessage = encryptDecrypt.Encrypt(message);
				sWriter.WriteLine(message);
				sWriter.Flush();
			}
		}
	}
	class Program
	{
		public static TcpListener tcpListener;
		public static List<TcpClient> tcpClientsList = new List<TcpClient>();
		private List<string> messagesToSave = new List<string>();

		static void Main(string[] args)
		{
			HandleClient handleClient = new HandleClient();

			//start process
			tcpListener = new TcpListener(IPAddress.Any, 5000);
			tcpListener.Start();
			Console.WriteLine("Server created");

			Aes aes = Aes.Create();
			aes.GenerateIV();
			aes.GenerateKey();

			while (true)
			{
				//add clients to list
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				tcpClientsList.Add(tcpClient);

				//broadcast from server
				Thread bc = new Thread(() => handleClient.Announce(tcpClient));
				bc.Start();

				//start listener
				Thread startListen = new Thread(() => handleClient.ClientListener(tcpClient));
				startListen.Start();
			}
		}

		public void removeClient(TcpClient client)
		{
			tcpClientsList.Remove(client);
		}
	}
}