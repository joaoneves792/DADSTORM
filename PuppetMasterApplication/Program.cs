using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using System.Threading;

namespace PuppetMasterApplication
{
    //Aliases
    using Url = String;
    using OperatorId = String;
    using ProcessName = String;

    ///<summary>
    /// Puppet Master CLI
    ///</summary>
    internal partial class PuppetMaster
    {
        private bool isConfiguring;

        //<summary>
        // Reads configuration file
        //</summary>
        internal void ExecuteConfigurationFile(String configurationFileName)
        {
            String line;
            StreamReader file = new StreamReader(configurationFileName);

            while ((line = file.ReadLine()) != null)
            {
                //Check line ignorability
                if (line.Equals("") || line[0].Equals('-') || line[0].Equals('/'))
                {
                    continue;
                }
                Console.WriteLine(line);
                try
                {
                    ParseLineAndExecuteCommand(line);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.ReadLine();
                }
            }
            file.Close();
        }
        //<summary>
        // Creates CLI interface for user interaction with Puppet Master Service
        //</summary>
        internal void StartCLI()
        {
            String command;
            while (true)
            {
                System.Console.Write("PuppetMaster> ");
                command = System.Console.ReadLine();
                if (command.ToLower().Equals("abort"))
                {
                    return;
                }
                if (command.Equals(""))
                {
                    continue;
                }
                try
                {
                    ParseLineAndExecuteCommand(command);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.ReadLine();
                }
            }
        }

        //<summary>
        // Match string using regex patterns
        //</summary>
        private bool Matches(String pattern, String line, out GroupCollection groupCollection) {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            Match match = regex.Match(line);
            if (!match.Success) {
                groupCollection = null;
                return false;
            }

            groupCollection = match.Groups;
            return true;
        }

        //<summary>
        // Change to execution mode
        //</summary>
        private void ToggleToExecutionMode() {
            if (isConfiguring) {
                isConfiguring = false;
                Thread.Sleep(5000);
            }
        }

        //<summary>
        // Change to configuration mode
        //</summary>
        private void ToggleToConfigurationMode() {
            isConfiguring = true;
        }

        //<summary>
        // Converts string input into command
        //</summary>
        private void ParseLineAndExecuteCommand(String line)
        {
            GroupCollection groupCollection;

            if (Matches(OPERATOR_ID_COMMAND, line, out groupCollection))
            {
                ExecuteOperatorIdCommand(
                    groupCollection[1].Value,
                    groupCollection[2].Value,
                    groupCollection[3].Value,
                    groupCollection[4].Value,
                    groupCollection[5].Value,
                    groupCollection[6].Value);

            } else if (Matches(START_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteStartCommand(groupCollection[1].Value);

            } else if (Matches(INTERVAL_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteIntervalCommand(groupCollection[1].Value, groupCollection[2].Value);

            } else if (Matches(STATUS_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteStatusCommand();

            } else if (Matches(CRASH_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteCrashCommand(groupCollection[1].Value);

            } else if (Matches(FREEZE_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteFreezeCommand(groupCollection[1].Value);

            } else if (Matches(UNFREEZE_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteUnfreezeCommand(groupCollection[1].Value);

            } else if (Matches(WAIT_COMMAND, line, out groupCollection)) {
                ToggleToExecutionMode();
                ExecuteWaitCommand(groupCollection[1].Value);
            }
        }

        private string GetScriptsDir()
        {
            string dir = Directory.GetCurrentDirectory();
            DirectoryInfo dirInfo = null;
            //go 2 directories up
            for (int i = 0; i < 2; i++)
            {
                dirInfo = Directory.GetParent(dir);
                dir = dirInfo.FullName;
            }
            return dirInfo.GetDirectories("Scripts").First().FullName;
        }

        public void Run(String fileNames)
        {
            System.Console.Clear();
            string dir = GetScriptsDir();
            ToggleToConfigurationMode();

            foreach (String fileName in fileNames.Split(' '))
            {
                if (!File.Exists(dir + "\\" + fileName))
                {
                    //get a list of scripts inside the folder
                    Console.WriteLine("Available scripts: ");
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        Console.WriteLine(" " + Path.GetFileName(file));
                    }
                    return;
                }
                ExecuteConfigurationFile(dir + "\\" + fileName);
            }
            StartCLI();
            CloseProcesses();
        }
    }
    //<summary>
    // Project @event point class
    //</summary>
    public static class Program
    {
        //<summary>
        // Project @event point method
        //</summary>
        public static void Main(string[] args)
        {
            PuppetMaster puppetMaster = new PuppetMaster();
            String fileNames = string.Join("", args);

            // Close all processes when ctrl+c is pressed
            Console.CancelKeyPress += new ConsoleCancelEventHandler(puppetMaster.CloseProcesses);

            while (true)
            {
                puppetMaster.Run(fileNames);
                fileNames = Console.ReadLine();
            }
        }
    }
}
