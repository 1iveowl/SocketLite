using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        public IObservable<ITcpSocketClient> ObservableTcpSocket { get; } = null;

        public Task<IObservable<ITcpSocketClient>> CreateObservableListener(int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            throw new NotImplementedException();
        }

        public int LocalPort => 0;

        public Task StartListeningAsync(
            int port, 
            ICommunicationInterface communicationInterface,
            bool allowMultipleBindToSamePort = false)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void StopListening()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
