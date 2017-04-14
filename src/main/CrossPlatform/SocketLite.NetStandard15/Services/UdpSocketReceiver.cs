using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
        public int Port => _ipEndPoint.Port;

        public async Task StartListeningAsync(
            int port = 0, 
            ICommunicationInterface communicationInterface = null, 
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            var ipAddress = IPAddress.Any;

            _ipEndPoint = new IPEndPoint(ipAddress, port);

            InitializeUdpClient(_ipEndPoint, allowMultipleBindToSamePort);

            MessageConcellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => RunMessageReceiver(MessageConcellationTokenSource.Token))
                .ConfigureAwait(false);
        }

        public void StopListening()
        {
            MessageConcellationTokenSource.Cancel();

#if (NETSTANDARD1_5)
            BackingUdpClient.Dispose();
#else
            BackingUdpClient.Close();
#endif

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
