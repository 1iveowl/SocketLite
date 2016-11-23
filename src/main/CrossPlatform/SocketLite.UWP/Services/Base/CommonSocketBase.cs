using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected CommonSocketBase()
        {
            
        }
        protected void CheckCommunicationInterface(ICommunicationInterface communicationInterface)
        {
            if (communicationInterface != null && !communicationInterface.IsUsable)
            {
                throw new InvalidOperationException("Cannot listen on an unusable communication interface.");
            }
        }
    }
}
