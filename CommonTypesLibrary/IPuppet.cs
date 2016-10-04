using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypesLibrary
{
    using Milliseconds = Int32;

    public interface IPuppet
    {
        void Start();
        void Interval(Milliseconds milliseconds);
        void Status();
        void Crash();
        void Freeze();
        void Unfreeze();
    }
}
