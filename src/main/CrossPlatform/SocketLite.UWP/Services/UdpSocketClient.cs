using System;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketClient : UdpSendBase, IUdpSocketClient
    {
        public async Task ConnectAsync(
            string address, 
            int port,
            bool allowMultipleBindToSamePort = false)
        {
            var hostName = new HostName(address);
            var serviceName = port.ToString();

            ConfigureDatagramSocket(allowMultipleBindToSamePort);

            await DatagramSocket.ConnectAsync(hostName, serviceName);
        }

        public void Disconnect()
        {
            DatagramSocket?.Dispose();
        }

        public void Dispose()
        {
            DatagramSocket?.Dispose();
        }
    }
}
