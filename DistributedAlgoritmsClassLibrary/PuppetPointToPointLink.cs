using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    public interface PuppetPointToPointLink
    {
        void Crash();
        void Recover();
        void Freeze();
        void Unfreeze();
    }
}
