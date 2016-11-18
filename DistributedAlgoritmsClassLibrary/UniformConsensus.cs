using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    using Value = IList<String>;

    public interface UniformConsensus
    {
        void Propose(Value value);
    }
}
