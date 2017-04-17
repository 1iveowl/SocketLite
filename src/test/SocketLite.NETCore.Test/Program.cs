using System;
using System.Threading;
using SocketLite.Services;

namespace SocketLite.NETCore.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

        }

        static async void Start()
        {
            var tcpListener = new TcpSocketListener();

            await tcpListener.StartListeningAsync(8000);

            Console.WriteLine("Listening...");
        }
    }
}