using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SocketLite.Extensions
{
    public static class AndroidNetworkExtensions
    {
        // thank you deapsquatter - https://gist.github.com/deapsquatter/5644550

        public static IEnumerable<T> GetEnumerable<T>(this Java.Util.IEnumeration enumeration) where T : class
        {
            while (enumeration.HasMoreElements)
                yield return enumeration.NextElement() as T;
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