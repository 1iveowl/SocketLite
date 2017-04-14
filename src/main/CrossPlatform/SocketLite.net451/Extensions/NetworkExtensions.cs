using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using ISocketLite.PCL.Model;
using CommunicationsInterface = SocketLite.Model.CommunicationsInterface;

namespace SocketLite.Extensions
{
    public static class NetworkExtensions
    {
        public static CommunicationsInterface ToCommsInterfaceSummary(this NetworkInterface nativeInterface)
        {
            return CommunicationsInterface.FromNativeInterface(nativeInterface);
        }

        public static CommunicationConnectionStatus ToCommsInterfaceStatus(this OperationalStatus nativeStatus)
        {
            switch (nativeStatus)
            {
                case OperationalStatus.Up:
                    return CommunicationConnectionStatus.Connected;
                case OperationalStatus.Down:
                    return CommunicationConnectionStatus.Disconnected;
                case OperationalStatus.Unknown:
                    return CommunicationConnectionStatus.Unknown;
                case OperationalStatus.Testing:
                    return CommunicationConnectionStatus.Testing;
                case OperationalStatus.Dormant:
                    return CommunicationConnectionStatus.Dormant;
                case OperationalStatus.LowerLayerDown:
                    return CommunicationConnectionStatus.LowerLayerDown;
                case OperationalStatus.NotPresent:
                    return CommunicationConnectionStatus.NotPresent;
                default:
                    return CommunicationConnectionStatus.Unknown;
            }

        }

        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var addressBytes = address.GetAddressBytes();
            var subnetBytes = subnetMask.GetAddressBytes();

            var broadcastBytes = addressBytes.Zip(subnetBytes, (a, s) => (byte)(a | (s ^ 255))).ToArray();

            return new IPAddress(broadcastBytes);
        }
    }
}
