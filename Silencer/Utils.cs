using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using NAudio.CoreAudioApi;
using Newtonsoft.Json;

namespace Silencer
{
    /// <summary>
    /// Static utility methods.
    /// </summary>
    static class Utils
    {
        #region Audio API wrapping methods

        public static MMDeviceEnumerator GetDeviceEnumerator()
        {
            return new MMDeviceEnumerator();
        }

        public static MMDevice GetDefaultDevice(MMDeviceEnumerator enumerator)
        {
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        public static AudioSessionManager GetAudioSessionManager(MMDevice device)
        {
            return device.AudioSessionManager;
        }

        public static List<AudioSessionControl> GetAudioSessions(AudioSessionManager manager)
        {
            var result = new List<AudioSessionControl>();
            for (int i = 0; i < manager.Sessions.Count; i++)
                result.Add(manager.Sessions[i]);
            return result;
        }

        public static void EnumerateSessions(params Action<AudioSessionControl>[] functions)
        {
            using (var deviceEnumerator = GetDeviceEnumerator())
            {
                using (var device = GetDefaultDevice(deviceEnumerator))
                {
                    var sessionManager = GetAudioSessionManager(device);
                    foreach(var session in GetAudioSessions(sessionManager))
                        foreach(var function in functions)
                            function(session);
                }
            }
        }

        public static SessionInfo GetSessionInfo(AudioSessionControl session)
        {
            SessionInfo result;
            try
            {
                Process process = Process.GetProcessById((int)session.GetProcessID);
                result = new SessionInfo(process.ProcessName, process.MainWindowTitle, session.DisplayName, session.SimpleAudioVolume.Mute);
            }
            catch
            {
                result = new SessionInfo("", "", session.DisplayName, false);
            }
            return result;
        }

        #endregion

        #region File IO

        public static void SaveConfiguration(string filepath, Configuration config)
        {
            try
            {
                File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        public static Configuration LoadConfiguration(string filepath)
        {
            Configuration result = null;
            try
            {
                result = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(filepath));
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            return result;
        }

        #endregion

        #region Extension methods

        public static void SetDataGridViewDoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }

        #endregion
    }
}
