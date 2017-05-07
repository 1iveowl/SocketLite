using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        public int TTL { get; set; }

        public int Port { get; }
        public string IpAddress { get; }

        public IObservable<IUdpMessage> ObservableMessages { get; } = null;

        public bool IsMulticastInterfaceActive => false;

        public IObservable<IUdpMessage> CreateObservableMultiCastListener(string multicastAddress, int port,
            ICommunicationInterface communicationInterface, bool allowMultipleBindToSamePort = false)
        {
            throw new NotImplementedException();
        }

        public IObservable<IUdpMessage> CreateObservableMultiCastListener(string multicastAddress, int port,
            ICommunicationInterface communicationInterface, IEnumerable<string> mcastIpv6AddressList,
            bool allowMultipleBindToSamePort = false)
        {
            throw new NotImplementedException();
        }

        public async Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port, ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false)
        {
            await JoinMulticastGroupAsync(
                multicastAddress,
                port,
                communicationInterface,
                null,
                allowMultipleBindToSamePort);
        }

        public IEnumerable<string> MulticastMemberShips { get; }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            throw new NotImplementedException();
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            throw new NotImplementedException();
        }

        public Task JoinMulticastGroupAsync(
            string multicastAddress, 
            int port, ICommunicationInterface communicationInterface,
            IEnumerable<string> mcastIpv6AddressList = null,
            bool allowMultipleBindToSamePort = false)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Disconnect()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public Task SendMulticastAsync(byte[] data)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public Task SendMulticastAsync(byte[] data, int length)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Dispose()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        Task<IObservable<IUdpMessage>> IUdpSocketMulticastClient.CreateObservableMultiCastListener(string multicastAddress, int port, ICommunicationInterface communicationInterface, bool allowMultipleBindToSamePort)
        {
            throw new NotImplementedException();
        }
    }
}
