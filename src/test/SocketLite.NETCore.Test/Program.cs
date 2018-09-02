using System;
using System.Linq;
using System.Threading.Tasks;
using SocketLite.Model;
using SocketLite.Services;

namespace SocketLite.NETCore.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TestTcpSocket();

            await Start();

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

        }

        static async Task TestTcpSocket()
        {
            //var uri = new Uri("...");

            //var socketClient = new TcpSocketClient();

            //await socketClient.ConnectAsync(uri.Host, uri.Port.ToString(),true, ignoreServerCertificateErrors:true);

            //var t = "";


        }

        static async Task Start()
        {

            var communicationInterface = new CommunicationsInterface();
            var allInterfaces = communicationInterface.GetAllInterfaces();

            var networkInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == "192.168.0.59");

            //var tcpListener = new TcpSocketListener();

            //var observerTcpListner = await tcpListener.CreateObservableListener(
            //    port:8000, 
            //    communicationInterface: networkInterface, 
            //    allowMultipleBindToSamePort:true);

            //var subscriberTcpListener = observerTcpListner.Subscribe(
            //    tcpClient =>
            //    {
            //        //Insert your code here
            //    },
            //    ex =>
            //    {
            //        // Insert your exception code here
            //    },
            //    () =>
            //    {
            //        // Insert your completed code here
            //    });

            //var udpReceiver = new UdpSocketReceiver();

            //var observerUdpReceiver = await udpReceiver.ObservableUnicastListener(
            //    port: 1900,
            //    communicationInterface: networkInterface,
            //    allowMultipleBindToSamePort: true);

            //var subscriberUpdReceiver = observerUdpReceiver.Subscribe(
            //    udpMsg =>
            //    {
            //        System.Console.WriteLine($"Udp package received: {udpMsg.RemoteAddress}:{udpMsg.RemotePort}");
            //    },
            //    ex =>
            //    {
            //        //Inset your exception code here
            //    },
            //    () =>
            //    {
            //        //Insert your completion code here
            //    });

            var udpMulticast = new UdpSocketMulticastClient();

            var observerUdpMulticast = await udpMulticast.ObservableMulticastListener(
                "239.255.255.250",
                1900,
                networkInterface,
                allowMultipleBindToSamePort: true);
            
            var subscriberUdpMilticast = observerUdpMulticast.Subscribe(
                udpMsg =>
                {
                    System.Console.WriteLine($"Udp package received: {udpMsg.RemoteAddress}:{udpMsg.RemotePort}");
                },
                ex =>
                {
                    //Insert your exception code here
                },
                () =>
                {
                    //Insert your completion code here
                });

            //await udpMulti.JoinMulticastGroupAsync(
            //    "239.255.255.250", 
            //    1900, 
            //    firstUsableInterface, 
            //    allowMultipleBindToSamePort: true,
            //    mcastIpv6AddressList:ipv6MultiCastAddressList);

            //var udpListener = new UdpSocketReceiver();

            //await udpListener.StartListeningAsync(1900, communicationInterface: firstUsableInterface, allowMultipleBindToSamePort: true);

            Console.WriteLine("Listening...");
        }
    }
}