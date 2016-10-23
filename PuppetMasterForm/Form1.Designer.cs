namespace PuppetMasterForm {
	partial class StartForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.ScriptFileLable = new System.Windows.Forms.Label();
			this.ScriptFile = new System.Windows.Forms.TextBox();
			this.RunStepByStep = new System.Windows.Forms.Button();
			this.RunAll = new System.Windows.Forms.Button();
			this.ManualCommandLable = new System.Windows.Forms.Label();
			this.Command = new System.Windows.Forms.TextBox();
			this.RunCommand = new System.Windows.Forms.Button();
			this.OutputLable = new System.Windows.Forms.Label();
			this.Output = new System.Windows.Forms.TextBox();
			this.AvailableScripts = new System.Windows.Forms.TextBox();
			this.AvailableScriptsLable = new System.Windows.Forms.Label();
			this.ControlLable = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ScriptFileLable
			// 
			this.ScriptFileLable.AutoSize = true;
			this.ScriptFileLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScriptFileLable.Location = new System.Drawing.Point(30, 268);
			this.ScriptFileLable.Name = "ScriptFileLable";
			this.ScriptFileLable.Size = new System.Drawing.Size(137, 16);
			this.ScriptFileLable.TabIndex = 0;
			this.ScriptFileLable.Text = "Select initialization file";
			this.ScriptFileLable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ScriptFile
			// 
			this.ScriptFile.Location = new System.Drawing.Point(30, 287);
			this.ScriptFile.Name = "ScriptFile";
			this.ScriptFile.Size = new System.Drawing.Size(389, 20);
			this.ScriptFile.TabIndex = 1;
			this.ScriptFile.TextChanged += new System.EventHandler(this.ScriptFile_TextChanged);
			this.ScriptFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptFile_KeyDown);
			// 
			// RunStepByStep
			// 
			this.RunStepByStep.Location = new System.Drawing.Point(266, 313);
			this.RunStepByStep.Name = "RunStepByStep";
			this.RunStepByStep.Size = new System.Drawing.Size(75, 23);
			this.RunStepByStep.TabIndex = 2;
			this.RunStepByStep.Text = "StepByStep";
			this.RunStepByStep.UseVisualStyleBackColor = true;
			this.RunStepByStep.Click += new System.EventHandler(this.RunStepByStep_Click);
			// 
			// RunAll
			// 
			this.RunAll.Location = new System.Drawing.Point(347, 313);
			this.RunAll.Name = "RunAll";
			this.RunAll.Size = new System.Drawing.Size(75, 23);
			this.RunAll.TabIndex = 3;
			this.RunAll.Text = "All";
			this.RunAll.UseVisualStyleBackColor = true;
			this.RunAll.Click += new System.EventHandler(this.RunAll_Click);
			// 
			// ManualCommandLable
			// 
			this.ManualCommandLable.AutoSize = true;
			this.ManualCommandLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ManualCommandLable.Location = new System.Drawing.Point(30, 385);
			this.ManualCommandLable.Name = "ManualCommandLable";
			this.ManualCommandLable.Size = new System.Drawing.Size(332, 32);
			this.ManualCommandLable.TabIndex = 4;
			this.ManualCommandLable.Text = "Manual command\r\n(<newline> will be ignored and converted into <space>)";
			// 
			// Command
			// 
			this.Command.Location = new System.Drawing.Point(30, 420);
			this.Command.Multiline = true;
			this.Command.Name = "Command";
			this.Command.Size = new System.Drawing.Size(389, 207);
			this.Command.TabIndex = 5;
			this.Command.TextChanged += new System.EventHandler(this.Command_TextChanged);
			this.Command.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Command_KeyDown);
			// 
			// RunCommand
			// 
			this.RunCommand.Location = new System.Drawing.Point(344, 633);
			this.RunCommand.Name = "RunCommand";
			this.RunCommand.Size = new System.Drawing.Size(75, 23);
			this.RunCommand.TabIndex = 6;
			this.RunCommand.Text = "Run";
			this.RunCommand.UseVisualStyleBackColor = true;
			this.RunCommand.Click += new System.EventHandler(this.RunCommand_Click);
			// 
			// OutputLable
			// 
			this.OutputLable.AutoSize = true;
			this.OutputLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputLable.Location = new System.Drawing.Point(463, 16);
			this.OutputLable.Name = "OutputLable";
			this.OutputLable.Size = new System.Drawing.Size(539, 20);
			this.OutputLable.TabIndex = 7;
			this.OutputLable.Text = "Output                                                                           " +
    "                    ";
			// 
			// Output
			// 
			this.Output.AllowDrop = true;
			this.Output.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Output.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Output.ForeColor = System.Drawing.SystemColors.ControlLightLight;
			this.Output.Location = new System.Drawing.Point(467, 39);
			this.Output.Multiline = true;
			this.Output.Name = "Output";
			this.Output.ReadOnly = true;
			this.Output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.Output.Size = new System.Drawing.Size(867, 723);
			this.Output.TabIndex = 8;
			// 
			// AvailableScripts
			// 
			this.AvailableScripts.BackColor = System.Drawing.SystemColors.Window;
			this.AvailableScripts.Location = new System.Drawing.Point(30, 89);
			this.AvailableScripts.Multiline = true;
			this.AvailableScripts.Name = "AvailableScripts";
			this.AvailableScripts.ReadOnly = true;
			this.AvailableScripts.Size = new System.Drawing.Size(389, 159);
			this.AvailableScripts.TabIndex = 9;
			// 
			// AvailableScriptsLable
			// 
			this.AvailableScriptsLable.AutoSize = true;
			this.AvailableScriptsLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AvailableScriptsLable.Location = new System.Drawing.Point(30, 70);
			this.AvailableScriptsLable.Name = "AvailableScriptsLable";
			this.AvailableScriptsLable.Size = new System.Drawing.Size(107, 16);
			this.AvailableScriptsLable.TabIndex = 10;
			this.AvailableScriptsLable.Text = "Available scripts";
			// 
			// ControlLable
			// 
			this.ControlLable.AutoSize = true;
			this.ControlLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ControlLable.Location = new System.Drawing.Point(26, 16);
			this.ControlLable.Name = "ControlLable";
			this.ControlLable.Size = new System.Drawing.Size(397, 20);
			this.ControlLable.TabIndex = 11;
			this.ControlLable.Text = "Control                                                                  ";
			// 
			// StartForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1364, 774);
			this.Controls.Add(this.ControlLable);
			this.Controls.Add(this.AvailableScriptsLable);
			this.Controls.Add(this.AvailableScripts);
			this.Controls.Add(this.Output);
			this.Controls.Add(this.OutputLable);
			this.Controls.Add(this.RunCommand);
			this.Controls.Add(this.Command);
			this.Controls.Add(this.ManualCommandLable);
			this.Controls.Add(this.RunAll);
			this.Controls.Add(this.RunStepByStep);
			this.Controls.Add(this.ScriptFile);
			this.Controls.Add(this.ScriptFileLable);
			this.Name = "StartForm";
			this.Text = "PuppetMasterForm";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StartForm_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ScriptFileLable;
		private System.Windows.Forms.TextBox ScriptFile;
		private System.Windows.Forms.Button RunStepByStep;
		private System.Windows.Forms.Button RunAll;
		private System.Windows.Forms.Label ManualCommandLable;
		private System.Windows.Forms.TextBox Command;
		private System.Windows.Forms.Button RunCommand;
		private System.Windows.Forms.Label OutputLable;
		private System.Windows.Forms.TextBox Output;
		private System.Windows.Forms.TextBox AvailableScripts;
		private System.Windows.Forms.Label AvailableScriptsLable;
		private System.Windows.Forms.Label ControlLable;
	}
}

