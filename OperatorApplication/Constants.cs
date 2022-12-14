namespace OperatorApplication
{
    partial class Operator
    {
        private const string URL = @"tcp:\/\/(?:(?:(?:\d{1,3}\.){3}\d{1,3})|(?:localhost)):\d{1,5}\/\w+",

                             DIR = @"(?:\\|\/)",
                             PARTITION = "(?:[A-Z]:" + DIR + @")?",
                             DIRNAME = @"(?:.+" + DIR + @")*",
                             FILENAME = @".+" + DIR + @"?",
                             PATH = @"(?:" + PARTITION + DIRNAME + FILENAME + @")",
                             HASHING = @"hashing\((\d+)\)";
    }
}
