using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected void CheckCommunicationInterface(ICommunicationInterface communicationInterface)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }
    }
}
