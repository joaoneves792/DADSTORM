using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DistributedAlgoritmsClassLibrary
{
    [Serializable]
    public class Process : IComparable
    {
        private readonly string _name,
                                _url,
                                _uri;
        private string _serviceName;
        private readonly int _port;
        private int _rank;
        private IList<string> _suffixes;

        public Process(string name, string url) {
            _name = name;
            _url = url;

            //TODO: needs to check and handle parse faults
            Match match = Regex.Match(url, @"^(tcp://[\w\.]+:(\d{4,5}))/(\w+)$");
            _uri = match.Groups[1].Value;
            _port = int.Parse(match.Groups[2].Value);
            _serviceName = match.Groups[3].Value;
            _suffixes = new List<string>();

            _rank = 0;
        }

        public Process(string name, string url, IList<string> suffix) {
            _name = name;
            _url = url;

            //TODO: needs to check and handle parse faults
            Match match = Regex.Match(url, @"^(tcp://[\w\.]+:(\d{4,5}))/(\w+)$");
            _uri = match.Groups[1].Value;
            _port = int.Parse(match.Groups[2].Value);
            _serviceName = match.Groups[3].Value;
            _suffixes = suffix;

            _rank = 0;
        }

        public string Name {
            get { return _name; }
        }

        public string Url {
            get { return _url; }
        }

        public string Uri {
            get { return _uri; }
        }

        public string SuffixedUrl {
            get { return _url + string.Join("_", _suffixes); }
        }

        public string ServiceName {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        public string SuffixedServiceName {
            get { return _serviceName + string.Join("_", _suffixes); }
            set { _serviceName = value; }
        }

        public int Port {
            get { return _port; }
        }

        public int Rank {
            get { return _rank; }
            set { _rank = value; }
        }

        public Process Concat(string suffixName) {
            IList<string> suffixes = new List<string>(_suffixes);
            suffixes.Add(suffixName);
            return new Process(_name, _url, suffixes);
        }

        public Process Unconcat(string suffixName) {
            IList<string> suffixes = new List<string>(_suffixes);
            suffixes.Remove(suffixName);
            return new Process(_name, _url, suffixes);
        }

        public override string ToString() {
            return "Name:    " + _name + Environment.NewLine +
                   "URL:     " + _url + Environment.NewLine +
                   "Service: " + _serviceName + Environment.NewLine +
                   "Port:    " + _port + Environment.NewLine +
                   "Rank:    " + _rank + Environment.NewLine +
                   "Suffixes:" + Environment.NewLine + string.Join(Environment.NewLine, _suffixes);
        }

        public bool Equals(Process process) {
            return process == null ? false :
                   process._name.Equals(_name) &&
                   process._url.Equals(_url) &&
                   process._serviceName.Equals(_serviceName) &&
                   process._port == _port;
        }

        public override bool Equals(object obj) {
            if (obj is Process) {
                return Equals(obj as Process);
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return (_name + _url /*+ _serviceName +  _port + _rank*/).GetHashCode();
        }

        public int CompareTo(Object process) {
            return _rank == ((Process)process).Rank ? - (_port - ((Process)process).Port) :
                   _rank - ((Process)process).Rank;
        }
    }
}
