using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAlgoritmsClassLibrary
{
    public enum Signal {
        ACCEPT,
        DECIDED,
        HEARTBEAT_REPLY,
        HEARTBEAT_REQUEST,
        NACK,
        NEW_EPOCH,
        READ,
        STATE,
        WRITE
    }
}
