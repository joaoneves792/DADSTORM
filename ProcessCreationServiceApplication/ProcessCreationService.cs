using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationServiceApplication
{
    public class ProcessCreationService : MarshalByRefObject
    {
        public void createProcess(int port, String[] replica_urls)
        {
            String urls = "";
            foreach(String s in replica_urls)
            {
                urls = urls + s + " ";
            }
            Process.Start("OperatorApplication.exe", port + " " + urls);
        }
    }
}
