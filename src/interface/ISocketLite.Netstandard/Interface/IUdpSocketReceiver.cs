using System;

using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface IUdpSocketReceiver : IDisposable
    {
        #region Obsolete

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        IObservable<IUdpMessage> ObservableMessages { get; }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        Task StartListeningAsync(
            int port,
            ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort);

        #endregion

        int Port { get; }
        
        bool IsUnicastActive { get; }

        bool IsMulticastActive { get; }

        Task<IObservable<IUdpMessage>> ObservableUnicastListener(
            int port = 0,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        void StopListening();

        Task SendToAsync(byte[] data, string address, int port);

        Task SendToAsync(byte[] data, int length, string address, int port);
    }
}
