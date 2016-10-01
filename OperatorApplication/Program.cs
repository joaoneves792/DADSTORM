using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = Int32.Parse(args[0]);
            //The rest of the args are the urls of the replicas for this operator

            Console.WriteLine("Hello! Running on port:"+ port);

			bool end = false;
			string str = "";
			Command command = null;

			while (!end) {
				str = Console.ReadLine();
				switch (str) {
						case "quit":
						end = true;
						break;
					case "UNIQ":
						command = new UNIQCommand();
						break;
					case "COUNT":
						command = new COUNTCommand();
						break;
					case "DUP":
						command = new DUPCommand();
						break;
					case "FILTER":
						command = new FILTERCommand();
						break;
					case "CUSTOM":
						command = new CUSTOMCommand();
						break;
					default:
						Console.WriteLine("unrecognised.");
						break;
				}
			}

		}
    }
}
