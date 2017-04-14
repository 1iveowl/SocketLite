using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketClient : UdpSendBase, IUdpSocketClient
    {
        public async Task ConnectAsync(string address, int port, bool allowMultipleBindToSamePort = false)
        {
            var ipAdress = IPAddress.Parse(address);
            var ipEndPoint = new IPEndPoint(ipAdress, port);

            if (allowMultipleBindToSamePort)
            {
                BackingUdpClient.ExclusiveAddressUse = false;
                BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            else
            {
                BackingUdpClient.ExclusiveAddressUse = true;
                BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            }

            BackingUdpClient = new UdpClient(ipEndPoint);
            //await Task.CompletedTask;
        }

        public void Disconnect()
        {
            MessageConcellationTokenSource?.Cancel();
            Dispose();
        }

        public void Dispose()
        {
//#if (NETSTANDARD)
            BackingUdpClient?.Dispose();
//#else
//            BackingUdpClient?.Close();
//#endif
        }
    }
}
