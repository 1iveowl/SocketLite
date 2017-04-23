using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        public int TTL { get; set; } = 1;

        public int Port { get; private set; }

        public string IpAddress { get; private set; }

        public UdpSocketMulticastClient()
        {
            
        }

        public async Task JoinMulticastGroupAsync(
            string multicastAddress, 
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false,
            IEnumerable<string> mcastIpv6AddressList = null)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationInterface);

            var hostName = new HostName(multicastAddress);
            var serviceName = port.ToString();

            await BindeUdpServiceNameAsync(communicationInterface, serviceName, allowMultipleBindToSamePort)
                .ConfigureAwait(false);

            DatagramSocket.Control.OutboundUnicastHopLimit = (byte)TTL;
            DatagramSocket.JoinMulticastGroup(hostName);

            IpAddress = multicastAddress;
            Port = port;
        }

        public async Task SendMulticastAsync(byte[] data)
        {
            await SendMulticastAsync(data, data.Length).ConfigureAwait(false);
        }

        public async Task SendMulticastAsync(byte[] data, int length)
        {
            if (IpAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, IpAddress, Port)
                .ConfigureAwait(false);
        }

        public void Disconnect()
        {
            CloseSocket();
        }
    }
}
