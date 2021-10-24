using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Library;

namespace Server
{
	static class KeyRepo
	{
		public static string publicKey = System.IO.File.ReadAllText("server-public-key.txt");
		public static string privateKey = System.IO.File.ReadAllText("server-private-key.txt");
		public static string symmetricKey = "";
		public static string clientPublicKey = "";
	}
	class HandleClient
	{
		Program program = new Program();

		public void clientListener(object obj)
		{
			TcpClient tcpClient = (TcpClient)obj;
			StreamReader reader = new StreamReader(tcpClient.GetStream());
			SymmetricUtils symmetricUtils = new SymmetricUtils();
			AssymetricUtils assymetricUtils = new AssymetricUtils();

			try
			{
				//chunk-receiving big data
				var sb = new System.Text.StringBuilder();
				string temp;
				while ((temp = reader.ReadLine()) != "done")
				{
					string key = assymetricUtils.decrypt(temp, KeyRepo.privateKey);
					sb.AppendFormat(key);
				}

				KeyRepo.clientPublicKey = sb.ToString();

				//sends symmetric key
				string symmetricKey = KeyRepo.symmetricKey;
				string encryptedMessage = assymetricUtils.encrypt(symmetricKey, KeyRepo.clientPublicKey);
				broadCast(encryptedMessage, tcpClient, null);

				string name = reader.ReadLine();

				while (true)
				{
					try
					{
						//record chat  
						string message = reader.ReadLine();
						string decryptedMessage = symmetricUtils.decryptSymmetric(KeyRepo.symmetricKey, message);
						Console.WriteLine("Decrypted message : " + decryptedMessage);
						encryptedMessage = symmetricUtils.encyrptSymmetric(KeyRepo.symmetricKey, decryptedMessage);
						broadCast(encryptedMessage, tcpClient, name);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						program.removeClient(tcpClient);
						break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				program.removeClient(tcpClient);
			}

		}

		public void broadCast(string msg, TcpClient excludeClient, string n)
		{
			if (n == null)
			{
				//send symmetric key
				StreamWriter sWriter = new StreamWriter(excludeClient.GetStream());
				sWriter.WriteLine(msg);
				sWriter.Flush();
			}
			else
			{
				foreach (TcpClient client in Program.tcpClientsList)
				{
					//broadCast to all client except sender
					StreamWriter sWriter = new StreamWriter(client.GetStream());
					string message = msg;
					if (client != excludeClient)
					{
						sWriter.WriteLine(message);
					}
					sWriter.Flush();
				}
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
			SymmetricUtils symmetricUtils = new SymmetricUtils();
			AssymetricUtils assymetricUtils = new AssymetricUtils();
			assymetricUtils.assignNewKey();

			//make sure only one symmetric key used
			KeyRepo.symmetricKey = symmetricUtils.makeSymmetricKey(32);
			HandleClient handleClient = new HandleClient();

			//start process
			tcpListener = new TcpListener(IPAddress.Any, 5000);
			tcpListener.Start();
			Console.WriteLine("Server created");

			while (true)
			{
				//add clients to list
				TcpClient tcpClient = tcpListener.AcceptTcpClient();
				tcpClientsList.Add(tcpClient);

				//start listener
				Thread startListen = new Thread(() => handleClient.clientListener(tcpClient));
				startListen.Start();
			}

		}
		//remove client if connection lost
		public void removeClient(TcpClient client)
		{
			tcpClientsList.Remove(client);
		}
	}
}