using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Value = IList<String>;

    public interface EpochConsensus
    {
        void Propose(Value value);
        void Abort();
    }
}
