using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketLite.Model
{
    internal static class UpdClientManager
    {

        internal static int UnicastPort { get; set; }

        internal static int MulticastPort { get; set; }

        //private static readonly IDictionary<int, bool> _udpPortList = new Dictionary<int, bool>();

        //internal static bool IsUdpPortInUse(int port) => _udpPortList.ContainsKey(port) && _udpPortList[port] == true;

        //internal static void AddUdpPort(int port)
        //{
        //    if (_udpPortList.ContainsKey(port))
        //    {
        //        _udpPortList[port] = true;
        //    }
        //    else
        //    {
        //        _udpPortList.Add(port, true);
        //    }
        //}

        //internal static void RemoveUpdPort(int port)
        //{
        //    if (_udpPortList.ContainsKey(port))
        //    {
        //        _udpPortList[port] = false;
        //    }
        //}
    }
}