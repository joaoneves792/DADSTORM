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
        IList<Process> Processes { get; }

        void Broadcast(Message message);

        //Indicator
        //void Deliver(Process process, Message message);
    }
}
