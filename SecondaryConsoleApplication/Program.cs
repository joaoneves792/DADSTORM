using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DistributedAlgoritmsClassLibrary;

namespace SecondaryConsoleApplication
{
    using Message = Object;

    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();

            Process process1 = new Process(args[0], args[1]),
                    process2 = new Process(args[2], args[3]),
                    process3 = new Process(args[4], args[5]);

            //if (args[0].Equals("Teste1"))
            //{
            //    Thread.Sleep(20000);
            //}

            //BestEffortBroadcast broadcast = new BasicBroadcast(process1, listener.Deliver/*, process2, process3*/);
            //EventualLeaderDetector detector = new MonarchicalEventualLeaderDetection(process1, listener.Trust, process2, process3);
            Console.WriteLine(args[0]);

            //UniformConsensus paxos = new LeaderDrivenConsensus(process1, 3, listener.Decide, process2, process3);

            EpochChange change = new LeaderBasedEpochChange(process1, process1, listener.StartEpoch, process2, process3);

            //FairLossPointToPointLink link1 = new RemotingNode(process1, listener.Deliver1, process2, process3);
            //FairLossPointToPointLink link2 = new RemotingNode(process1, listener.Deliver2, process2, process3);

            Thread.Sleep(1000);

            //link1.Send(process2, "test1");
            //link2.Send(process2, "test1");

            //if (!args[0].Equals("Teste1"))
            //{
            //    Thread.Sleep(20000);
            //}
            //else {

            //}

            //if (args[0].Equals("Teste1"))
            //{
            //    IList<String> proposal = new List<String>();
            //    proposal.Add("a");
            //    proposal.Add("b");
            //    proposal.Add("c");
            //    paxos.Propose(proposal);
            //}

            //broadcast.Connect(process2);
            //broadcast.Connect(process3);

            //Thread.Sleep(1000);

            //broadcast.Broadcast("to " + process2.Name);

            Console.ReadLine();
        }

        internal class Listener
        {
            internal void Deliver1(Process process, Message message)
            {
                Console.WriteLine("[1] From " + process.Name + " " + (String)message);
            }

            internal void Deliver2(Process process, Message message)
            {
                Console.WriteLine("[2] From " + process.Name + " " + (String)message);
            }

            internal void Trust(Process process)
            {
                Console.WriteLine("The King has died. Long live the King " + process.Name + "!");
            }

            internal void Decide(IList<String> tuples) {
                Console.WriteLine(String.Join(" - ", tuples));
            }

            internal void StartEpoch(int timestamp, Process leader) {
                Console.WriteLine("Timestamp: " + timestamp);
                Console.WriteLine("Leader:    " + leader);
            }

            internal void Suspect(Process process) {
                Console.WriteLine("Suspect " + process.Name);
            }

            internal void Restore(Process process) {
                Console.WriteLine("Restore " + process.Name);
            }
        }
    }
}
