using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {

        #region Obsolete

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public async Task StartListeningAsync(
            int port = 0,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            Port = port;

            CheckCommunicationInterface(communicationInterface);

            var serviceName = port == 0 ? "" : port.ToString();

            await BindeUdpServiceNameAsync(communicationInterface, serviceName, allowMultipleBindToSamePort)
                .ConfigureAwait(false);
        }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public void StopListening()
        {
            Cleanup();
            base.Cleanup();
        }

        #endregion

        private readonly IDictionary<string, bool> _multicastMemberships = new Dictionary<string, bool>();

        public int Port { get; private set; }

        public bool IsUnicastActive => _isUnicastInitialized;

        public bool IsMulticastActive { get; private set; }

        public async Task<IObservable<IUdpMessage>> ObservableUnicastListener(
            int port = 0, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            Port = port;

            CheckCommunicationInterface(communicationInterface);

            var serviceName = port == 0 ? "" : port.ToString();

            await BindeUdpServiceNameAsync(communicationInterface, serviceName, allowMultipleBindToSamePort)
                .ConfigureAwait(false);

            var cancellationSourceToken = new CancellationTokenSource();

            _isUnicastInitialized = true;

            return CreateObservableMessageStream(cancellationSourceToken);

        }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            if (!_isUnicastInitialized) throw new ArgumentException("Multicast interface must be initialized before adding multicast memberships");

            var hostName = new HostName(mcastAddress);

            DatagramSocket.JoinMulticastGroup(hostName);

            _multicastMemberships.Add(mcastAddress, true);
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!_isUnicastInitialized) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

        }
        protected override void Cleanup()
        {
            CloseSocket();
        }
    }
}
