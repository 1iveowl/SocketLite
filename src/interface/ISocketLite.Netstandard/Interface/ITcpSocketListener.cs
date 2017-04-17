using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace ISocketLite.PCL.Interface
{
    public interface ITcpSocketListener : IDisposable
    {
        IObservable<ITcpSocketClient> ObservableTcpSocket { get; }

        Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationEntity,
            bool allowMultipleBindToSamePort);

        void StopListening();

        int LocalPort { get; }
        
    }
}
