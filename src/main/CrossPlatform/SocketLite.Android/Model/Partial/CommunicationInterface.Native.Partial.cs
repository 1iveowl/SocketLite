using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NetworkInterface = Java.Net.NetworkInterface;
using SocketLite.Extensions;
using static SocketLite.Extensions.AndroidNetworkExtensions;

namespace SocketLite.Model
{
    public partial class CommunicationsInterface
    {
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            // short circuit on null ip. 
            if (ip == null)
                return null;

            // TODO: Use native java network library rather than incomplete mono/.NET implementation: 
            // Move this into CommsInterface.cs and call once rather than iterating all adapters for each GetSubnetMaskCall. 
            var interfaces = NetworkInterface.NetworkInterfaces.GetEnumerable<NetworkInterface>().ToList();
            var interfacesWithIPv4Addresses = interfaces
                                                .Where(ni => ni.InterfaceAddresses != null)
                                                .SelectMany(ni => ni.InterfaceAddresses
                                                .Where(a => a.Address?.HostAddress != null)
                                                .Select(a => new { NativeInterface = ni, Address = a }))
                                                .ToList();

            var ipAddress = ip.Address.ToString();

            // match the droid interface with the NetworkInterface interface on the IpAddress string
            var match = interfacesWithIPv4Addresses.FirstOrDefault(ni => ni.Address.Address.HostAddress == ipAddress);

            // no match, no good
            if (match == null)
                return null;

            // use the network prefix length to calculate the subnet address
            var networkPrefixLength = match.Address.NetworkPrefixLength;
            var netMask = AndroidNetworkExtensions.GetSubnetAddress(ipAddress, networkPrefixLength);

            return IPAddress.Parse(netMask);
        }
    }
}