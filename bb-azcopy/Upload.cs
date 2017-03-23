using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace bb_azcopy
{
    public partial class Upload : Form
    {
        private static readonly StringBuilder CmdOutput = new StringBuilder();
        private Process _p;
        private readonly bb_netfx.NetFxVersion _netFx = bb_netfx.NetFxVersion.Get45PlusFromRegistry();
        private readonly bool _is45Available;

        private static void Log(string message)
        {
            var msg = $"LOG: {DateTime.Now:O}: {message}";
            Debug.WriteLine(msg);
            CmdOutput.AppendLine(msg);
        }

        public Upload()
        {
            InitializeComponent();
            _is45Available = (_netFx.Major >= 4 && _netFx.Minor >= 5);

            Log(_is45Available
                ? ".net fx 4.5 available, using latest azcopy"
                : ".net fx 4.5 not available, falling back to azcopy 2.2.2");
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

            //BE SURE to use a SAS with a SV (service version) of 2013-08-05 or earlier
            if (!Uri.TryCreate(textStorageKey.Text, UriKind.Absolute, out Uri blobUrl))
            {
                MessageBox.Show(@"Check the storage URL");
                return;
            }

            if (!blobUrl.DnsSafeHost.Contains("windows.net")) //short link resolution
            {
                var req = (HttpWebRequest)WebRequest.Create(blobUrl);
                req.AllowAutoRedirect = false;

                using (var response = (HttpWebResponse)req.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Moved) //bit.ly uses 301s
                    {
                        blobUrl = response.ResponseUri == blobUrl ? new Uri(response.Headers["Location"]) : response.ResponseUri;
                    }
                }
            }

            if (!a.Exists) MessageBox.Show(@"Can't find that file to upload. Check and try again.");
            Log($"Starting upload to {blobUrl}");
            var blob = $"{blobUrl.Scheme}://{blobUrl.Host}{blobUrl.AbsolutePath}";
            var sas = blobUrl.Query;
            StartAzCopy(a, blob, sas, _is45Available);
        }

        private void StartAzCopy(FileInfo filePath, string storage, string sas, bool latest = true)
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
            _p = new Process
            {
                StartInfo =
                {
                    FileName = azPath,
                    Arguments = azArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                }
            };

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

           

            _p.Start();

            var output = new AsyncStreamReader(_p.StandardOutput);
            var error = new AsyncStreamReader(_p.StandardError);

            output.DataReceived += (sender, data) =>
            {
                if (string.IsNullOrEmpty(data)) return;
                CmdOutput.AppendLine(data);
                Debug.WriteLine(data);
            };

            error.DataReceived += (sender, data) =>
            {
                if (string.IsNullOrEmpty(data)) return;
                CmdOutput.AppendLine(data);
                Debug.WriteLine(data);
            };

            output.Start();
            error.Start();

            //_cmdStreamWriter = _cmdProcess.StandardInput;
            //_p.BeginOutputReadLine();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textOutput.Text = CmdOutput.ToString();
        }
    }

    /// <summary>
    /// Stream reader for StandardOutput and StandardError stream readers
    /// Runs an eternal BeginRead loop on the underlaying stream bypassing the stream reader.
    /// 
    /// The TextReceived sends data received on the stream in non delimited chunks. Event subscriber can
    /// then split on newline characters etc as desired.
    /// </summary>
    class AsyncStreamReader
    {
        public delegate void EventHandler<TArgs>(object sender, string Data);
        public event EventHandler<string> DataReceived;

        protected readonly byte[] Buffer = new byte[256];
        private readonly StreamReader _reader;

        public bool Active { get; private set; }

        public void Start()
        {
            if (Active) return;
            Active = true;
            BeginReadAsync();
        }

        public void Stop()
        {
            Active = false;
        }

        public AsyncStreamReader(StreamReader readerToBypass)
        {
            this._reader = readerToBypass;
            this.Active = false;
        }

        protected void BeginReadAsync()
        {
            if (this.Active)
            {
                _reader.BaseStream.BeginRead(Buffer, 0, Buffer.Length, ReadCallback, null);
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            var bytesRead = _reader.BaseStream.EndRead(asyncResult);

            string data = null;

            //Terminate async processing if callback has no bytes
            if (bytesRead > 0)
            {
                data = _reader.CurrentEncoding.GetString(this.Buffer, 0, bytesRead);
            }
            else
            {
                //callback without data - stop async
                Active = false;
            }

            //Send data to event subscriber - null if no longer active
            DataReceived?.Invoke(this, data);

            //Wait for more data from stream
            BeginReadAsync();
        }
    }
}
