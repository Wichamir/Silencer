using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Silencer
{
    /// <summary>
    /// Records multimedia device output to a specified file.
    /// </summary>
    class Recorder : IDisposable
    {
        public bool IsRecording { get; private set; }

        private readonly WasapiLoopbackCapture capture;
        private readonly WaveFileWriter writer;
        private readonly MMDevice device;

        public Recorder(MMDevice device, string filename)
        {
            this.device = device;
            capture = new WasapiLoopbackCapture(device);
            writer = new WaveFileWriter(filename, capture.WaveFormat);

            capture.DataAvailable += (object sender, WaveInEventArgs args) =>
            {
                writer.Write(args.Buffer, 0, args.BytesRecorded);
            };

            capture.RecordingStopped += (object sender, StoppedEventArgs args) =>
            {
                writer.Close();
            };

            capture.StartRecording();
            IsRecording = true;
        }

        ~Recorder()
        {
            Dispose();
        }

        public void Dispose()
        {
            capture.StopRecording();

            writer.Dispose();
            capture.Dispose();
            device.Dispose();

            IsRecording = false;
        }
    }
}
