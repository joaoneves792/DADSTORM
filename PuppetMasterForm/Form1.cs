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
			// FIXME finish eventhandler
			_pm.Print += PrintToOutput;

			DisplayHelp();
		}



		// global events //

		// FIXME Alt+F4 to abort

		private void StartForm_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.H) {
					DisplayHelp();
				}
			}
		}

		private void DisplayHelp() {

			string str = "help:\r\n"
				+ "display help \t\t Ctrl+H" + "\r\n"
				+ "run step by step \t\t Return" + "\r\n"
				+ "run all \t\t\t Ctrl+Return" + "\r\n"
				+ "execute command \t\t Return";

			PrintToOutput(str);
		}



		// script file handling //

		private void ScriptFile_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.Back) {
					ScriptFile.Text = "";
				} else if (e.KeyCode == Keys.Return) {
					PrintToOutput("Runnig all from: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
					RunAll_Click(this, null);
				} else if (e.KeyCode == Keys.H) {
					DisplayHelp();
				}

			} else if (e.KeyCode == Keys.Return) {
				PrintToOutput("Runnig step from: " + Path.GetFullPath(".") + "\\" + ScriptFile.Text);
				RunStepByStep_Click(this, null);
			}
		}

		private void RunStepByStep_Click(object sender, EventArgs e) {
			// FIXME step by step execution
			PrintToOutput("FIXME step by step execution");
		}

		private void RunAll_Click(object sender, EventArgs e) {
			_pm.ExecuteConfigurationFile("..\\..\\scripts\\" + ScriptFile.Text);
			ScriptFile.Text = "";
		}

		private void ScriptFile_TextChanged(object sender, EventArgs e) {
			// FIXME verify validity as typing
		}



		// command handling //

		private void Command_KeyDown(object sender, KeyEventArgs e) {
			if (Control.ModifierKeys == Keys.Control) {
				if (e.KeyCode == Keys.H) {
					DisplayHelp();
				}

			} else if (e.KeyCode == Keys.Return) {
				PrintToOutput("manual command: " + Command.Text);
				RunCommand_Click(this, null);
			}
		}

		private void RunCommand_Click(object sender, EventArgs e) {
			_pm.ParseLineAndExecuteCommand(Command.Text);
			Command.Text = "";
		}

		private void Command_TextChanged(object sender, EventArgs e) {
			// FIXME verify validity as typing
		}



		// command handling //

		private void PrintToOutput(string text) {
			Output.Text = text + "\r\n\r\n" + Output.Text;
		}

		private void PrintToOutput(object sender, TextEventArgs e) {
			Output.Text = e.Text + "\r\n\r\n" + Output.Text;
		}


	}
}
