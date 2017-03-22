namespace bb_azcopy
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.textStoragePath = new System.Windows.Forms.TextBox();
            this.textStorageKey = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(73, 78);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(212, 75);
            this.button1.TabIndex = 0;
            this.button1.Text = "Choose file...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textStoragePath
            // 
            this.textStoragePath.Location = new System.Drawing.Point(311, 86);
            this.textStoragePath.Name = "textStoragePath";
            this.textStoragePath.Size = new System.Drawing.Size(830, 26);
            this.textStoragePath.TabIndex = 1;
            this.textStoragePath.Text = "https://jpda.blob.core.windows.net/bbtest";
            // 
            // textStorageKey
            // 
            this.textStorageKey.Location = new System.Drawing.Point(311, 118);
            this.textStorageKey.Name = "textStorageKey";
            this.textStorageKey.Size = new System.Drawing.Size(830, 26);
            this.textStorageKey.TabIndex = 2;
            this.textStorageKey.Text = "uFgk5IaeCR4Vh8d0UHyje11wkZB6tuZb3EptaOxk4oasWQWS1Rf9TX8eWfb7KKdn/l45OhC5cuVj5Tpyv" +
    "gIOWg==";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(929, 164);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(212, 75);
            this.button2.TabIndex = 3;
            this.button2.Text = "Upload";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textOutput
            // 
            this.textOutput.AcceptsReturn = true;
            this.textOutput.AcceptsTab = true;
            this.textOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textOutput.Location = new System.Drawing.Point(73, 291);
            this.textOutput.Multiline = true;
            this.textOutput.Name = "textOutput";
            this.textOutput.ReadOnly = true;
            this.textOutput.Size = new System.Drawing.Size(1025, 467);
            this.textOutput.TabIndex = 4;
            this.textOutput.Text = "leggo";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1169, 790);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textOutput);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textStorageKey);
            this.Controls.Add(this.textStoragePath);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textStoragePath;
        private System.Windows.Forms.TextBox textStorageKey;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
    }
}

