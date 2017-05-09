using System;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        
        public int Port { get; private set; }
        public bool IsUnicastInterfaceActive => _isUnicastInitialized;

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

        protected override void Cleanup()
        {
            CloseSocket();
        }
    }
}
