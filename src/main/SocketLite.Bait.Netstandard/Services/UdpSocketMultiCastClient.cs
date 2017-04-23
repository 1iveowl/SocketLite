using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
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


    }
}
