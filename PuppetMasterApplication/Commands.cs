using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

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
        private IDictionary<OperatorId, IList<Url>> _operatorResolutionCache;
        private IDictionary<Url, IProcessCreationService> _processCreationServiceTable;

        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            _operatorResolutionCache = new Dictionary<OperatorId, IList<Url>>();
            _processCreationServiceTable = new Dictionary<Url, IProcessCreationService>();

            String processCreationServiceUrl = "tcp://localhost:10000/";

            Console.WriteLine(processCreationServiceUrl + ProcessCreationService.SERVICE_NAME);

            IProcessCreationService processCreationService = (IProcessCreationService)Activator.GetObject(
                typeof(IProcessCreationService),
                processCreationServiceUrl + ProcessCreationService.SERVICE_NAME);

            _processCreationServiceTable.Add(processCreationServiceUrl, processCreationService);

            processCreationService.Ping();
        }


        private void ExecuteOperatorIdCommand(
            OperatorId operatorId,
            InputOps inputOps,
            RepFact repFact,
            Routing routing,
            Address addresses,
            OperatorSpec operatorSpec) {
            IList<Url> urlList;

            MatchCollection inputOpList = new Regex(GROUP_INPUT_OP, RegexOptions.Compiled).Matches(inputOps),
                            addressList = new Regex(GROUP_URL, RegexOptions.Compiled).Matches(addresses),
                            operatorSpecList = new Regex(GROUP_OPERATOR_SPEC, RegexOptions.Compiled).Matches(operatorSpec);

            //Organize source list
            String sources = "", inputOp;
            foreach (Match inputOpMatch in inputOpList) {
                inputOp = inputOpMatch.Value;
                if (_operatorResolutionCache.TryGetValue(inputOp, out urlList)) {
                    //FIXME: after checkpoint
                    sources += urlList.First() + ",";
                } else {
                    sources += inputOp + ",";
                }
            }
            if (!sources.Equals("")) {
                sources = sources.Remove(sources.Length - 1, 1);
            }
            Console.WriteLine("Sources:    " + sources);

            //Organize replica list
            String replicas = "";
            foreach (Match address in addressList) {
                replicas += address.Value + ",";
            }
            if (!replicas.Equals("")) {
                replicas = replicas.Remove(replicas.Length - 1, 1);
            }
            Console.WriteLine("Replicas:   " + replicas);

            //Organize operator spec list
            String operatorSpecs = "";
            foreach (Match operatorSpecField in operatorSpecList) {
                operatorSpecs += operatorSpecField.Value + ",";
            }
            if (!operatorSpecs.Equals("")) {
                operatorSpecs = operatorSpecs.Remove(operatorSpecs.Length - 1, 1);
            }
            Console.WriteLine("Operator:   " + operatorSpecs);

            foreach (Match address in addressList) {
                GroupCollection groupCollection = new Regex(URL_ADDRESS, RegexOptions.Compiled).Match(address.Value).Groups;

                Console.WriteLine("a");
                //Create process
                String processCreationServiceUrl = groupCollection[1].Value + ":10000/";
                _processCreationServiceTable[processCreationServiceUrl].CreateProcess(
                    operatorId + " " + groupCollection[0].Value + " " + sources + " " + replicas + " " + operatorSpecs);
                Console.WriteLine("b");

                //Add operator id into operator resolution cache
                if (_operatorResolutionCache.TryGetValue(operatorId, out urlList)) {
                    urlList.Add(groupCollection[0].Value);
                    _operatorResolutionCache.Remove(operatorId);
                    _operatorResolutionCache.Add(operatorId, urlList);
                } else {
                    urlList = new List<Url>();
                    urlList.Add(groupCollection[0].Value);
                    _operatorResolutionCache.Add(operatorId, urlList);
                }
            }
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
            foreach (IProcessCreationService service in _processCreationServiceTable.Values) {
                try {
                    //TODO: Uncomment me
                    //service.CloseProcesses();
                }
                catch (Exception) { }
            }

            _operatorResolutionCache = new Dictionary<OperatorId, IList<Url>>();
            _processCreationServiceTable = new Dictionary<Url, IProcessCreationService>();
        }
    }
}
