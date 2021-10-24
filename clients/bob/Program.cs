using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Library;

namespace Client
{
	static class KeyRepo
	{
		public static string publicKey = "";
		public static string privateKey = "";
		public static string symmetricKey = "";
		public static string serverPublicKey = System.IO.File.ReadAllText("server-public-key.txt");
	}
	public class Read
	{
		SymmetricUtils symmetricUtils = new SymmetricUtils();
		public void ReadMessage(object obj)
		{
			TcpClient tcpClient = (TcpClient)obj;
			StreamReader streamReader = new StreamReader(tcpClient.GetStream());

			AssymetricUtils assymetricUtils = new AssymetricUtils();
			while (true)
			{
				try
				{
					//read incoming message 
					string message = streamReader.ReadLine();
					string decryptedMessage = symmetricUtils.decryptSymmetric(KeyRepo.symmetricKey, message);
					Console.WriteLine("Decrypted message : " + decryptedMessage);
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
			Read readMessage = new Read();
			SymmetricUtils symmetricUtils = new SymmetricUtils();
			AssymetricUtils assymetricUtils = new AssymetricUtils();
			try
			{
				//this computer  
				TcpClient tcpClient = new TcpClient("127.0.0.1", 5000);
				Console.WriteLine("Connected to server.");


				StreamWriter streamWriter = new StreamWriter(tcpClient.GetStream());
				StreamReader streamReader = new StreamReader(tcpClient.GetStream());

				//generates key
				assymetricUtils.assignNewKey();
				KeyRepo.privateKey = assymetricUtils.getPrivateKey();
				KeyRepo.publicKey = assymetricUtils.getPublicKey();

				//chunk-sending public key
				string str = KeyRepo.publicKey;
				int chunkSize = 16;
				int stringLength = str.Length;
				var sb = new System.Text.StringBuilder();
				for (int i = 0; i < stringLength; i += chunkSize)
				{
					if (i + chunkSize > stringLength) chunkSize = stringLength - i;
					string dataChunk = str.Substring(i, chunkSize);
					string encryptedDataChunk = assymetricUtils.encrypt(dataChunk, KeyRepo.serverPublicKey);
					sb.AppendFormat(dataChunk);
					streamWriter.WriteLine(encryptedDataChunk);
					streamWriter.Flush();
				}

				streamWriter.WriteLine("done");
				streamWriter.Flush();
				
				//decrypt symmetric key
				string message = streamReader.ReadLine();
				string decryptedMessage = assymetricUtils.decrypt(message, KeyRepo.privateKey);
				KeyRepo.symmetricKey = decryptedMessage;
				Console.WriteLine("You are now encrypted!");

				//can only read incoming chat after symmetric key received
				Thread thread = new Thread(readMessage.ReadMessage);
				thread.Start(tcpClient);

				Console.WriteLine("What's your name?");
				string name = Console.ReadLine();
				streamWriter.WriteLine(name);
				while (true)
				{
					if (tcpClient.Connected)
					{
						//send message
						string input = System.IO.File.ReadAllText("message.txt") + name;
						string encryptedMessage = symmetricUtils.encyrptSymmetric(KeyRepo.symmetricKey, input);
						streamWriter.WriteLine(encryptedMessage);
						streamWriter.Flush();
						Console.ReadLine();
					}
				}
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
			Console.ReadLine();
		}
	}
}