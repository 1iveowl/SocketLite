using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Exceptions
{
    public sealed class SocketException : Exception
    {
        public SocketException(Exception innerException) : base(innerException.Message, innerException)
        {

        }
    }
}
