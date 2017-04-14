using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services.Base;
//#if !(NETSTANDARD) //Not NetStandard
//using CommunicationInterface = SocketLite.Model.CommunicationsInterface;
//#endif
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {
        public UdpSocketMulticastClient()
        {
        }

        private string _multicastAddress;
        private int _multicastPort;

        public int TTL { get; set; } = 1;

        public int Port { get; private set; }
        public string IpAddress { get; private set; }

        public async Task JoinMulticastGroupAsync(
            string multicastAddress, 
            int port, 
            ICommunicationInterface communicationInterface = null, 
            bool allowMultipleBindToSamePort = false)
        {
            Port = port;
            IpAddress = multicastAddress;
            
            CheckCommunicationInterface(communicationInterface);
//#if (NETSTANDARD)
            var ipAddress = IPAddress.Any;
//#else
//            var ipAddress = (communicationInterface as CommunicationInterface)?.NativeIpAddress ?? IPAddress.Any;
//#endif
             var ipEndPoint = new IPEndPoint(ipAddress, port);

            InitializeUdpClient(ipEndPoint, allowMultipleBindToSamePort);

            MessageConcellationTokenSource = new CancellationTokenSource();

            var multicastIp = IPAddress.Parse(multicastAddress);
            try
            {
                
                BackingUdpClient.JoinMulticastGroup(multicastIp, TTL);
            }
            catch (Exception ex)
            {
                throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new PclSocketException(ex)
                        : ex;
            }

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            await Task.Run(() => RunMessageReceiver(MessageConcellationTokenSource.Token)).ConfigureAwait(false);
        }

        public void Disconnect()
        {
            MessageConcellationTokenSource.Cancel();

//#if (NETSTANDARD)
            BackingUdpClient?.Dispose();
//#else
//            BackingUdpClient?.Close();
//#endif

            _multicastAddress = null;
            _multicastPort = 0;
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
