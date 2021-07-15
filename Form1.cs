using System;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;
using System.Diagnostics;

namespace System_Sound_Recorder
{
    public partial class Form1 : Form
    {
        Stopwatch stopwatch;
        private string outputFileName;
        private WasapiLoopbackCapture capture;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            stopwatch = new Stopwatch();
        }

        private void btnStart_Clicked(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "wave files | *.wav";

            if(dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            outputFileName = dialog.FileName;
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            capture = new WasapiLoopbackCapture();
            var writer = new WaveFileWriter(outputFileName, capture.WaveFormat);

            capture.DataAvailable += async (s, t) =>
            {
                if (writer != null)
                {
                    await writer.WriteAsync(t.Buffer, 0, t.BytesRecorded);
                    await writer.FlushAsync();
                }
            };

            capture.RecordingStopped += (s, t) =>
            {
                if(writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }

                btnStart.Enabled = true;
                btnStop.Enabled = false;
                capture.Dispose();
            };

            capture.StartRecording();
            stopwatch.Start();
        }

        private void btnStop_Clicked(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            capture.StopRecording();
            stopwatch.Stop();
            stopwatch.Reset();

            if(outputFileName == null)
            {
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetDirectoryName(outputFileName),
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lbl_Timer.Text = string.Format("{0:hh\\:mm\\:ss}",stopwatch.Elapsed);
        }
    }
}
