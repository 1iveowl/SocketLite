using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {
        public int Port { get; private set; }
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

        public void StopListening()
        {
            CloseSocket();
        }
    }
}
