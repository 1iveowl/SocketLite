using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketMulticastClient : IDisposable
    {
        int Port { get; }
        string IpAddress { get; }

        IEnumerable<string> MulticastMemberShips { get; }

        bool IsMulticastInterfaceActive { get; }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        IObservable<IUdpMessage> ObservableMessages { get; }

        Task<IObservable<IUdpMessage>> CreateObservableMultiCastListener(
            string multicastAddress,
            int port, ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false);

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port, ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false
        );

        

        void MulticastAddMembership(string ipLan, string mcastAddress);

        void MulticastDropMembership(string ipLan, string mcastAddress);

        void Disconnect();

        Task SendMulticastAsync(byte[] data);

        Task SendMulticastAsync(byte[] data, int length);

        int TTL { get; set; }

        
    }
}
