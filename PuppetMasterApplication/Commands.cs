using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.IO;

using ProcessCreationServiceApplication;
using CommonTypesLibrary;

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

    internal partial class PuppetMaster : MarshalByRefObject, IPuppetMaster

    {
        //Tables
        private IDictionary<OperatorId, IList<Url>> _operatorResolutionCache;
        private IDictionary<Url, IPuppet> _puppetTable;
        private IDictionary<Url, IProcessCreationService> _processCreationServiceTable;

        //Constants
        private const String OPERATOR_NAME = "Operator";
        private const String PUPPET_NAME = "Puppet";

        private String _semantic, _loggingLevel;

        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            _operatorResolutionCache = new Dictionary<OperatorId, IList<Url>>();
            _puppetTable = new Dictionary<Url, IPuppet>();
            _processCreationServiceTable = new Dictionary<Url, IProcessCreationService>();

            _semantic = "at-most-once";
            _loggingLevel = "light";

            String processCreationServiceUrl = "tcp://localhost:10000/";

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary RemoteChannelProperties = new Hashtable();
            RemoteChannelProperties["name"] = SERVICE_NAME;
            RemoteChannelProperties["port"] = PORT;
            TcpChannel channel = new TcpChannel(RemoteChannelProperties, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                this,
                SERVICE_NAME,
                typeof(IPuppetMaster));

            IProcessCreationService processCreationService = (IProcessCreationService)Activator.GetObject(
                typeof(IProcessCreationService),
                processCreationServiceUrl + ProcessCreationService.SERVICE_NAME);

            _processCreationServiceTable.Add(processCreationServiceUrl, processCreationService);
        }

        public void ReceiveUrl(Url url, ObjRef objRef) {
            IPuppet puppet = (IPuppet)RemotingServices.Unmarshal(objRef);
            puppet.Semantics(_semantic);
            puppet.LoggingLevel(_loggingLevel);
            _puppetTable.Add(url, puppet);
        }

        public void Log(String message) {
            lock (this) {
                StreamWriter w = File.AppendText("log.txt");
                w.WriteLine(message);
                w.Flush();
                w.Close();

                Console.WriteLine(message);
            }
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
                            addressList = new Regex(GROUP_URL, RegexOptions.Compiled).Matches(addresses);
            //operatorSpecList = new Regex(GROUP_OPERATOR_SPEC, RegexOptions.Compiled).Matches(operatorSpec);

            GroupCollection operatorSpecList;
            Matches(GROUP_OPERATOR_SPEC, operatorSpec, out operatorSpecList);

            //Organize source list
            //Assumption: the process creation is made downstream-wise
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

            //Organize replica list
            String replicas = "";
            foreach (Match address in addressList) {
                replicas += address.Value + ",";
            }
            if (!replicas.Equals("")) {
                replicas = replicas.Remove(replicas.Length - 1, 1);
            }

            //Organize operator spec list
            String operatorSpecs = "";
            if (operatorSpecList[1].Value.Equals("UNIQ")) {
                operatorSpecs += operatorSpecList[1].Value + ",";
                operatorSpecs += operatorSpecList[2].Value;
            }
            if (operatorSpecList[3].Value.Equals("COUNT")) {
                operatorSpecs += operatorSpecList[3].Value;
            }
            if (operatorSpecList[4].Value.Equals("DUP")) {
                operatorSpecs += operatorSpecList[4].Value;
            }
            if (operatorSpecList[5].Value.Equals("FILTER")) {
                operatorSpecs += operatorSpecList[5].Value + ",";
                operatorSpecs += operatorSpecList[6].Value + ",";
                operatorSpecs += operatorSpecList[7].Value + ",";
                operatorSpecs += operatorSpecList[8].Value;
            }
            if (operatorSpecList[9].Value.Equals("CUSTOM")) {
                operatorSpecs += operatorSpecList[9].Value + ",";
                operatorSpecs += operatorSpecList[10].Value + ".dll,";
                operatorSpecs += operatorSpecList[10].Value + "." + operatorSpecList[11].Value + ",";
                operatorSpecs += operatorSpecList[12].Value;
            }

            foreach (Match address in addressList) {
                GroupCollection groupCollection = new Regex(URL_ADDRESS, RegexOptions.Compiled).Match(address.Value).Groups;

                //Create process
                String processCreationServiceUrl = groupCollection[1].Value + ":10000/";
                _processCreationServiceTable[processCreationServiceUrl].CreateProcess(
                    operatorId + " " + groupCollection[0].Value + " " + sources + " " + replicas + " " + operatorSpecs);


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
            Console.WriteLine("ExecuteStartCommand: " + operatorId);
            IList<Url> urlList = _operatorResolutionCache[operatorId];
            foreach (Url url in urlList) {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Start();
                });
            }
        }

        private void ExecuteIntervalCommand(OperatorId operatorId, Milliseconds milliseconds) {
            IList<Url> urlList = _operatorResolutionCache[operatorId];
            foreach(Url url in urlList) {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Interval(Int32.Parse(milliseconds));
                });
            }
            
        }

        private void ExecuteStatusCommand() {
            foreach (KeyValuePair<Url, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.Status();
                });
            }
        }

        private void ExecuteCrashCommand(ProcessName processName) {
            processName = processName.Replace(OPERATOR_NAME, PUPPET_NAME);

            IPuppet puppet = (IPuppet)Activator.GetObject(typeof(IPuppet), processName);
            Task.Run(() => {
                puppet.Crash();
            });

        }

        private void ExecuteFreezeCommand(ProcessName processName) {
            processName = processName.Replace(OPERATOR_NAME, PUPPET_NAME);

            IPuppet puppet = (IPuppet)Activator.GetObject(typeof(IPuppet), processName);
            Task.Run(() => {
                puppet.Freeze();
            });

        }

        private void ExecuteUnfreezeCommand(ProcessName processName) {
            processName = processName.Replace(OPERATOR_NAME, PUPPET_NAME);
            IPuppet puppet = (IPuppet)Activator.GetObject(typeof(IPuppet), processName);
            Task.Run(() => {
                puppet.Unfreeze();
            });

        }

        private void ExecuteWaitCommand(Milliseconds milliseconds) {
            //TODO: double-check this
            Thread.Sleep(Int32.Parse(milliseconds));
        }

        private void ExecuteSemanticsCommand(String semantic) {
            _semantic = semantic;

            foreach (KeyValuePair<Url, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.Semantics(semantic);
                });
            }
        }

        private void ExecuteLoggingLevelCommand(String loggingLevel) {
            _loggingLevel = loggingLevel;

            foreach (KeyValuePair<Url, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.LoggingLevel(loggingLevel);
                });
            }
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
            foreach (IPuppet puppet in _puppetTable.Values) {
                try {
                    puppet.Crash();
                }
                catch (Exception) { }
            }

            _operatorResolutionCache = new Dictionary<OperatorId, IList<Url>>();
            _puppetTable = new Dictionary<Url, IPuppet>();
        }
    }
}
