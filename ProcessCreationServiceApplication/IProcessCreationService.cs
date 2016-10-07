using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationServiceApplication
{
    public interface IProcessCreationService
    {
        void CreateProcess(String arguments);
    }
}
