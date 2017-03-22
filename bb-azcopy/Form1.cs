using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace bb_azcopy
{
    public partial class Form1 : Form
    {
        private static StringBuilder _cmdOutput = new StringBuilder();
        Process _cmdProcess;
        StreamWriter _cmdStreamWriter;

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                label1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var file = openFileDialog1.FileName;
            var a = new FileInfo(file);
            if (a.Exists)
            {
                StartAzCopy(a, textStoragePath.Text, textStorageKey.Text);
            }
        }

        private void StartAzCopy(FileInfo filePath, string storage, string key)
        {
            _cmdOutput = new StringBuilder();
            _cmdProcess = new Process
            {
                StartInfo =
                {
                    FileName = $@"{Environment.CurrentDirectory}\assets\AzCopy.exe",
                    Arguments = $"/source:\"{filePath.Directory.FullName}\" /pattern:\"{filePath.Name}\" /dest:{storage} /destkey:{key} /Y",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false
                }
            };

            _cmdProcess.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                _cmdOutput.AppendLine(args.Data);
                Debug.WriteLine(args.Data);
            };

            _cmdProcess.Start();

            //_cmdStreamWriter = _cmdProcess.StandardInput;
            //_cmdProcess.BeginOutputReadLine();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textOutput.Text = _cmdOutput.ToString();
        }
    }
}
