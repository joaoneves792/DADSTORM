using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PuppetMasterLibrary;
using System.IO;

namespace PuppetMasterForm {
	public partial class StartForm : Form {

		private PuppetMaster _pm = null;

		public StartForm() {
			InitializeComponent();

			// FIXME start puppetmaster
			_pm = new PuppetMaster();
		}

		private void ScriptFile_TextChanged(object sender, EventArgs e) {

		}

		private void RunStepByStep_Click(object sender, EventArgs e) {

		}

		private void RunAll_Click(object sender, EventArgs e) {

			_pm.ExecuteConfigurationFile("..\\..\\scripts\\" + ScriptFile.Text);
			ScriptFile.Text = "";

		}

		private void RunCommand_Click(object sender, EventArgs e) {
			_pm.ParseLineAndExecuteCommand(Command.Text);
			Command.Text = "";
		}

		private void Command_TextChanged(object sender, EventArgs e) {

		}

		private void Output_TextChanged(object sender, EventArgs e) {

		}
	}
}
