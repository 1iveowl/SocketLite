using System;
using System.Collections.Generic;
using ISocketLite.PCL.Interface;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected static readonly HashSet<Type> NativeSocketExceptions = new HashSet<Type> { typeof(System.Net.Sockets.SocketException) };
        protected void CheckCommunicationInterface(ICommunicationInterface communicationInterface)
        {
            if (communicationInterface != null && !communicationInterface.IsUsable)
            {
                throw new InvalidOperationException("Cannot listen on an unusable communication interface.");
            }
        }
    }
}
