using System;

using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketReceiver : IDisposable
    {
        int Port { get; }

        IObservable<IUdpMessage> ObservableMessages { get; }

        Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort);

        void StopListening();

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);

        
    }
}
