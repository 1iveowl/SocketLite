using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Model;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected CommonSocketBase()
        {
            
        }
        protected void CheckCommunicationInterface(ICommunicationInterface communicationsInterface)
        {
            if (communicationsInterface != null && !communicationsInterface.IsUsable)
            {
                throw new InvalidOperationException("Cannot listen on an unusable communication interface.");
            }

            if (communicationsInterface == null) //Try and find best possible Network interface.
            {
                var newCommunicationsInterface = new CommunicationsInterface();
                var allInterfaces = newCommunicationsInterface.GetAllInterfaces();
                if (allInterfaces != null && allInterfaces.Any())
                {
                    var firstUsableCommunicationInterface = allInterfaces.FirstOrDefault(x => x.IsUsable && !x.IsLoopback && x.GatewayAddress != null);
                    if (firstUsableCommunicationInterface == null)
                    {
                        communicationsInterface = allInterfaces.FirstOrDefault(x => x.IsUsable);
                    }
                }
            }
        }
    }
}
