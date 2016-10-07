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
                                OR = @"|",
                                OPEN = @"(?:",
                                GROUP_OPEN = @"(",
                                CLOSE = @")",

                                INT = @"\d+",
                                GROUP_INT = GROUP_OPEN + INT + CLOSE,
                                URL = @"tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+",
                                URL_ADDRESS = @"(tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost))):\d{1,5}\/\w+",
                                GROUP_URL = GROUP_OPEN + URL + CLOSE,
                                OPERATOR_ID = OPEN + INT + CLOSE,

                                PARTITION = "(?:[A-Z]:" + DIR + @")?",
                                DIRNAME = @"(?:.+" + DIR + @")*",
                                FILENAME = @".+" + DIR + @"?",
                                PATH = OPEN + PARTITION + DIRNAME + FILENAME + CLOSE,

                                INPUT_OP = OPEN + OPERATOR_ID + OR + PATH + CLOSE,
                                GROUP_INPUT_OP = GROUP_OPEN + INPUT_OP + CLOSE,

                                UNIQ = @"(?:UNIQ" + SPACE + @"-?\d+)",
                                COUNT = @"(?:COUNT)",
                                DUP = @"(?:DUP)",
                                FILTER = @"(?:FILTER" + SPACE + @"-?\d+" + COMMA + @"(?:>|<|=)" + COMMA + @"-?\d+)",
                                CUSTOM = @"(?:CUSTOM" + SPACE + @"\w+\.dll" + COMMA + @"\w+" + COMMA + @"\w+)",

                                GROUP_OPERATOR_SPEC = @"(UNIQ)" + OR +
                                                      @"(COUNT)" + OR +
                                                      @"(DUP)" + OR +
                                                      @"(FILTER)" + OR +
                                                      @"(CUSTOM)" + OR +
                                                      @"(>)|(<)|(=)" + OR +
                                                      @"(-?\d+)"  + OR +
                                                      @"(\w+\.dll)" + OR +
                                                      @"(\w+)",

                                OPERATOR_ID_COMMAND =   START + GROUP_INT + SPACE +
                                                        @"INPUT_OPS" + SPACE + @"((?:" + INPUT_OP + COMMA + @")*" + INPUT_OP + CLOSE + SPACE +
                                                        @"REP_FACT" + SPACE + GROUP_INT + SPACE +
                                                        @"ROUTING" + SPACE + @"((?:primary)|(?:hashing)|(?:random))" + SPACE +
                                                        @"ADDRESS" + SPACE + @"((?:" + URL + COMMA + @")*" + URL + CLOSE + SPACE +
                                                        @"OPERATOR_SPEC" + SPACE + GROUP_OPEN + UNIQ + OR +
                                                                                                COUNT + OR +
                                                                                                DUP + OR +
                                                                                                FILTER + OR +
                                                                                                CUSTOM + CLOSE + END,
                                START_COMMAND = START + @"Start" + SPACE + GROUP_INT + END,
                                INTERVAL_COMMAND = START + @"Interval" + SPACE + GROUP_INT + SPACE + GROUP_INT + END,
                                STATUS_COMMAND = START + @"Status" + END,
                                CRASH_COMMAND = START + @"Crash" + SPACE + GROUP_URL + END,
                                FREEZE_COMMAND = START + @"Freeze" + SPACE + GROUP_URL + END,
                                UNFREEZE_COMMAND = START + @"Unfreeze" + SPACE + GROUP_URL + END,
                                WAIT_COMMAND = START + @"Wait" + SPACE + GROUP_INT + END;
    }
}
