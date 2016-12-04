using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DistributedAlgoritmsClassLibrary;
using CommonTypesLibrary;

namespace SecondaryConsoleApplication
{
    using Message = Object;
    using Value = IList<String>;
    using Timestamp = Int32;
    using TupleMessage = IList<String>;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Serialization.Formatters;
    using System.Collections;
    using System.Runtime.Remoting.Channels.Tcp;

    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();

            Process process1 = new Process(args[0], args[1]),
                    process2 = new Process(args[2], args[3]),
                    process3 = new Process(args[4], args[5]);

            Console.WriteLine(args[0]);

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["port"] = process1.Port;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            //BestEffortBroadcast broadcast = new BasicBroadcast(process1, listener.Deliver/*, process2, process3*/);
            //EventualLeaderDetector detector = new MonarchicalEventualLeaderDetection(process1, listener.Trust, process2, process3);
            //EpochChange change = new LeaderBasedEpochChange(process1, process1, listener.StartEpoch, process2, process3);
            //EpochConsensus<TupleMessage> consensus = new ReadWriteEpochConsensus<TupleMessage>(
            //    process1,
            //    new Tuple<Timestamp, Value>(0, null),
            //    3,
            //    0,
            //    listener.Decide,
            //    listener.Aborted,
            //    process2,
            //    process3
            //);

            //UniformConsensus<TupleMessage> consensus = new LeaderDrivenConsensus<TupleMessage>(
            //    process1,
            //    3,
            //    listener.Decide,
            //    process2,
            //    process3
            //);

            UniformConsensus<TupleMessage> consensus = new FloodingUniformConsensus<TupleMessage>(
                process1,
                listener.Decide,
                process2,
                process3
            );

            Thread.Sleep(5000);

            if (args[0].Equals("Teste1")) {
                TupleMessage proposal = new List<String>();
                proposal.Add("a");
                proposal.Add("b");
                proposal.Add("c");
                consensus.Propose(proposal);
            }

            // else {
            //    Flag.Frozen = true;
            //IList<String> proposal2 = new List<String>();
            //proposal2.Add("d");
            //proposal2.Add("e");
            //proposal2.Add("f");
            //quorumConsensus.Propose(proposal2);
            //}
            //Thread.Sleep(100000);
            //Flag.Frozen = false;

            //Console.ReadLine();
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

            internal void Decide(Value tuples) {
                Console.WriteLine("Decided: " + String.Join(" - ", tuples));
            }

            internal void StartEpoch(int timestamp, Process leader) {
                Console.WriteLine("Timestamp:\n" + timestamp);
                Console.WriteLine("Leader:\n" + leader);
            }

            internal void Suspect(Process process) {
                Console.WriteLine("Suspect " + process.Name);
            }

            internal void Restore(Process process) {
                Console.WriteLine("Restore " + process.Name);
            }

            public void Aborted (Tuple<Timestamp, Value> state) {
                Console.WriteLine("Aborted");
            }
        }
    }
}
