using System;

using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketReceiver : IDisposable
    {
        int Port { get; }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        IObservable<IUdpMessage> ObservableMessages { get; }

        Task<IObservable<IUdpMessage>> CreateObservableListener(
            int port = 0,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationInterface, 
            bool allowMultipleBindToSamePort);

        void StopListening();

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);

        
    }
}
