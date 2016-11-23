using System;

using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketReceiver : IDisposable
    {
        IObservable<IUdpMessage> ObservableMessages { get; }

        Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort = false);

        void StopListening();

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);

        
    }
}
