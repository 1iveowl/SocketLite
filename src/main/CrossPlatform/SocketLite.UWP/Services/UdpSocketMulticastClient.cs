using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.EventArgs;
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

        public async Task<IObservable<IUdpMessage>> CreateObservableMultiCastListener(
            string multicastAddress, 
            int port,
            ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort = false)
        {
            return await CreateObservableMultiCastListener(
                multicastAddress,
                port,
                communicationInterface,
                null,
                allowMultipleBindToSamePort);
        }

        public async Task<IObservable<IUdpMessage>> CreateObservableMultiCastListener(
            string multicastAddress, 
            int port,
            ICommunicationInterface communicationInterface, 
            IEnumerable<string> mcastIpv6AddressList,
            bool allowMultipleBindToSamePort = false)
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

            var messageCancellationTokenSource = new CancellationTokenSource();

            return CreateObservableMessageStream(messageCancellationTokenSource);

        }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public async Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            await JoinMulticastGroupAsync(
                multicastAddress, 
                port, 
                communicationInterface, 
                null,
                allowMultipleBindToSamePort);
        }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public async Task JoinMulticastGroupAsync(
            string multicastAddress, 
            int port, 
            ICommunicationInterface communicationInterface = null,
            IEnumerable<string> mcastIpv6AddressList = null,
            bool allowMultipleBindToSamePort = false)
            
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

        [Obsolete("ObservableMessages is dreprecated, please use CreateObservableMulticastListener instead")]
        public void Disconnect()
        {
            Cleanup();
        }

        protected override void Cleanup()
        {
            CloseSocket();
        }
    }
}
