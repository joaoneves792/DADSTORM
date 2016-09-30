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


        private void ExecuteOperatorIdCommand() {
            System.Console.WriteLine("ExecuteOperatorIdCommand");
        }

        private void ExecuteStartCommand(OperatorId operatorId) {
            System.Console.WriteLine("ExecuteStartCommand");
        }

        private void ExecuteIntervalCommand(OperatorId operatorId, Milliseconds milliseconds) {
            System.Console.WriteLine("ExecuteIntervalCommand");
        }

        private void ExecuteStatusCommand() {
            System.Console.WriteLine("ExecuteStatusCommand");
        }

        private void ExecuteCrashCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteCrashCommand");
        }

        private void ExecuteFreezeCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteFreezeCommand");
        }

        private void ExecuteUnfreezeCommand(ProcessName processName) {
            System.Console.WriteLine("ExecuteUnfreezeCommand");
        }

        private void ExecuteWaitCommand(Milliseconds milliseconds) {
            System.Console.WriteLine("ExecuteWaitCommand");
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
