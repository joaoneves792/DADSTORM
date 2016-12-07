using System.Collections.Generic;

namespace CustomOperatorExamplaes
{
    public class Basic {

		public Basic() { }

		public List<List<string>> Repeat(List<string> input) {
            List<List<string>> result = new List<List<string>>();
            result.Add(input);
            return result;
		}


    }
}
