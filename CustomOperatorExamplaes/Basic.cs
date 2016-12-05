using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomOperatorExamplaes {
    public class Basic {

		public Basic() { }

		public List<List<string>> Repeat(List<string> input) {
            List<List<string>> result = new List<List<string>>();
            result.Add(input);
            return result;
		}


    }
}
