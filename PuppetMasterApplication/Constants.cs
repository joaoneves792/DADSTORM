using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterApplication
{
    internal partial class PuppetMaster
    {
        //Constants
        private const int PORT = 10001;
        private const String    SERVICE_NAME = "puppet",

                                OPERATOR_ID_COMMAND = @"^\ *(\d+)\ +INPUT_OPS\ +((?:(?:(?:\d+)|(?:(?:[A-Z]:(?:\\|\/))?(?:.+(?:\\|\/))*.+(?:\\|\/)?))\ *\,\ *)*(?:(?:\d+)|(?:(?:[A-Z]:(?:\\|\/))?(?:.+(?:\\|\/))*.+(?:\\|\/)?)))\ +REP_FACT\ +(\d+)\ +ROUTING\ +((?:primary)|(?:hashing)|(?:random))\ +ADDRESS\ +((?:tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+\ *\,\ *)*tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+)\ +OPERATOR_SPEC\ +((?:UNIQ\ +-?\d+)|(?:COUNT)|(?:DUP)|(?:FILTER\ +-?\d+\ *\,\ *(?:>|<|=)\ *\,\ *-?\d+)|(?:CUSTOM\ +\w+\.dll\ *\,\ *\w+\ *\,\ *\w+))\ *$",
                                START_COMMAND = @"^\ *Start\ +(\d+)\ *$",
                                INTERVAL_COMMAND = @"^\ *Interval\ +(\d+)\ +(\d+)\ *$",
                                STATUS_COMMAND = @"^\ *Status\ *$",
                                CRASH_COMMAND = @"^\ *Crash\ +(tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+)\ *$",
                                FREEZE_COMMAND = @"^\ *Freeze\ +(tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+)\ *$",
                                UNFREEZE_COMMAND = @"^\ *Unfreeze\ +(tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+)\ *$",
                                WAIT_COMMAND = @"^\ *Wait\ +(\d+)\ *$";
    }
}
