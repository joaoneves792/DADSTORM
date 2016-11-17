using System;
using System.Text.RegularExpressions;

namespace DistributedAlgoritmsClassLibrary
{
    public class Process : MarshalByRefObject, IComparable
    {
        private readonly String _name,
                                _url,
                                _serviceName;
        private readonly int _port;
        private int _rank;

        public Process(String name, String url) {
            _name = name;
            _url = url;

            //TODO: needs to check and handle parse faults
            Match match = Regex.Match(url, @"^tcp://[\w\.]+:(\d{4,5})/(\w+)$");
            _port = int.Parse(match.Groups[1].Value);
            _serviceName = match.Groups[2].Value;

            //TODO: Generate a more accurate rank
            _rank = 10;
        }

        public String Name {
            get { return _name; }
        }

        public String Url {
            get { return _url; }
        }

        public String ServiceName {
            get { return _serviceName; }
        }

        public int Port {
            get { return _port; }
        }

        public int Rank {
            get { return _rank; }
            set { _rank = value; }
        }

        public override string ToString() {
            return _name +
                   _url;
        }

        public bool Equals(Process process) {
            return process.Name.Equals(_name) &&
                   process.Url.Equals(_url);
        }

        public override bool Equals(object obj) {
            if (obj is Process) {
                return Equals(obj as Process);
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return (_name + _url + _serviceName +  _port + _rank).GetHashCode();
        }

        public int CompareTo(Object process) {
            return _rank - ((Process)process).Rank;
        }
    }
}
