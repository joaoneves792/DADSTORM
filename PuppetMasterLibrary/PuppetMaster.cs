using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

using CommonTypesLibrary;

namespace PuppetMasterLibrary
{
    public partial class PuppetMaster {

		public delegate void PrintToOutputHandler(object sender, TextEventArgs args);
		public event PrintToOutputHandler PrintEvent;

		protected virtual void Print(string text) {
			PrintEvent?.Invoke(this, new TextEventArgs(text));
		}

		public delegate void DisableExecutionHandler(object sender, EventArgs args);
		public event DisableExecutionHandler DisableExecutionEvent;

		protected virtual void DisableExecution() {
			DisableExecutionEvent?.Invoke(this, new EventArgs());
		}

		//public delegate void EnableExecutionHandler(object sender, EventArgs args);
		//public event EnableExecutionHandler EnableExecutionEvent;

		//protected virtual void EnableExecution() {
		//	EnableExecutionEvent?.Invoke(this, new EventArgs());
		//}

		private bool isConfiguring;

		private List<string> _cmdList = new List<string>();


		// FIXME parsing should be done here to avoid having wrong or empty lines
		public void LoadFile(string configFile) {
			string line = null;
			StreamReader file = new StreamReader(configFile);
			while ((line = file.ReadLine()) != null) {
				if (line.Trim().Equals("")) { continue; }

				_cmdList.Add(line);
			}
		}

		public void ExecuteSingleCommand() {
			if (_cmdList.Count > 0) {
				ParseLineAndExecuteCommand(_cmdList.First());
				_cmdList.RemoveAt(0);

			} else {
				DisableExecution();
				Print("No more commands.");
			}
		}

		public void ExecuteAllCommands() {
			foreach (string cmd in _cmdList) {
				ParseLineAndExecuteCommand(cmd);
			}
			_cmdList.Clear();
		}


		private bool Matches(string pattern, string line, out GroupCollection groupCollection) {
			Regex regex = new Regex(pattern, RegexOptions.Compiled);

			Match match = regex.Match(line);
			if (!match.Success) {
				groupCollection = null;
				return false;
			}

			groupCollection = match.Groups;
			return true;
		}


		private void ToggleToExecutionMode() {
			if (isConfiguring) {
				isConfiguring = false;
			}
		}


		private void ToggleToConfigurationMode() {
			isConfiguring = true;
		}


		public void ParseLineAndExecuteCommand(string line) {
			GroupCollection groupCollection;
			if (Matches(OPERATOR_ID_COMMAND, line, out groupCollection)) {
				Log(line);
                ExecuteOperatorIdCommand(
					groupCollection[1].Value,
					groupCollection[2].Value,
					groupCollection[3].Value,
					groupCollection[4].Value,
					groupCollection[5].Value,
					groupCollection[6].Value);

			} else if (Matches(START_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteStartCommand(groupCollection[1].Value);

			} else if (Matches(INTERVAL_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteIntervalCommand(groupCollection[1].Value, groupCollection[2].Value);

			} else if (Matches(STATUS_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteStatusCommand();

			} else if (Matches(CRASH_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteCrashCommand(groupCollection[1].Value, groupCollection[2].Value);

            } else if (Matches(FREEZE_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteFreezeCommand(groupCollection[1].Value, groupCollection[2].Value);

            } else if (Matches(UNFREEZE_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteUnfreezeCommand(groupCollection[1].Value, groupCollection[2].Value);

            } else if (Matches(WAIT_COMMAND, line, out groupCollection)) {
				ToggleToExecutionMode();
				Log(line);
				ExecuteWaitCommand(groupCollection[1].Value);

			} else if (Matches(SEMANTICS_COMMAND, line, out groupCollection)) {
				Log(line);
				ExecuteSemanticsCommand(groupCollection[1].Value);

			} else if (Matches(LOGGING_LEVEL_COMMAND, line, out groupCollection)) {
				Log(line);
				ExecuteLoggingLevelCommand(groupCollection[1].Value);

            }
		}


		public string GetScriptsDir() {

			string dir = Directory.GetCurrentDirectory();
			DirectoryInfo dirInfo = null;
			//go 2 directories up
			for (int i = 0; i < 2; i++) {
				dirInfo = Directory.GetParent(dir);
				dir = dirInfo.FullName;
			}

			return dirInfo.GetDirectories("Scripts").First().FullName;
		}


		public void Run(string fileNames) {

			System.Console.Clear();
			string dir = GetScriptsDir();
			ToggleToConfigurationMode();

			foreach (string fileName in fileNames.Split(' ')) {

				if (!File.Exists(dir + "\\" + fileName)) {
					//get a list of scripts inside the folder
					Print("Available scripts: ");

					foreach (string file in Directory.GetFiles(dir)) {
						Print(" " + Path.GetFileName(file));
					}

					return;
				}

				LoadFile(dir + "\\" + fileName);
				ExecuteAllCommands();

				//ExecuteConfigurationFile(dir + "\\" + fileName);
			}

			StartCLI();
			CloseProcesses();
		}

		// LEGACY
		internal void StartCLI() {
			string command;
			while (true) {
				System.Console.Write("PuppetMaster> ");
				command = System.Console.ReadLine();
				if (command.ToLower().Equals("abort")) {
					return;
				}

				if (command.Equals("")) {
					continue;
				}

				try {
					ParseLineAndExecuteCommand(command);

				} catch (Exception exception) {
					Console.WriteLine(exception.Message);
					Console.ReadLine();
				}

				Console.WriteLine(Path.GetFullPath("."));

			}
		}
	}
}

// REMOVED //

//<summary>
// Reads configuration file
//</summary>

//<summary>
// Creates CLI interface for user interaction with Puppet Master Service
//</summary>

//<summary>
// Match string using regex patterns
//</summary>

//<summary>
// Change to execution mode
//</summary>

//<summary>
// Change to configuration mode
//</summary>

//<summary>
// Converts string input into command
//</summary>

////<summary>
//// Project @event point class
////</summary>
//public static class Program {
//	//<summary>
//	// Project @event point method
//	//</summary>
//	public static void Main(string[] args) {
//		PuppetMaster puppetMaster = new PuppetMaster();
//		string fileNames = string.Join("", args);

//		// Close all processes when ctrl+c is pressed
//		Console.CancelKeyPress += new ConsoleCancelEventHandler(puppetMaster.CloseProcesses);

//		while (true) {
//			puppetMaster.Run(fileNames);
//			fileNames = Console.ReadLine();
//		}
//	}
//}

//public void ExecuteConfigurationFile(string configurationFileName) {
//	string line;
//	StreamReader file = new StreamReader(configurationFileName);

//	while ((line = file.ReadLine()) != null) {
//		//Check line ignorability
//		if (line.Equals("") || line[0].Equals('-') || line[0].Equals('/')) {
//			continue;
//		}

//		try {
//			ParseLineAndExecuteCommand(line);

//		} catch (Exception exception) {
//			Print(exception.Message);
//			//Console.ReadLine();
//		}
//	}

//	file.Close();
//}
