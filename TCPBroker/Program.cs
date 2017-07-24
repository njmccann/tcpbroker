using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPBroker
{
    class Program
    {
        static void Main(string[] args)
        {
			run();
			return;
        }

		private static string getLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("Local IP Address Not Found!");
		}

		private static void run()
		{
			Console.WriteLine("IP: " + getLocalIPAddress());

			var srv = new TcpListener(IPAddress.Any, 52002);
			srv.Start();
			Console.WriteLine("Server started");

			var wrkRecPipe = srv.AcceptTcpClient();
			Console.WriteLine("Client connected");
			var wrkSendPipe = new TcpClient();
			wrkSendPipe.Connect(wrkRecPipe.GetRemoteIP(), 52002);
			Console.WriteLine("Connected to client");

			var wrkDupe = new DuplexConnection();
			wrkDupe.Inbound = new ReadWriteClient(wrkRecPipe, "Wrk Inbound");
			wrkDupe.Outbound = new ReadWriteClient(wrkSendPipe, "Wrk Outbound");

			// connect to TG server
			var srvSendPipe = new TcpClient();
			srvSendPipe.Connect("10.220.5.246", 52002);
			Console.WriteLine("Connected to server");

			// TG server will connect to us
			var srvRecPipe = srv.AcceptTcpClient();
			Console.WriteLine("Server connected");

			var srvDupe = new DuplexConnection();
			srvDupe.Inbound = new ReadWriteClient(srvRecPipe, "Server Inbound");
			srvDupe.Outbound = new ReadWriteClient(srvSendPipe, "Server Outbound");

			srvDupe.Inbound.OnRead = wrkDupe.Outbound.Send;
			wrkDupe.Inbound.OnRead = srvDupe.Outbound.Send;

			srvDupe.Inbound.OnClose = srvDupe.Outbound.Stop;
			wrkDupe.Inbound.OnClose = wrkDupe.Outbound.Stop;

			srvDupe.Start();
			wrkDupe.Start();

			Console.WriteLine("Press enter to quit");
			Console.ReadLine();
		}

    }

	public static class extensions
	{
		public static IPAddress GetRemoteIP(this TcpClient client)
		{
			var ep = client.Client.RemoteEndPoint as System.Net.IPEndPoint;

			return ep.Address;
		}
	}

}
