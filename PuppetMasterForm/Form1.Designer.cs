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
			this.SuspendLayout();
			// 
			// ScriptFileLable
			// 
			this.ScriptFileLable.AutoSize = true;
			this.ScriptFileLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScriptFileLable.Location = new System.Drawing.Point(48, 39);
			this.ScriptFileLable.Name = "ScriptFileLable";
			this.ScriptFileLable.Size = new System.Drawing.Size(131, 16);
			this.ScriptFileLable.TabIndex = 0;
			this.ScriptFileLable.Text = "initialization script file";
			this.ScriptFileLable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ScriptFile
			// 
			this.ScriptFile.Location = new System.Drawing.Point(51, 67);
			this.ScriptFile.Name = "ScriptFile";
			this.ScriptFile.Size = new System.Drawing.Size(420, 20);
			this.ScriptFile.TabIndex = 1;
			this.ScriptFile.TextChanged += new System.EventHandler(this.ScriptFile_TextChanged);
			// 
			// RunStepByStep
			// 
			this.RunStepByStep.Location = new System.Drawing.Point(499, 65);
			this.RunStepByStep.Name = "RunStepByStep";
			this.RunStepByStep.Size = new System.Drawing.Size(75, 23);
			this.RunStepByStep.TabIndex = 2;
			this.RunStepByStep.Text = "StepByStep";
			this.RunStepByStep.UseVisualStyleBackColor = true;
			this.RunStepByStep.Click += new System.EventHandler(this.RunStepByStep_Click);
			// 
			// RunAll
			// 
			this.RunAll.Location = new System.Drawing.Point(598, 65);
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
			this.ManualCommandLable.Location = new System.Drawing.Point(48, 137);
			this.ManualCommandLable.Name = "ManualCommandLable";
			this.ManualCommandLable.Size = new System.Drawing.Size(115, 16);
			this.ManualCommandLable.TabIndex = 4;
			this.ManualCommandLable.Text = "manual command";
			// 
			// Command
			// 
			this.Command.Location = new System.Drawing.Point(51, 165);
			this.Command.Name = "Command";
			this.Command.Size = new System.Drawing.Size(523, 20);
			this.Command.TabIndex = 5;
			this.Command.TextChanged += new System.EventHandler(this.Command_TextChanged);
			// 
			// RunCommand
			// 
			this.RunCommand.Location = new System.Drawing.Point(598, 161);
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
			this.OutputLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputLable.Location = new System.Drawing.Point(48, 227);
			this.OutputLable.Name = "OutputLable";
			this.OutputLable.Size = new System.Drawing.Size(44, 16);
			this.OutputLable.TabIndex = 7;
			this.OutputLable.Text = "output";
			// 
			// Output
			// 
			this.Output.Location = new System.Drawing.Point(51, 256);
			this.Output.Multiline = true;
			this.Output.Name = "Output";
			this.Output.Size = new System.Drawing.Size(622, 314);
			this.Output.TabIndex = 8;
			this.Output.TextChanged += new System.EventHandler(this.Output_TextChanged);
			// 
			// StartForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(719, 615);
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
	}
}

