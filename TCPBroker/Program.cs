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
            Console.WriteLine("TCPBroker");

            System.Threading.Thread.Sleep(5000);

            var srv = new TcpListener(IPAddress.Any, 52002);

            srv.Start();

            Console.WriteLine("Server started");

            while (true)
            {
                var client = srv.AcceptTcpClient();

                Console.WriteLine("Client connected");

                var tgClient = new TcpClient();

                //tgClient.Connect("10.220.5.246", 52002);
                tgClient.Connect("127.0.0.1", 52001);

                Console.WriteLine("TGClient connected");

                var s1 = tgClient.GetStream();
                var s2 = client.GetStream();

                var stitch = new Stitch(s1, s2);
            }
        }
    }
}
