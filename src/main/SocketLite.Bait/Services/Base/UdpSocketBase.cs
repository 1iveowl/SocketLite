using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using SocketLite.Model;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : CommonSocketBase
    {

        public virtual Task SendAsync(byte[] data)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public virtual Task SendAsync(byte[] data, int length)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public virtual Task SendToAsync(byte[] data, string address, int port)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public virtual Task SendToAsync(byte[] data, int length, string address, int port)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
