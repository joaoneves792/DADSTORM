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
        private const String SERVICE_NAME = "puppet",
                                START = @"^\ *",
                                END = @"\ *$",
                                SPACE = @"\ +",
                                COMMA = @"\ *\,\ *",
                                DIR = @"(?:\\|\/)",

                                INT = @"\d+",
                                GROUP_INT = @"(" + INT + @")",
                                URL = @"tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+",
                                GROUP_URL = @"(" + URL + @")",
                                OPERATOR_ID = @"(?:" + INT + @")",

                                PARTITION = "(?:[A-Z]:" + DIR + @")?",
                                DIRNAME = @"(?:.+" + DIR + @")*",
                                FILENAME = @".+" + DIR + @"?",
                                PATH = @"(?:" + PARTITION + DIRNAME + FILENAME + @")",

                                INPUT_OP = @"(?:" + OPERATOR_ID + @"|" + PATH + @")",

                                UNIQ = @"(?:UNIQ\ +-?\d+)",
                                COUNT = @"(?:COUNT)",
                                DUP = @"(?:DUP)",
                                FILTER = @"(?:FILTER\ +-?\d+\ *\,\ *(?:>|<|=)\ *\,\ *-?\d+)",
                                CUSTOM = @"(?:CUSTOM\ +\w+\.dll\ *\,\ *\w+\ *\,\ *\w+)",

                                OPERATOR_ID_COMMAND =   START + GROUP_INT + SPACE +
                                                        @"INPUT_OPS" + SPACE + @"((?:" + INPUT_OP + COMMA + @")*" + INPUT_OP + @")" + SPACE +
                                                        @"REP_FACT" + SPACE + GROUP_INT + SPACE +
                                                        @"ROUTING" + SPACE + @"((?:primary)|(?:hashing)|(?:random))" + SPACE +
                                                        @"ADDRESS" + SPACE + @"((?:" + URL + COMMA + @")*" + URL + @")" + SPACE +
                                                        @"OPERATOR_SPEC" + SPACE + @"(" + UNIQ + @"|" +
                                                                                          COUNT + @"|" +
                                                                                          DUP + @"|" +
                                                                                          FILTER + @"|" +
                                                                                          CUSTOM + @")" + END,
                                START_COMMAND = START + @"Start" + SPACE + GROUP_INT + END,
                                INTERVAL_COMMAND = START + @"Interval" + SPACE + GROUP_INT + SPACE + GROUP_INT + END,
                                STATUS_COMMAND = START + @"Status" + END,
                                CRASH_COMMAND = START + @"Crash" + SPACE + GROUP_URL + END,
                                FREEZE_COMMAND = START + @"Freeze" + SPACE + GROUP_URL + END,
                                UNFREEZE_COMMAND = START + @"Unfreeze" + SPACE + GROUP_URL + END,
                                WAIT_COMMAND = START + @"Wait" + SPACE + GROUP_INT + END;
    }
}
