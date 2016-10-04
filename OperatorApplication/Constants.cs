using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication
{
    partial class Operator
    {
        private const String URL = @"tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+",

                             DIR = @"(?:\\|\/)",
                             PARTITION = "(?:[A-Z]:" + DIR + @")?",
                             DIRNAME = @"(?:.+" + DIR + @")*",
                             FILENAME = @".+" + DIR + @"?",
                             PATH = @"(?:" + PARTITION + DIRNAME + FILENAME + @")";
    }
}
