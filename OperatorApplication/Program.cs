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
            Console.ReadKey();
        }
    }
}
