using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace iFakeLocation
{
    public partial class DownloadForm : Form {
        private WebClient webClient = new WebClient();

        private int index = -1;
        private List<Tuple<string, string>> downloads = new List<Tuple<string, string>>();

        public bool Success { get; private set; }

        public DownloadForm()
        {
            InitializeComponent();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            Success = false;
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            if (e.Error != null)
            {
                MessageBox.Show(this, "An error occurred while downloading: " +
                                Path.GetFileName(downloads[index].Item2) + "\n\n" + e.Error.Message,
                    "iFakeLocation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
            }
            else
            {
                try
                {
                    File.Move(downloads[index].Item2 + ".incomplete", downloads[index].Item2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "An error occurred while downloading: " +
                                    Path.GetFileName(downloads[index].Item2) + "\n\n" + ex.Message,
                        "iFakeLocation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Close();
                    return;
                }

                HandleNextDownload();
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive < 0)
            {
                mainProgressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                var progress = e.BytesReceived * 100.0f / e.TotalBytesToReceive;
                mainProgressBar.Value = (int) (progress * 10);
                statusLabel.Text = $"Downloading: {Path.GetFileName(downloads[index].Item2)} ({progress:F1}%)";
            }
        }

        public void AddDownload(string url, string dest) {
            downloads.Add(new Tuple<string, string>(url, dest));
        }

        private void HandleNextDownload() {

            index++;
            if (index >= downloads.Count)
            {
                Success = true;
                Close();
                return;
            }

            var dir = Path.GetDirectoryName(downloads[index].Item2);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            webClient.DownloadFileAsync(new Uri(downloads[index].Item1), downloads[index].Item2 + ".incomplete");
        }

        private void DownloadForm_Load(object sender, EventArgs e)
        {
            if (downloads.Count > 0)
            {
                statusLabel.Text = $"Downloading: {Path.GetFileName(downloads[0].Item2)} (0%)";
                HandleNextDownload();
            }
            else
            {
                Close();
            }
        }
    }
}
