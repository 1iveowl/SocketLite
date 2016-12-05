using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketMulticastClient : IDisposable
    {
        int Port { get; }
        string IpAddress { get; }

        IObservable<IUdpMessage> ObservableMessages { get; }

        Task JoinMulticastGroupAsync(
            string multicastAddress, 
            int port, ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort = false);

        void Disconnect();

        Task SendMulticastAsync(byte[] data);

        Task SendMulticastAsync(byte[] data, int length);

        int TTL { get; set; }

        
    }
}
