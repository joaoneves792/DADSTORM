using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Remoting;

namespace CommonTypesLibrary
{
    using Url = String;

    public interface IPuppetMaster
    {
        void ReceiveUrl(Url url, ObjRef objRef);
    }
}
