using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace bb_azcopy
{
    public partial class Upload : Form
    {
        private static StringBuilder _cmdOutput = new StringBuilder();
        Process _cmdProcess;
        StreamWriter _cmdStreamWriter;
        private bb_netfx.NetFxVersion _netFx = bb_netfx.NetFxVersion.Get45PlusFromRegistry();
        private bool _is45Available = false;

        public Upload()
        {
            InitializeComponent();
            _is45Available = (_netFx.Major >= 4 && _netFx.Minor >= 5);

            if (_is45Available)
            {
                statusStrip1.Items.Add(".net fx 4.5 available, using latest azcopy");
            }
            else
            {
                statusStrip1.Items.Add(".net fx 4.5 not available, falling back to azcopy 2.2.2");
            }
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
            var blobUrl = new Uri(textStorageKey.Text);

            if (!blobUrl.DnsSafeHost.Contains("windows.net")) //short link resolution
            {
                var req = (HttpWebRequest)WebRequest.Create(blobUrl);
                req.AllowAutoRedirect = false;

                using (var response = (HttpWebResponse)req.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Moved) //bit.ly uses 301s
                    {
                        if (response.ResponseUri == blobUrl)
                        {
                            blobUrl = new Uri(response.Headers["Location"]); //and stuffs new location in Location header
                        }
                        else
                        {
                            blobUrl = response.ResponseUri;
                        }
                    }
                }
            }

            //BE SURE to use a SAS with a SV (service version) of 2013-08-05 or earlier
            if (a.Exists)
            {
                statusStrip1.Items.Add($"Starting upload to {blobUrl.ToString()}");
                var blob = $"{blobUrl.Scheme}://{blobUrl.Host}{blobUrl.AbsolutePath}";
                var sas = blobUrl.Query;

                StartAzCopy(a, blob, sas, !_is45Available);
            }
        }

        private void StartAzCopy(FileInfo filePath, string storage, string sas, bool latest = true)
        {
            _cmdOutput = new StringBuilder();
            //this should probably be more robust
            var azCopyVersion = latest ? "5.2.0" : "2.2.2";
            var azPath = $@"{Environment.CurrentDirectory}\assets\{azCopyVersion}\AzCopy.exe";
            var azArgs = $"/source:\"{filePath.Directory.FullName}\" /pattern:\"{filePath.Name}\" /dest:{storage} /destsas:\"{sas}\" /Y";

            if (!latest)
            {
                azArgs = $"\"{filePath.Directory.FullName}\" {storage} \"{filePath.Name}\" /destsas:\"{sas}\" /Y";
            }

            _cmdOutput.Append(azPath + " " + azArgs);
            _cmdProcess = new Process
            {
                StartInfo =
                {
                    FileName = azPath,
                    Arguments = azArgs,
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
