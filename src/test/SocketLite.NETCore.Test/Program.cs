﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using SocketLite.Model;
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

            var communicationInterface = new CommunicationsInterface();
            var allInterfaces = communicationInterface.GetAllInterfaces();

            var firstUsableInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == "10.211.55.8");

            //var tcpListener = new TcpSocketListener();

            //await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort:true);

            var udpMulti = new UdpSocketMulticastClient();

            var ipv6MultiCastAddressList = new List<string>
            {
                "ff02::c",
            };

            await udpMulti.JoinMulticastGroupAsync(
                "239.255.255.250", 
                1900, 
                firstUsableInterface, 
                allowMultipleBindToSamePort: true,
                mcastIpv6AddressList:ipv6MultiCastAddressList);

            var udpListener = new UdpSocketReceiver();

            await udpListener.StartListeningAsync(1900, communicationInterface: firstUsableInterface, allowMultipleBindToSamePort: true);

            Console.WriteLine("Listening...");
        }
    }
}