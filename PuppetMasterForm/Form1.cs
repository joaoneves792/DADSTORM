using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CommonTypesLibrary;
using PuppetMasterLibrary;
using System.IO;

namespace PuppetMasterForm {
	public partial class StartForm : Form {

		private PuppetMaster _pm = null;
		private BackgroundWorker _worker = null;


		#region Constructors and Aux functions

		public StartForm() {
			InitializeComponent();

			_pm = new PuppetMaster();
			_pm.PrintEvent += PrintToOutput;
			_pm.DisableExecutionEvent += DisableCommandExecution;
			//_pm.EnableExecutionEvent += EnableCommandExecution;

			// enable form level key events
			this.KeyPreview = true;

			DisplayHelp();
			DisplayAvailableScripts();
		}

		private void DisplayHelp() {

			string str = "help:\r\n"
				+ "\t display help \t\t Ctrl+H" + "\r\n"
				+ "\t clear output console \t Ctrl+L" + "\r\n"
				+ "\t load file \t\t\t Return" + "\r\n"
				+ "\t run step by step \t\t Ctrl+S" + "\r\n"
				+ "\t run all \t\t\t Ctrl+A" + "\r\n"
				+ "\t execute command \t\t Ctrl+Return" + "\r\n"
				+ "\t quick abort \t\t Ctrl+C";

			PrintToOutput(str);
		}

		private void DisplayAvailableScripts() {

			String str = "";
			String dir = _pm.GetScriptsDir();

			//get a list of scripts inside the folder
			foreach (string file in Directory.GetFiles(dir)) {
				str += Path.GetFileName(file) + "\r\n";
			}

			PrintToAvailableScripts(str);
		}

		#endregion


		#region Global Events

		private void StartForm_KeyUp(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.H) {
					DisplayHelp();

				} else if (e.KeyCode == Keys.L) {
					Output.Clear();

				} else if (e.KeyCode == Keys.C) {
					RunAbort_Click(this, null);
				}

			} else if (Control.ModifierKeys == Keys.Alt) {
				if (e.KeyCode == Keys.F4) {
					PrintToOutput("FIXME alt+f4 -> abort");
				}
			}
		}

		private void StartForm_FormClosing(object sender, FormClosingEventArgs e) {
			_pm.CloseProcesses();
		}

		#endregion


		#region File loading
		private void ScriptFile_TextChanged(object sender, EventArgs e) {
			// TODO verify validity as typing
		}

		private void LoadFile_Click(object sender, EventArgs e) {

			if (!ScriptFile.Text.Equals("")) {
				//LoadFile.Enabled = false;
				//ScriptFile.Enabled = false;

				_worker = new BackgroundWorker();
				_worker.DoWork += new DoWorkEventHandler((doWorkEventSender, doWorkEventArgs) => {
					_pm.LoadFile("..\\..\\scripts\\" + (String) doWorkEventArgs.Argument);
				});
				_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadFile_RunWorkerCompleted);
				_worker.RunWorkerAsync(ScriptFile.Text);
			}
		}

		private void LoadFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			ScriptFile.Clear();

			//LoadFile.Enabled = true;
			//ScriptFile.Enabled = true;
			RunStepByStep.Enabled = true;
			RunAll.Enabled = true;
			//RunCommand.Enabled = true;
			//Command.Enabled = true;
		}

		private void ScriptFile_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.Back) {
					ScriptFile.Text = "";

				} else if (e.KeyCode == Keys.S) {
					RunStepByStep_Click(this, null);

				} else if (e.KeyCode == Keys.A) {
					RunAll_Click(this, null);
				}

			} else if (e.KeyCode == Keys.Return) {
				LoadFile_Click(this, null);
				PrintToOutput("Loaded: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
			}
		}

		#endregion


		#region Abort handling

		private void RunAbort_Click(object sender, EventArgs e) {
			RunAbort.Enabled = false;
			RunStepByStep.Enabled = false;
			RunAll.Enabled = false;
			LoadFile.Enabled = false;
			ScriptFile.Enabled = false;
			RunCommand.Enabled = false;
			Command.Enabled = false;

			//_worker?.CancelAsync();

			_worker = new BackgroundWorker();
			_worker.DoWork += new DoWorkEventHandler((doWorkEventSender, doWorkEventArgs) => {
				_pm.CloseProcesses();
			});
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunAbort_RunWorkerCompleted);
			_worker.RunWorkerAsync();
		}

		private void RunAbort_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			PrintToOutput("Closed operators.");

			LoadFile.Enabled = true;
			ScriptFile.Enabled = true;
			RunCommand.Enabled = true;
			Command.Enabled = true;
		}

		#endregion


		#region File execution control

		private void RunStepByStep_Click(object sender, EventArgs e) {
			RunAbort.Enabled = true;
			RunStepByStep.Enabled = false;
			RunAll.Enabled = false;
			LoadFile.Enabled = false;
			ScriptFile.Enabled = false;
			RunCommand.Enabled = false;
			Command.Enabled = false;

			_worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			_worker.ProgressChanged += new ProgressChangedEventHandler(RunStepByStep_ProgressChanged);
			_worker.DoWork += new DoWorkEventHandler((doWorkEventSender, doWorkEventArgs) => {
				_pm.ExecuteSingleCommand();
			});
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunStepByStep_RunWorkerCompleted);
			_worker.RunWorkerAsync();
		}

		private void RunStepByStep_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			RunStepByStep.Enabled = true;
			RunAll.Enabled = true;
			RunCommand.Enabled = true;
			Command.Enabled = true;
		}

		private void RunStepByStep_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			PrintToOutput((String) e.UserState);
		}

		private void RunAll_Click(object sender, EventArgs e) {
			RunAbort.Enabled = true;
			RunStepByStep.Enabled = false;
			RunAll.Enabled = false;

			_worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			_worker.ProgressChanged += new ProgressChangedEventHandler(RunAll_ProgressChanged);
			_worker.DoWork += new DoWorkEventHandler((doWorkEventSender, doWorkEventArgs) => {
				_pm.ExecuteAllCommands();
			});
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunAll_RunWorkerCompleted);
			_worker.RunWorkerAsync();
		}

		private void RunAll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			RunCommand.Enabled = true;
			Command.Enabled = true;
		}

		private void RunAll_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			PrintToOutput((String) e.UserState);
		}

		private void DisableCommandExecution() {
			RunStepByStep.Enabled = false;
			RunAll.Enabled = false;
		}

		private void DisableCommandExecution(object sender, EventArgs e) {
			if (_worker.IsBusy) {
				_worker.ReportProgress(0);
			} else {
				DisableCommandExecution();
			}
		}

		//private void EnableCommandExecution() {
		//	RunStepByStep.Enabled = true;
		//	RunAll.Enabled = true;
		//}

		//private void EnableCommandExecution(object sender, EventArgs e) {
		//	if (_worker.IsBusy) {
		//		_worker.ReportProgress(0);
		//	} else {
		//		EnableCommandExecution();
		//	}
		//}

		#endregion


		#region Manual command handling

		private void Command_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.Return) {
					RunCommand_Click(this, null);
				}
			}
		}

		private void RunCommand_Click(object sender, EventArgs e) {
			RunAbort.Enabled = true;
			//LoadFile.Enabled = false;
			//ScriptFile.Enabled = false;
			//RunCommand.Enabled = false;
			//Command.Enabled = false;

			String cmd = Command.Text.Replace(System.Environment.NewLine, " ");
			PrintToOutput("manual command: " + cmd);

			_worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			_worker.ProgressChanged += new ProgressChangedEventHandler(RunCommand_ProgressChanged);
			_worker.DoWork += new DoWorkEventHandler((doWorkEventSender, doWorkEventArgs) => {
				_pm.ParseLineAndExecuteCommand((String) doWorkEventArgs.Argument);
			});
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunCommand_RunWorkerCompleted);
			_worker.RunWorkerAsync(cmd);
		}

		private void RunCommand_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			//RunCommand.Enabled = true;
			//Command.Enabled = true;

			Command.Clear();
		}

		private void RunCommand_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			PrintToOutput((String) e.UserState);
		}

		private void Command_TextChanged(object sender, EventArgs e) {
			// TODO Everify validity as typing
		}

		#endregion


		#region Output handling

		private void PrintToOutput(string text) {
			Output.Text = text + "\r\n\r\n" + Output.Text;
		}

		private void PrintToOutput(object sender, TextEventArgs e) {
			String text = e.Text;

			if (_worker.IsBusy) {
				_worker.ReportProgress(0, text);
			} else {
				PrintToOutput(text);
			}
		}

		#endregion



		// REMOVE this is only necessary if related to events. just use TextBox.Text = string;
		private void PrintToAvailableScripts(string text) {
			AvailableScripts.Text = text;
		}

		private void PrintToAvailableScripts(object sender, TextEventArgs e) {
			PrintToAvailableScripts(e.Text);
		}
	}
}
