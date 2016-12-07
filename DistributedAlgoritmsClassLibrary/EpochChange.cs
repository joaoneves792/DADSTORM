using System;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using NewTimestamp = Int32;

    public interface EpochChange
    {
        void Trust(Process process);

        //Indicator
        //void Deliver(Process process, Message message);
        //void Deliver(Process process, Tuple<Signal, NewTimestamp> message);
        //void Deliver(Process process, Signal nack);
    }
}
