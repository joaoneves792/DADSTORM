using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DistributedAlgoritmsClassLibrary
{
    [Serializable]
    public class Process : IComparable
    {
        private readonly String _name,
                                _url,
                                _uri;
        private String _serviceName;
        private readonly int _port;
        private int _rank;
        private IList<String> _suffixes;

        public Process(String name, String url) {
            _name = name;
            _url = url;

            //TODO: needs to check and handle parse faults
            Match match = Regex.Match(url, @"^(tcp://[\w\.]+:(\d{4,5}))/(\w+)$");
            _uri = match.Groups[1].Value;
            _port = int.Parse(match.Groups[2].Value);
            _serviceName = match.Groups[3].Value;
            _suffixes = new List<String>();

            _rank = 0;
        }

        public Process(String name, String url, IList<String> suffix) {
            _name = name;
            _url = url;

            //TODO: needs to check and handle parse faults
            Match match = Regex.Match(url, @"^(tcp://[\w\.]+:(\d{4,5}))/(\w+)$");
            _port = int.Parse(match.Groups[2].Value);
            _serviceName = match.Groups[3].Value;
            _suffixes = suffix;

            _rank = 0;
        }

        public String Name {
            get { return _name; }
        }

        public String Url {
            get { return _url + String.Join("_", _suffixes); }
        }

        public String Uri {
            get { return _uri; }
        }

        public String ServiceName {
            get { return _serviceName + String.Join("_", _suffixes); }
            set { _serviceName = value; }
        }

        public int Port {
            get { return _port; }
        }

        public int Rank {
            get { return _rank; }
            set { _rank = value; }
        }

        public Process Concat(String suffixName) {
            IList<String> suffixes = new List<String>(_suffixes);
            suffixes.Add(suffixName);
            return new Process(_name, _url, suffixes);
        }

        public Process Unconcat(String suffixName) {
            IList<String> suffixes = new List<String>(_suffixes);
            suffixes.Remove(suffixName);
            return new Process(_name, _url, suffixes);
        }

        public override string ToString() {
            return "Name:    " + _name + Environment.NewLine +
                   "URL:     " + _url + Environment.NewLine +
                   "Service: " + _serviceName + Environment.NewLine +
                   "Port:    " + _port + Environment.NewLine +
                   "Rank:    " + _rank + Environment.NewLine +
                   "Suffixes:" + Environment.NewLine + String.Join(Environment.NewLine, _suffixes);
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
