using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Extensions;

namespace SocketLite.Model
{
    public partial class CommunicationsInterface : ICommunicationInterface
    {
        public string NativeInterfaceId { get; internal set; }

        public string Name { get; internal set; }

        public string IpAddress { get; internal set; }

        public string GatewayAddress { get; internal set; }

        public string BroadcastAddress { get; internal set; }

        public bool IsUsable => !string.IsNullOrWhiteSpace(IpAddress);

        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

        public bool IsLoopback => _loopbackAddresses.Contains(IpAddress);
        //public bool IsInternetConnected { get; internal set; }

        public CommunicationConnectionStatus ConnectionStatus { get; internal set; }

        internal NetworkInterface NativeInterface;

        internal IPAddress NativeIpAddress;

        internal IPEndPoint EndPoint(int port)
        {
            return new IPEndPoint(NativeIpAddress, port);
        }

        

        public IEnumerable<ICommunicationInterface> GetAllInterfaces()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Select(FromNativeInterface);
        }

        internal static CommunicationsInterface FromNativeInterface(NetworkInterface nativeInterface)
        {
            var ip =
                nativeInterface
                    .GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            var gateway =
                nativeInterface
                    .GetIPProperties()
                    .GatewayAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();

            var netmask = ip != null ? CommunicationsInterface.GetSubnetMask(ip) : null; // implemented natively for each .NET platform

            var broadcast = (ip != null && netmask != null) ? ip.Address.GetBroadcastAddress(netmask).ToString() : null;

            return new CommunicationsInterface
            {
                NativeInterfaceId = nativeInterface.Id,
                NativeIpAddress = ip?.Address,
                Name = nativeInterface.Name,
                IpAddress = ip?.Address.ToString(),
                GatewayAddress = gateway,
                BroadcastAddress = broadcast,
                ConnectionStatus = nativeInterface.OperationalStatus.ToCommsInterfaceStatus(),
                NativeInterface = nativeInterface
            };
        }
    }
}
