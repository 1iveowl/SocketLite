using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Extensions;


// .NET 4.51
namespace SocketLite.Model
{
    public partial class CommunicationsInterface : ICommunicationInterface
    {
        public string NativeInterfaceId { get; private set; }

        public string Name { get; private set; }

        public string IpAddress { get; private set; }

        public string GatewayAddress { get; private set; }

        public string BroadcastAddress { get; private set; }

        public bool IsUsable => !string.IsNullOrWhiteSpace(IpAddress);

        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

        public bool IsLoopback => _loopbackAddresses.Contains(IpAddress);
        //public bool IsInternetConnected { get; internal set; }

        public CommunicationConnectionStatus ConnectionStatus { get; private set; }

        private NetworkInterface NativeInterface;

        internal IPAddress NativeIpAddress { get; set; }

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

#if (NETSTANDARD1_5 || NETSTANDARD2_0 || NETSTANDARD1_3)
            string broadcast = null;
#else
            var broadcast = (ip != null && netmask != null) ? ip.Address.GetBroadcastAddress(netmask).ToString() : null;
#endif
            
            return new CommunicationsInterface
            {
                NativeInterfaceId = nativeInterface.Id,
                NativeIpAddress = ip?.Address,
                Name = nativeInterface.Name,
                IpAddress = ip?.Address.ToString(),
                GatewayAddress = gateway,
                BroadcastAddress = broadcast,
#if (NETSTANDARD1_5 || NETSTANDARD2_0 || NETSTANDARD1_3)
                ConnectionStatus = new CommunicationConnectionStatus(),
#else
                ConnectionStatus = nativeInterface.OperationalStatus.ToCommsInterfaceStatus(),
#endif
                NativeInterface = nativeInterface
            };
        }


    }
}
