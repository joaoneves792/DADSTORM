using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterLibrary {

	// internal -> public
	public partial class PuppetMaster {
        //Constants
        private const int PORT = 10001;
        private const String SERVICE_NAME = "PuppetMaster",
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
                                WORD = @"\w+",
                                GROUP_INT = GROUP_OPEN + INT + CLOSE,
                                GROUP_OPERATOR_ID = GROUP_OPEN + WORD + CLOSE,
                                URL = @"tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+",
                                URL_ADDRESS = @"(tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost))):\d{1,5}\/\w+",
                                GROUP_URL = GROUP_OPEN + URL + CLOSE,
                                OPERATOR_ID = @"(" + WORD + CLOSE,

                                PARTITION = "(?:[A-Z]:" + DIR + @")?",
                                DIRNAMES = @"(?:.+" + DIR + @")*",
                                FILENAME = @".+" + DIR + @"?",
                                PATH = OPEN + PARTITION + DIRNAMES + FILENAME + CLOSE,

                                INPUT_OP = OPEN + @"(?:" + WORD + @")" + OR + PATH + CLOSE,
                                //GROUP_INPUT_OP = @"(\w+)|((?:[A-Z]:(?:\\|\/))?(?:[\w.]+(?:\\|\/))*[\w.]+(?:\\|\/)?)",
                                GROUP_INPUT_OP = @"((?:\w+)|(?:(?:[A-Z]:(?:\\|\/))?(?:[\w.]+(?:\\|\/))*[\w.]+(?:\\|\/)?))(?:(?:\ ?,\ )|$)",

                                QUOTE = "(\"[^\"]+\")",
                                GROUP_QUOTE = "\"([^\"]+)\"",
                                GROUP_QUOTE_DLL = "\"([^\"]+).dll\"",

                                UNIQ = @"(?:UNIQ" + SPACE + @"-?\d+)",
                                COUNT = @"(?:COUNT)",
                                DUP = @"(?:DUP)",
                                FILTER = @"(?:FILTER" + SPACE + @"-?\d+" + COMMA + @"(?:>|<|=)" + COMMA + @"-?""?(\w|.)+""?)",
                                //CUSTOM = @"(?:CUSTOM" + SPACE + @"\w+\.dll" + COMMA + WORD + COMMA + WORD + @")",
                                CUSTOM = @"(?:CUSTOM" + SPACE + QUOTE + COMMA + QUOTE + COMMA + QUOTE + @")",

                                GROUP_UNIQ = @"(?:(UNIQ)" + SPACE + @"(-?\d+))",
                                GROUP_COUNT = @"(COUNT)",
                                GROUP_DUP = @"(DUP)",
                                GROUP_FILTER = @"(?:(FILTER)" + SPACE + @"(-?\d+)" + COMMA + @"(>|<|=)" + COMMA + @"(-?""?(\w|.)+""?))",
                                //GROUP_CUSTOM = @"(?:(CUSTOM)" + SPACE + @"(\w+\.dll)" + COMMA + @"(\w)" + COMMA + @"(\w)" + @")",
                                GROUP_CUSTOM = @"(?:(CUSTOM)" + SPACE + GROUP_QUOTE_DLL + COMMA + GROUP_QUOTE + COMMA + GROUP_QUOTE + @")",

                                INPUT_OPS = @"(?:" + @"INPUT_OPS" + OR + @"input ops" + CLOSE,
                                REP_FACT = @"(?:" + @"REP_FACT" + OR + @"rep fact" + CLOSE,
                                ROUTING = @"(?:" + @"ROUTING" + OR + @"routing" + CLOSE,
                                ADDRESS = @"(?:" + @"ADDRESS" + OR + @"address" + CLOSE, 
                                OPERATOR_SPEC = @"(?:" + @"OPERATOR_SPEC" + OR + @"operator spec" + CLOSE,
            
                                GROUP_OPERATOR_SPEC = OPEN + GROUP_UNIQ + OR +
                                                      GROUP_COUNT + OR +
                                                      GROUP_DUP + OR +
                                                      GROUP_FILTER + OR +
                                                      GROUP_CUSTOM + CLOSE,

                                OPERATOR_ID_COMMAND =   START + OPERATOR_ID + SPACE +
                                                        INPUT_OPS + SPACE + @"((?:" + INPUT_OP + COMMA + @")*" + INPUT_OP + CLOSE + SPACE +
                                                        REP_FACT + SPACE + GROUP_INT + SPACE +
                                                        ROUTING + SPACE + @"((?:primary)|(?:hashing\(\d+\))|(?:random))" + SPACE +
                                                        ADDRESS + SPACE + @"((?:" + URL + COMMA + @")*" + URL + CLOSE + SPACE +
                                                        OPERATOR_SPEC + SPACE + GROUP_OPEN + UNIQ + OR +
                                                                                                COUNT + OR +
                                                                                                DUP + OR +
                                                                                                FILTER + OR +
                                                                                                CUSTOM + CLOSE + END,
                                START_COMMAND = START + @"(?:S|s)tart" + SPACE + GROUP_OPERATOR_ID + END,
                                INTERVAL_COMMAND = START + @"(?:I|i)nterval" + SPACE + GROUP_OPERATOR_ID + SPACE + GROUP_INT + END,
                                STATUS_COMMAND = START + @"(?:S|s)tatus" + END,
                                CRASH_COMMAND = START + @"(?:C|c)rash" + SPACE + GROUP_OPERATOR_ID + SPACE + GROUP_INT + END,
                                FREEZE_COMMAND = START + @"(?:F|f)reeze" + SPACE + GROUP_OPERATOR_ID + SPACE + GROUP_INT + END,
                                UNFREEZE_COMMAND = START + @"(?:U|u)nfreeze" + SPACE + GROUP_OPERATOR_ID + SPACE + GROUP_INT + END,
                                WAIT_COMMAND = START + @"(?:W|w)ait" + SPACE + GROUP_INT + END,
                                SEMANTICS_COMMAND = START + @"(?:S|s)emantics" + SPACE + "((?:at-most-once)|(?:at-least-once)|(?:exactly-once))" + END,
                                LOGGING_LEVEL_COMMAND = START + @"(?:L|l)ogging(?:L|l)evel" + SPACE + "((?:full)|(?:light))" + END;
    }
}
