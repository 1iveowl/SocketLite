using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketMulticastClient : IDisposable
    {
        #region Obsolete

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        IObservable<IUdpMessage> ObservableMessages { get; }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port, ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false
        );

        #endregion

        Task<IObservable<IUdpMessage>> ObservableMulticastListener(
            string multicastAddress,
            int port, 
            ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false);


        int Port { get; }
        string IpAddress { get; }

        IEnumerable<string> MulticastMemberShips { get; }

        bool IsMulticastActive { get; }

        void MulticastAddMembership(string ipLan, string mcastAddress);

        void MulticastDropMembership(string ipLan, string mcastAddress);

        void Disconnect();

        Task SendMulticastAsync(byte[] data);

        Task SendMulticastAsync(byte[] data, int length);

        int TTL { get; set; }
    }
}
