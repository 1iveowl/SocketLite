using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using ISocketLite.PCL.Exceptions;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {

        #region Obsolete

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public async Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationsInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationsInterface);

            _ipEndPoint = UdpClientInitialize(
                communicationsInterface,
                port,
                allowMultipleBindToSamePort,
                isUdpMultiCast: true,
                mcastAddress: IPAddress.Parse(multicastAddress));

            _cancellationTokenSource = new CancellationTokenSource();

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            await Task.Run(() => RunMessageReceiver(_cancellationTokenSource.Token)).ConfigureAwait(false);
        }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public void Disconnect()
        {
            Cleanup();
        }

        #endregion

        public IEnumerable<string> MulticastMemberShips => MulticastMemberships.Where(m => m.Value.Equals(true)).Select(m => m.Key);

        private IPEndPoint _ipEndPoint;

        public UdpSocketMulticastClient()
        {
        }

        private string _multicastAddress;
        private int _multicastPort;
        private CancellationTokenSource _cancellationTokenSource;

        public int TTL { get; set; } = 1;

        public int Port => _ipEndPoint.Port;
        public string IpAddress => _ipEndPoint.Address.ToString();


        public async Task<IObservable<IUdpMessage>> ObservableMulticastListener(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationsInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationsInterface);

            _ipEndPoint = UdpClientInitialize(
                communicationsInterface,
                port,
                allowMultipleBindToSamePort,
                isUdpMultiCast: true,
                mcastAddress: IPAddress.Parse(multicastAddress));

            _cancellationTokenSource = new CancellationTokenSource();

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            return CreateObservableMessageStream(_cancellationTokenSource);
        }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            if (!IsMulticastActive)
            {
                ConvertUnicastToMulticast(IPAddress.Parse(ipLan));
            }

            MulticastAddMembership(IPAddress.Parse(ipLan), IPAddress.Parse(mcastAddress));
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!IsMulticastActive) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

            if (!MulticastMemberships.ContainsKey(mcastAddress)) return;

            BackingUdpClient.DropMulticastGroup(IPAddress.Parse(mcastAddress));

            MulticastMemberships.Remove(mcastAddress);
        }

        protected override void Cleanup()
        {
            _cancellationTokenSource.Cancel();
            MulticastMemberships.Clear();

#if (NETSTANDARD1_5 || NETSTANDARD1_3)
            BackingUdpClient?.Dispose();
#else
            BackingUdpClient?.Close();
#endif

            _multicastAddress = null;
            _multicastPort = 0;
            base.Cleanup();
        }

        public async Task SendMulticastAsync(byte[] data)
        {
            await SendMulticastAsync(data, data.Length).ConfigureAwait(false);
        }

        public async Task SendMulticastAsync(byte[] data, int length)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, length, _multicastAddress, _multicastPort).ConfigureAwait(false);
        }


    }
}
