using DistributedAlgoritmsClassLibrary;
using System;
using System.Collections.Generic;

namespace SecondaryConsoleApplication
{
    using System.Collections;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Runtime.Serialization.Formatters;
    using Message = Object;
    using Timestamp = Int32;
    using TupleMessage = IList<string>;
    using Value = IList<string>;

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

            UniformConsensus<TupleMessage> consensus = new LeaderDrivenConsensus<TupleMessage>(
                process1,
                3,
                listener.Decide,
                listener.StartEpoch,
                new Process("Teste1", "tcp://localhost:53001/teste"),
                process2,
                process3
            );

            //UniformConsensus<TupleMessage> consensus = new FloodingUniformConsensus<TupleMessage>(
            //    process1,
            //    listener.Decide,
            //    process2,
            //    process3
            //);

            if (args[0].Equals("Teste1")) {
                TupleMessage proposal = new List<string>();
                proposal.Add("a");
                proposal.Add("b");
                proposal.Add("c");
                consensus.Propose(proposal);
            }
        }

        internal class Listener
        {
            internal void Deliver1(Process process, Message message)
            {
                Console.WriteLine("[1] From " + process.Name + " " + (string)message);
            }

            internal void Deliver2(Process process, Message message)
            {
                Console.WriteLine("[2] From " + process.Name + " " + (string)message);
            }

            internal void Trust(Process process)
            {
                Console.WriteLine("The King has died. Long live the King " + process.Name + "!");
            }

            internal void Decide(Value tuples) {
                Console.WriteLine("Decided: " + string.Join(" - ", tuples));
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
