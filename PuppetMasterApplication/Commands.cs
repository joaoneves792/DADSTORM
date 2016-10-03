using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProcessCreationServiceApplication;

namespace PuppetMasterApplication
{
    //Aliases
    using Url = String;
    using OperatorId = String;
    using Milliseconds = String;
    using ProcessName = String;
    using InputOps = String;
    using RepFact = String;
    using Routing = String;
    using Address = String;
    using OperatorSpec = String;

    internal partial class PuppetMaster
    {
        //Tables
        private IDictionary<OperatorId, Url> operatorResolutionCache;
        private IDictionary<ProcessName, Url> processResolutionCache;
        private IDictionary<Url, IProcessCreationService> processCreationServiceTable;

        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            operatorResolutionCache = new Dictionary<OperatorId, Url>();
            processCreationServiceTable = new Dictionary<Url, IProcessCreationService>();
        }


        private void ExecuteOperatorIdCommand(
            OperatorId operatorId,
            InputOps inputOps,
            RepFact repFact,
            Routing routing,
            Address address,
            OperatorSpec operatorSpec) {
            System.Console.WriteLine("ExecuteOperatorIdCommand: " + operatorId + " : " + inputOps + " : " + repFact + " : " + routing + " : " + address + " : " + operatorSpec);
        }

        private void ExecuteStartCommand(OperatorId operatorId) {
            System.Console.WriteLine("ExecuteStartCommand: " + operatorId);
        }

        private void ExecuteIntervalCommand(OperatorId operatorId, Milliseconds milliseconds) {
            System.Console.WriteLine("ExecuteIntervalCommand: " + operatorId + " : " + milliseconds);
        }

        private void ExecuteStatusCommand() {
            System.Console.WriteLine("ExecuteStatusCommand");
        }

        private void ExecuteCrashCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteCrashCommand: " + processName);
        }

        private void ExecuteFreezeCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteFreezeCommand: " + processName);
        }

        private void ExecuteUnfreezeCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteUnfreezeCommand: " + processName);
        }

        private void ExecuteWaitCommand(Milliseconds milliseconds) {
            //TODO: double-check this
            System.Threading.Thread.Sleep(Int32.Parse(milliseconds));
            System.Console.WriteLine("Executed Wait Command: " + milliseconds);
        }


        //<summary>
        // Close processes
        //</summary>
        internal void CloseProcesses(object sender, ConsoleCancelEventArgs args) {
            CloseProcesses();
        }

        //<summary>
        // Close processes
        //</summary>
        internal void CloseProcesses() {
            foreach (IProcessCreationService service in processCreationServiceTable.Values) {
                try {
                    //TODO: Uncomment me
                    //service.CloseProcesses();
                }
                catch (Exception) { }
            }

            operatorResolutionCache = new Dictionary<OperatorId, Url>();
            processCreationServiceTable = new Dictionary<Url, IProcessCreationService>();
        }
    }
}
