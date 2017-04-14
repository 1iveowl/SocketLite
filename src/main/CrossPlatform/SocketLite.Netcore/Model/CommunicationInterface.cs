using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net;
//using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;

namespace SocketLite.Model
{

    // .NET Standard
    public class CommunicationInterface : ICommunicationInterface
    {
        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

        public string NativeInterfaceId { get; }
        public string Name { get; }
        public string IpAddress { get; }
        public string GatewayAddress { get; }
        public string BroadcastAddress { get; }
        public CommunicationConnectionStatus ConnectionStatus { get; }
        public bool IsUsable { get; }
        public bool IsLoopback => _loopbackAddresses.Contains(IpAddress);

        public IEnumerable<ICommunicationInterface> GetAllInterfaces()
        {
            NetworkInterface.GetAllNetworkInterfaces();
        }
    }
}
