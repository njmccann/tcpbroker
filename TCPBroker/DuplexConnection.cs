using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPBroker
{

	public class DuplexConnection
	{
		public ReadWriteClient Inbound { get; set; }
		public ReadWriteClient Outbound { get; set; }
		public void Start()
		{
			Inbound.Start();
			Outbound.Start();
		}
	}

	public class ReadWriteClient
	{

		public TcpClient Client { get; private set; }

		public NetworkStream Stream { get; private set; }

		public Action OnClose { get; set; }

		public Action<byte[], int> OnRead { get; set; }

		public bool Connected { get; private set; }

		public string Name { get; set; }

		private readonly object _lck = new object();

		private string path = "";

		private FileStream io;

		public ReadWriteClient(TcpClient client, string name)
		{
			Client = client;
			Stream = client.GetStream();
			
			Connected = true;
			Name = name;

			path = "out\\" + Name + "_out.txt";
		}

		public void Send(byte[] buff, int len)
		{
			if (Connected && Stream != null && Stream.CanWrite)
			{
				Stream.Write(buff, 0, len);
			}
		}

		public void Start()
		{
			read();
		}

		public void Stop()
		{
			try
			{
				if (Client != null) Client.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private Task read()
		{
			return Task.Run(() =>
			{
				io = File.OpenWrite("out\\" + Name + "_out.hex");

				try
				{
					while (Stream.CanRead)
					{
						var buff = new byte[Client.ReceiveBufferSize];
						var len = Stream.Read(buff, 0, buff.Length);

						if (len == 0)
						{
							OnClose();
							Client.Close();
							break;
						}

						Console.WriteLine(Name + " - " + len + " bytes");
						
						io.Write(buff, 0, len);

						var content = Encoding.Default.GetString(buff, 0, len);
						File.AppendAllText(path, DateTime.Now.ToString("hh\\:mm\\:ss\\.fff - ") + content);
						File.AppendAllText(path, "\r\nBytes - ");
						File.AppendAllText(path, BitConverter.ToString(buff, 0, len));
						File.AppendAllText(path, "\r\n------------------------EOF--------------------------\r\n\r\n");

						OnRead(buff, len);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				Connected = false;
			});
		}

	}

}
