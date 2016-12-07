using System;

namespace DistributedAlgoritmsClassLibrary
{
    using Message = Object;

    public interface PointToPointLink
    {
        void Connect(Process process);

        void Send(Process process, Message message);
        void Deliver(Process process, Message message);
    }
}
