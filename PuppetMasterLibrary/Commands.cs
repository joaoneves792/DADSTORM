using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.IO;

using ProcessCreationServiceApplication;
using CommonTypesLibrary;

namespace PuppetMasterLibrary
{
    public partial class PuppetMaster : MarshalByRefObject, IPuppetMaster {
        //Constants
        private const string OPERATOR_NAME = "Operator";
        private const string PUPPET_NAME = "Puppet";

        //Tables
        private IDictionary<string, IList<string>> _operatorResolutionCache;
        private IDictionary<string, IPuppet> _puppetTable;
        private IDictionary<string, IProcessCreationService> _processCreationServiceTable;

        private string _semantic, _loggingLevel;

        private readonly EventWaitHandle _waitHandle;

        // internal -> public
        public PuppetMaster() {
            ToggleToConfigurationMode();

            _operatorResolutionCache = new Dictionary<string, IList<string>>();
            _puppetTable = new Dictionary<string, IPuppet>();
            _processCreationServiceTable = new Dictionary<string, IProcessCreationService>();

            _semantic = "at-most-once";
            _loggingLevel = "light";

            string processCreationServiceUrl = "tcp://localhost:10000/";

            _waitHandle = new AutoResetEvent(false);

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

        public void ReceiveUrl(string url, ObjRef objRef) {
            IPuppet puppet = (IPuppet)RemotingServices.Unmarshal(objRef);
            puppet.Semantics(_semantic);
            puppet.LoggingLevel(_loggingLevel);
            _puppetTable.Add(url, puppet);
        }

        public void Log(string message) {
            lock (this) {
                StreamWriter w = File.AppendText("log.txt");
                w.WriteLine(message);
                w.Flush();
                w.Close();

				Print(message);
            }
        }

        private void ExecuteOperatorIdCommand(
            string operatorId,
            string inputOps,
            string repFact,
            string routing,
            string addresses,
            string operatorSpec) {
            IList<string> urlList;


            MatchCollection inputOpList = new Regex(GROUP_INPUT_OP, RegexOptions.Compiled).Matches(inputOps),
                            addressList = new Regex(GROUP_URL, RegexOptions.Compiled).Matches(addresses);
            //operatorSpecList = new Regex(GROUP_OPERATOR_SPEC, RegexOptions.Compiled).Matches(operatorSpec);

            GroupCollection operatorSpecList;
            Matches(GROUP_OPERATOR_SPEC, operatorSpec, out operatorSpecList);

            //Organize source list
            //Assumption: the process creation is made downstream-wise
            string sources = "", inputOp;
            foreach (Match inputOpMatch in inputOpList) {
                GroupCollection groups;
                Matches(GROUP_INPUT_OP, inputOpMatch.Value, out groups);
                inputOp = groups[1].Value;
                if (_operatorResolutionCache.TryGetValue(inputOp, out urlList)) {
                    sources += inputOp + "|" + string.Join("," + inputOp + "|", urlList) + ",";
                } else {
                    sources += inputOp + ",";
                }
            }
            if (!sources.Equals("")) {
                sources = sources.Remove(sources.Length - 1, 1);
            }

            //Organize replica list
            string replicas = "";
            foreach (Match address in addressList) {
                replicas += address.Value + ",";
            }
            if (!replicas.Equals("")) {
                replicas = replicas.Remove(replicas.Length - 1, 1);
            }


            //Organize operator spec list
            string operatorSpecs = "";
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
            if (operatorSpecList[10].Value.Equals("CUSTOM")) {
                operatorSpecs += operatorSpecList[10].Value + ",";
                operatorSpecs += operatorSpecList[11].Value + ",";
                operatorSpecs += operatorSpecList[12].Value + ",";
                //operatorSpecs += operatorSpecList[11].Value + "." + operatorSpecList[12].Value + ",";
                operatorSpecs += operatorSpecList[13].Value;
            }

            foreach (Match address in addressList) {
                GroupCollection groupCollection = new Regex(URL_ADDRESS, RegexOptions.Compiled).Match(address.Value).Groups;

                //Create process
                string processCreationServiceUrl = groupCollection[1].Value + ":10000/";
                _processCreationServiceTable[processCreationServiceUrl].CreateProcess(
                    operatorId + " " + groupCollection[0].Value + " " + sources + " " + replicas + " " + operatorSpecs + " " + routing + " " + _semantic);

                _waitHandle.WaitOne();

                //Add operator id into operator resolution cache
                if (_operatorResolutionCache.TryGetValue(operatorId, out urlList)) {
                    urlList.Add(groupCollection[0].Value);
                    _operatorResolutionCache.Remove(operatorId);
                    _operatorResolutionCache.Add(operatorId, urlList);
                } else {
                    urlList = new List<string>();
                    urlList.Add(groupCollection[0].Value);
                    _operatorResolutionCache.Add(operatorId, urlList);
                }
            }
            Thread.Sleep(1000);
        }

        public void ResetWaitHandle() {
            _waitHandle.Set();
            _waitHandle.Reset();
        }

        private void ExecuteStartCommand(string operatorId) {
            IList<string> urlList = _operatorResolutionCache[operatorId];

			foreach (string url in urlList) {
				IPuppet puppet = _puppetTable[url];
				Task.Run(() => {
					puppet.Start();
				});
			}
        }


        private void ExecuteIntervalCommand(string operatorId, string milliseconds) {
            IList<string> urlList = _operatorResolutionCache[operatorId];
            foreach(string url in urlList) {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Interval(Int32.Parse(milliseconds));
                });
            }
        }


        private void ExecuteStatusCommand() {
            foreach (KeyValuePair<string, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.Status();
                });
            }
        }


        private void ExecuteCrashCommand(string operatorId) {
            IList<string> urlList = _operatorResolutionCache[operatorId];
            foreach (string url in urlList)
            {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Crash();
                });
            }

        }


        private void ExecuteFreezeCommand(string operatorId){
            IList<string> urlList = _operatorResolutionCache[operatorId];
            foreach (string url in urlList)
            {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Freeze();
                });
            }

        }


        private void ExecuteUnfreezeCommand(string operatorId)
        {
            IList<string> urlList = _operatorResolutionCache[operatorId];
            foreach (string url in urlList)
            {
                IPuppet puppet = _puppetTable[url];
                Task.Run(() => {
                    puppet.Unfreeze();
                });
            }

        }


        private void ExecuteWaitCommand(string milliseconds) {
            //TODO: double-check this
            Thread.Sleep(Int32.Parse(milliseconds));
        }


        private void ExecuteSemanticsCommand(string semantic) {
            _semantic = semantic;

            foreach (KeyValuePair<string, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.Semantics(semantic);
                });
            }
        }


        private void ExecuteLoggingLevelCommand(string loggingLevel) {
            _loggingLevel = loggingLevel;

            foreach (KeyValuePair<string, IPuppet> entry in _puppetTable) {
                IPuppet puppet = entry.Value;
                Task.Run(() => {
                    puppet.LoggingLevel(loggingLevel);
                });
            }
        }


        public void CloseProcesses(object sender, ConsoleCancelEventArgs args) {
            CloseProcesses();
        }


        public void CloseProcesses() {
            Thread.Sleep(2000);

            foreach (IPuppet puppet in _puppetTable.Values) {
                try {
                    puppet.Crash();
                }
                catch (Exception) { }
            }

            ToggleToConfigurationMode();
            _operatorResolutionCache = new Dictionary<string, IList<string>>();
            _puppetTable = new Dictionary<string, IPuppet>();
        }
    }
}


// REMOVED //

//<summary>
// Close processes
//</summary>

///<summary>
/// Puppet Master CLI constructor
///</summary>

//<summary>
// Close processes
//</summary>

