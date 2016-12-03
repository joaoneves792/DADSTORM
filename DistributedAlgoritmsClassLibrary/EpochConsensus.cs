using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    public interface EpochConsensus<Value>
    {
        void Propose(Value value);
        void Abort();
    }
}
