using System;
using System.Collections;
using System.Linq;

namespace SocketLite.Extensions
{
    public static class NetworkExtensions
    {
        public static string GetBroadcastAddress(string address, string subnetMask)
        {
            var addressBytes = GetAddressBytes(address);
            var subnetBytes = GetAddressBytes(subnetMask);

            var broadcastBytes = addressBytes.Zip(subnetBytes, (a, s) => (byte)(a | (s ^ 255))).ToArray();

            return broadcastBytes.ToDottedQuadNotation();
        }

        public static string GetSubnetAddress(string ipAddress, int prefixLength)
        {
            var maskBits = Enumerable.Range(0, 32)
                .Select(i => i < prefixLength)
                .ToArray();

            return maskBits
                .ToBytes()
                .ToDottedQuadNotation();
        }

        public static byte[] ToBytes(this bool[] bits)
        {
            var theseBits = bits.Reverse().ToArray();
            var ba = new BitArray(theseBits);

            var bytes = new byte[theseBits.Length / 8];
            ((ICollection)ba).CopyTo(bytes, 0);

            bytes = bytes.Reverse().ToArray();

            return bytes;
        }

        public static byte[] GetAddressBytes(string ipAddress)
        {
            if (ipAddress == null) return new byte[4] {0, 0, 0, 0};

            var ipBytes = new byte[4];

            var parsesResults =
                ipAddress.Split('.')
                    .Select((p, i) => byte.TryParse(p, out ipBytes[i]))
                    .ToList();

            var valid = (parsesResults.Count == 4 && parsesResults.All(r => r));

            if (valid)
                return ipBytes;
            else
                throw new InvalidOperationException("The string provided did not appear to be a valid IP Address");
        }

        public static string ToDottedQuadNotation(this byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
                throw new InvalidOperationException("Byte array has an invalid byte count for dotted quad conversion");

            return string.Join(".", bytes.Select(b => ((int)b).ToString()));
        }
    }
}
