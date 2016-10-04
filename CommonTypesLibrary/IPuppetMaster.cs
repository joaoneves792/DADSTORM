using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Remoting;

namespace CommonTypesLibrary
{
    public interface IPuppetMaster
    {
        void ReceiveUrl(ObjRef objRef);
    }
}
