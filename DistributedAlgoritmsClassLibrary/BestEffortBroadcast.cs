using System;
using System.Collections.Generic;

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
