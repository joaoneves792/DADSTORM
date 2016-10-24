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

		public StartForm() {
			InitializeComponent();

			_pm = new PuppetMaster();
			_pm.PrintEvent += PrintToOutput;

			this.KeyPreview = true;

			DisplayHelp();
			// FIXME missing function in puppetmaster
			AvailableScripts.Text = "FIXME";
		}



		// global events //

		// FIXME Alt+F4 to abort before closing
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.F4) { return true; }
			return base.ProcessCmdKey(ref msg, keyData);
		}


		private void StartForm_KeyUp(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.H) {
					DisplayHelp();

				} else if (e.KeyCode == Keys.L) {
					Output.Clear();
				}

			} else if (Control.ModifierKeys == Keys.Alt) {
				if (e.KeyCode == Keys.F4) {
					PrintToOutput("FIXME alt+f4 -> abort");
				}
			}
		}
		
		private void StartForm_FormClosing(object sender, FormClosingEventArgs e) {
			_pm.ParseLineAndExecuteCommand("abort");
		}

		private void DisplayHelp() {

			string str = "help:\r\n"
				+ "\t display help \t\t Ctrl+H" + "\r\n"
				+ "\t run step by step \t\t Return" + "\r\n"
				+ "\t run all \t\t\t Ctrl+Return" + "\r\n"
				+ "\t execute command \t\t Ctrl+Return" + "\r\n"
				+ "\t clear console \t\t Ctrl+L";

			PrintToOutput(str);
		}



		// script file handling //

		private void LoadFile_Click(object sender, EventArgs e) {
			_pm.LoadFile("..\\..\\scripts\\" + ScriptFile.Text);
			ScriptFile.Clear();
		}

		private void ScriptFile_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.Back) {
					ScriptFile.Text = "";

				} else if (e.KeyCode == Keys.S) {
					//PrintToOutput("Runnig all from: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
					RunStepByStep_Click(this, null);

				} else if (e.KeyCode == Keys.A) {
					//PrintToOutput("Runnig all from: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
					RunAll_Click(this, null);
				}

			} else if (e.KeyCode == Keys.Return) {
				PrintToOutput("Loaded: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
				LoadFile_Click(this, null);
			}
		}


		// file execution handling //

		private void RunStepByStep_Click(object sender, EventArgs e) {
			_pm.ExecuteSingleCommand();
		}

		private void RunAll_Click(object sender, EventArgs e) {
			//_pm.ExecuteConfigurationFile("..\\..\\scripts\\" + ScriptFile.Text);
			_pm.ExecuteAllCommands();
		}

		private void ScriptFile_TextChanged(object sender, EventArgs e) {
			// FIXME verify validity as typing
		}



		// command handling //

		private void Command_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.Return) {
					RunCommand_Click(this, null);
				}
			}
		}

		private void RunCommand_Click(object sender, EventArgs e) {
			string cmd = Command.Text.Replace(System.Environment.NewLine, " ");
			PrintToOutput("manual command: " + cmd);
			_pm.ParseLineAndExecuteCommand(cmd);
			//Command.Text = "";
			Command.Clear();
		}

		private void Command_TextChanged(object sender, EventArgs e) {
			// FIXME verify validity as typing
		}



		// output handling //

		private void PrintToOutput(string text) {
			Output.Text = text + "\r\n\r\n" + Output.Text;
		}

		private void PrintToOutput(object sender, TextEventArgs e) {
			Output.Text = e.Text + "\r\n\r\n" + Output.Text;
		}

	}
}
