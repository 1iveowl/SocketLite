using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Obsolete

        [Obsolete("ObservableMessages is dreprecated, please use CreateObservableMulticastListener instead")]
        public void Disconnect()
        {
            Cleanup();
        }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public async Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationInterface = null,
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

        #endregion

        private bool _isMulticastInitialized = false;

        private readonly IDictionary<string, bool> _multicastMemberships = new Dictionary<string, bool>();

        public int TTL { get; set; } = 1;

        public int Port { get; private set; }

        public string IpAddress { get; private set; }

        public bool IsMulticastActive => _isMulticastInitialized;

        public IEnumerable<string> MulticastMemberShips => _multicastMemberships.Where(m => m.Value.Equals(true)).Select(m => m.Key);

        public UdpSocketMulticastClient()
        {
            
        }

        public async Task<IObservable<IUdpMessage>> ObservableMulticastListener(
            string multicastAddress, 
            int port,
            ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort = false)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationInterface);

            
            var serviceName = port.ToString();

            await BindeUdpServiceNameAsync(communicationInterface, serviceName, allowMultipleBindToSamePort)
                .ConfigureAwait(false);

            DatagramSocket.Control.OutboundUnicastHopLimit = (byte)TTL;

            _isMulticastInitialized = true;

            MulticastAddMembership(null, multicastAddress);

            IpAddress = multicastAddress;
            Port = port;

            var messageCancellationTokenSource = new CancellationTokenSource();

            return CreateObservableMessageStream(messageCancellationTokenSource);

        }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            if (!_isMulticastInitialized) throw new ArgumentException("Multicast interface must be initialized before adding multicast memberships");

            var hostName = new HostName(mcastAddress);

            DatagramSocket.JoinMulticastGroup(hostName);

            _multicastMemberships.Add(mcastAddress, true);
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!_isMulticastInitialized) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

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

        protected override void Cleanup()
        {
            _multicastMemberships.Clear();
            _isMulticastInitialized = false;
            CloseSocket();
        }
    }
}
