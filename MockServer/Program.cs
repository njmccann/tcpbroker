using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MockServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mock Server");

            var lck = new object();

            var clients = new Dictionary<TcpClient, NetworkStream>();

            var srv = new TcpListener(IPAddress.Any, 52001);

            srv.Start();

            while (true)
            {
                var client = srv.AcceptTcpClient();
                var stream = client.GetStream();

                Console.WriteLine("Client connected");

                lock (lck)
                {
                    clients.Add(client, stream);
                }

                readFrom(stream, (data, start, len) =>
                {
                    lock (lck)
                    {
                        foreach(var stm in clients.Values)
                        {
                            if(stm != stream)
                            {
                                stm.Write(data, 0, data.Length);
                            }
                        }
                    }
                });
            }
        }

        private static Task readFrom(NetworkStream stream, Action<byte[], int, int> p)
        {
            return Task.Run(() =>
            {
                while (stream.CanRead)
                {
                    var buff = new byte[4096];
                    var len = stream.Read(buff, 0, buff.Length);

                    p(buff, 0, len);
                }
            });
        }

    }
}
