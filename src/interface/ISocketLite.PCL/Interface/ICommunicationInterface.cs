using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Model;

namespace ISocketLite.PCL.Interface
{
    public interface ICommunicationInterface
    {
        string NativeInterfaceId { get; }

        string Name { get; }

        string IpAddress { get; }

        string GatewayAddress { get; }

        string BroadcastAddress { get; }

        CommunicationConnectionStatus ConnectionStatus { get; }

        bool IsUsable { get; }

        bool IsLoopback { get; }

        //bool IsInternetConnected { get; }

        IEnumerable<ICommunicationInterface> GetAllInterfaces();
    }
}
