using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Interface
{
    public interface IExposeBackingSocket
    {
        object Socket { get; }
    }
}
