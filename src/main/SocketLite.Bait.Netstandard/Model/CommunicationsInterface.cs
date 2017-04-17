using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using static SocketLite.Helper.Helper;

namespace SocketLite.Model
{
    public class CommunicationsInterface : ICommunicationInterface
    {
        public string NativeInterfaceId { get; } = null;
        public string Name { get; } = null;
        public string IpAddress { get; } = null;
        public string GatewayAddress { get; } = null;
        public string BroadcastAddress { get; } = null;
        public CommunicationConnectionStatus ConnectionStatus { get; } = CommunicationConnectionStatus.Testing;
        public bool IsUsable { get; } = false;
        public bool IsLoopback { get; } = false;
        //public bool IsInternetConnected { get; } = false;

        public IEnumerable<ICommunicationInterface> GetAllInterfaces()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
