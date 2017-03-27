using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace bb_azdm
{
    public partial class Form1 : Form
    {
        private static readonly StringBuilder CmdOutput = new StringBuilder();

        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 8;
            ServicePointManager.Expect100Continue = false;
            _worker.WorkerReportsProgress = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var fi = new FileInfo(openFileDialog1.FileName);
                label1.Text = $@"{openFileDialog1.FileName}: {fi.Length} bytes";
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
                label2.Text = $@"{e.UserState.ToString()} of {fileInfo.Length} bytes transferred...";
                progressBar1.Value = e.ProgressPercentage;
            };
            _worker.DoWork += (s, e) => RunDataMovement(blobUrl, fileInfo);
            _worker.RunWorkerAsync();
            //RunDataMovement(blob, fileInfo);
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

        private void RunDataMovement(Uri sas, FileInfo file)
        {
            var container = new CloudBlobContainer(sas);
            try
            {
                var blob = container.GetBlockBlobReference(file.Name);
                var size = file.Length;

                TransferManager.Configurations.ParallelOperations = 64;
                var context = new SingleTransferContext
                {
                    ProgressHandler = new Progress<TransferStatus>(p =>
                    {
                        var percentageComplete = (double)p.BytesTransferred / size;
                        var pc = Convert.ToInt32(percentageComplete * 100);
                        _worker.ReportProgress(pc, p.BytesTransferred);
                    })
                };

                var task = TransferManager.UploadAsync(file.FullName, blob, null, context, CancellationToken.None);
                task.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Any())
                {
                    MessageBox.Show($@"{string.Join(",", e.InnerExceptions.Select(x => x.Message))}");
                }
            }
        }

        private static void Log(string message)
        {
            var msg = $"LOG: {DateTime.Now:O}: {message}";
            Debug.WriteLine(msg);
            CmdOutput.AppendLine(msg);
        }

        private void timer1_Tick(object sender, EventArgs e) => textOutput.Text = CmdOutput.ToString();
    }
}
