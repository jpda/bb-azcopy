using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using bb_netfx;

namespace bb_azcopy
{
    public partial class Upload : Form
    {
        private static readonly StringBuilder CmdOutput = new StringBuilder();
        private Process _p;
        private readonly NetFxVersion _netFx = NetFxVersion.Get45PlusFromRegistry();
        private readonly bool _is45Available;

        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public Upload()
        {
            InitializeComponent();
            _is45Available = (_netFx.Major >= 4 && _netFx.Minor >= 5);

            Log(_is45Available
                ? ".net fx 4.5 available, using latest azcopy"
                : ".net fx 4.5 not available, falling back to azcopy 2.2.2");

            _worker.WorkerReportsProgress = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                label1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs args)
        {
            var fileInfo = new FileInfo(openFileDialog1.FileName);
            if (!fileInfo.Exists)
            {
                MessageBox.Show(@"Can't find that file to upload. Check and try again.");
                return;
            }

            var blobUrl = ResolveTargetUri(textStorageKey.Text);

            Log($"Starting upload to {blobUrl}");
            var blob = $"{blobUrl.Scheme}://{blobUrl.Host}{blobUrl.AbsolutePath}";
            var sas = blobUrl.Query;

            _worker.ProgressChanged += (o, e) =>
            {
                CmdOutput.AppendLine(e.ToString());
            };
            _worker.DoWork += (s, e) => RunAzCopyBackground(fileInfo, blob, sas, _is45Available);
            _worker.RunWorkerAsync();
        }

        private Uri ResolveTargetUri(string uri)
        {
            //BE SURE to use a SAS with a SV (service version) of 2013-08-05 or earlier
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri blobUrl))
            {
                MessageBox.Show(@"Check the storage URL");
                return null;
            }

            if (blobUrl.DnsSafeHost.Contains("windows.net")) return blobUrl;

            var req = (HttpWebRequest)WebRequest.Create(blobUrl);
            req.AllowAutoRedirect = false;

            try
            {
                using (var response = (HttpWebResponse)req.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Moved) //bit.ly uses 301s
                    {
                        blobUrl = response.ResponseUri == blobUrl ? new Uri(response.Headers["Location"]) : response.ResponseUri;
                    }
                }
            }
            catch (WebException e)
            {
                MessageBox.Show($@"went sideways: {e.Status}");
                return null;
            }
            return blobUrl;
        }

        private void RunAzCopyBackground(FileInfo filePath, string storage, string sas, bool latest = true)
        {
            //this should probably be more robust
            var azCopyVersion = latest ? "5.2.0" : "2.2.2";
            Log($"Using azcopy version {azCopyVersion}");
            var azPath = $@"{Environment.CurrentDirectory}\assets\{azCopyVersion}\AzCopy.exe";
            var azArgs = $"/source:\"{filePath.Directory?.FullName}\" /pattern:\"{filePath.Name}\" /dest:{storage} /destsas:\"{sas}\" /Y";

            if (!latest)
            {
                azArgs = $"\"{filePath.Directory?.FullName}\" {storage} \"{filePath.Name}\" /destsas:\"{sas}\" /Y";
            }

            Log($"Executing: {azPath} {azArgs}");
            CmdOutput.AppendLine("--------------------- Process output ---------------------");

            using (_p = new Process
            {
                StartInfo = {
                    FileName = azPath,
                    Arguments = azArgs,
                    // shell execute will at least give progress in the AzCopy window - not the best solution
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //RedirectStandardInput = true
                }
            })
            {
                _p.Start();

                // here we have the remnants from attempting to redirect output - the output does redirect using standard methods, like BeginOutputReadLine, but since AzCopy never writes an actual line during the transfer (e.g., no flush()),
                // it never actually calls any of these events
                //var length = 0;
                //new Thread(() =>
                //{
                //    var buffer = new byte[8];
                //    while (true)
                //    {
                //        this read doesn't run until the process exits - oddly
                //        _p.StandardOutput.BaseStream.BeginRead(buffer, 0, buffer.Length, ar =>
                //        {
                //            _worker.ReportProgress(0, System.Text.Encoding.UTF8.GetString(buffer));
                //        }, null);
                //    }
                //}).Start();

                // use these with BeginOutputReadLine()
                //_p.ErrorDataReceived += (sender, args) =>
                //{
                //    if (string.IsNullOrEmpty(args.Data)) return;
                //    CmdOutput.AppendLine(args.Data);
                //    Debug.WriteLine(args.Data);
                //};

                //_p.OutputDataReceived += (sender, args) =>
                //{
                //    if (string.IsNullOrEmpty(args.Data)) return;
                //    CmdOutput.AppendLine(args.Data);
                //    Debug.WriteLine(args.Data);
                //};

                //_p.Start();

                // since no line is created until the transfer is finished, this will *eventually* return but may take a while on a big file
                _p.BeginOutputReadLine();
                _p.WaitForExit();
            }
        }

        private void timer1_Tick(object sender, EventArgs e) => textOutput.Text = CmdOutput.ToString();

        private static void Log(string message)
        {
            var msg = $"LOG: {DateTime.Now:O}: {message}";
            Debug.WriteLine(msg);
            CmdOutput.AppendLine(msg);
        }
    }
}