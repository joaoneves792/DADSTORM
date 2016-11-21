using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;
    using NewTimestamp = Int32;

    public interface EpochChange
    {
        void Trust(Process process);

        //Indicator
        //void Deliver(Process process, Message message);
        //void Deliver(Process process, Tuple<NewEpoch, NewTimestamp> message);
        //void Deliver(Process process, Nack nack);
    }
}
