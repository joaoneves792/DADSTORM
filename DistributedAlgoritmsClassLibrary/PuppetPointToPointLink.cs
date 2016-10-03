using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    public interface PuppetPointToPointLink
    {
        void Connect(Process process);

        void Crash();
        void Freeze();
        void Unfreeze();
    }
}
