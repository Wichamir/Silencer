using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;

namespace Silencer
{
    /// <summary>
    /// Records audio to seperate files based on session's process name.
    /// </summary>
    class SessionObserver : IDisposable
    {
        private Recorder recorder;

        public MMDevice Device { get; private set; }
        public AudioSessionControl Session { get; private set; }
        public string Directory { get; private set; }
        public string LastWindowName { get; private set; }
        public int InvalidFilenameCount { get; private set; }

        public SessionObserver(MMDevice device, AudioSessionControl session, string directory)
        {
            Device = device;
            Session = session;
            Directory = directory;
            LastWindowName = GetWindowName();
            
            recorder = new Recorder(device, GetFilePath(LastWindowName));
        }

        ~SessionObserver()
        {
            Dispose();
        }
        
        public void Update()
        {
            var currentWindowName = GetWindowName();

            if (currentWindowName == LastWindowName)
                return; // if names stayed the same

            if (currentWindowName == string.Empty)
                Dispose();
            
            recorder.Dispose();
            recorder = new Recorder(Device, GetFilePath(currentWindowName));
            LastWindowName = currentWindowName;
        }

        public string GetWindowName()
        {
            string result;
            try
            {
                var process = Process.GetProcessById((int)Session.GetProcessID);
                result = process.MainWindowTitle;
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        public string GetFilePath(string filename, string format = "wav")
        {
            const string pattern = @"{0}\{1}{2}.{3}";
            const string invalidCharacters = @"[\/?:*""><|]+";
            filename = Regex.Replace(filename, invalidCharacters, "", RegexOptions.Compiled); // replace invalid characters
            var filepath = string.Format(pattern, Directory, filename, string.Empty, format); // format filepath
            try
            {
                // validate filepath
                var fileInfo = new FileInfo(filepath);
            }
            catch
            {
                // if filepath is still invalid
                filepath = string.Format(pattern, Directory, "invalid", InvalidFilenameCount++, "wav");
            }
            return filepath;
        }

        public void Dispose()
        {
            recorder.Dispose();
            Device.Dispose();
        }
    }
}
