using System;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketListener : IDisposable
    {
        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        IObservable<ITcpSocketClient> ObservableTcpSocket { get; }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationEntity,
            bool allowMultipleBindToSamePort);

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        void StopListening();

        Task<IObservable<ITcpSocketClient>> CreateObservableListener(
            int port,
            ICommunicationInterface listenOn = null,
            bool allowMultipleBindToSamePort = false);

        int LocalPort { get; }
        
    }
}
