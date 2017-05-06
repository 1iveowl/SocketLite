using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Extensions;
using SocketLite.Services.Base;
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        private IPEndPoint _ipEndPoint;
        private CancellationTokenSource _cancellationTokenSource;
        public int Port => _ipEndPoint.Port;

        public async Task<IObservable<IUdpMessage>> CreateObservableListener(
            int port = 0,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            _ipEndPoint = InitializeUdpClient(communicationInterface, port, allowMultipleBindToSamePort);

            _cancellationTokenSource = new CancellationTokenSource();

            return CreateObservableMessageStream(_cancellationTokenSource);
        }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public async Task StartListeningAsync(
            int port = 0, 
            ICommunicationInterface communicationInterface = null, 
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            _ipEndPoint = InitializeUdpClient(communicationInterface, port, allowMultipleBindToSamePort);

            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => RunMessageReceiver(_cancellationTokenSource.Token))
                .ConfigureAwait(false);
        }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public void StopListening()
        {
            Cleanup();
        }

        protected override void Cleanup()
        {
            _cancellationTokenSource.Cancel();

#if (NETSTANDARD1_5)
            BackingUdpClient.Dispose();
#else
            BackingUdpClient.Close();
#endif
            base.Cleanup();
        }

        public override async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            if (BackingUdpClient == null)
            {
                try
                {
                    using (var backingPort = new UdpClient { EnableBroadcast = true })
                    {
                        await backingPort.SendAsync(data, data.Length, address, port).WrapNativeSocketExceptions();
                    }
                }
                catch (PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }
            }
            else
            {
                await base.SendToAsync(data, length, address, port).ConfigureAwait(false);
            }
        }
    }
}
