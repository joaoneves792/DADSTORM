using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public interface BestEffortBroadcast
    {
        void Broadcast(Message message);
        void Deliver(Process process, Message message);
        void Connect(Process process);
    }
}
