using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using NAudio.CoreAudioApi;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Silencer
{
    public partial class MainForm : Form
    {
        public readonly string ConfigDefaultPath = Application.UserAppDataPath + @"\config_default.json";
        public readonly string ConfigCurrentPath = Application.UserAppDataPath + @"\config_current.json";

        private SessionObserver sessionObserver;

        public MainForm()
        {
            InitializeComponent();
            InitializeAudioApi();
            InitializeConfiguration();
        }

        public void InitializeAudioApi()
        {
            UpdateSessionList();
            updateTimer.Tick += OnUpdateTimerTick;
        }

        public void InitializeConfiguration()
        {
            if (!File.Exists(ConfigDefaultPath))
                Utils.SaveConfiguration(ConfigDefaultPath, GetConfiguration());
            if (File.Exists(ConfigCurrentPath))
                SetConfiguration(Utils.LoadConfiguration(ConfigCurrentPath));
        }

        public void AddSession(AudioSessionControl session)
        {
            var names = Utils.GetSessionInfo(session);
            var isMuted = session.SimpleAudioVolume.Mute ? "Yes" : "No";
            var item = names.GetListViewItem();
            item.SubItems.Add(isMuted);
            sessionList.Items.Add(item);
        }

        public Configuration GetConfiguration()
        {
            var settings = new List<SettingInfo>();
            for(int i = 0; i < settingsList.Items.Count; i++)
            {
                settings.Add(
                    new SettingInfo(
                        settingsList.Items[i].Checked,
                        settingsList.Items[i].SubItems[1].Text,
                        settingsList.Items[i].SubItems[2].Text,
                        settingsList.Items[i].SubItems[3].Text
                    )
                );
            }
            return new Configuration(muteEnabled.Checked, settings.ToArray(), recordProcessTextBox.Text);
        }

        public void SetConfiguration(Configuration config)
        {
            if (config == null)
            {
                MessageBox.Show("Config file is invalid!", "Error!");
                return;
            }
            muteEnabled.Checked = config.MuteEnabled;
            settingsList.Items.Clear();
            settingsList.Items.AddRange(config.GetListViewItems());
            recordProcessTextBox.Text = config.RecordProcessName;
        }

        public void UpdateSessionList()
        {
            sessionList.Items.Clear();
            Utils.EnumerateSessions((session) => AddSession(session));
        }

        public void UpdateFileList()
        {
            if (sessionObserver == null)
                return;

            var files = Directory.GetFiles(sessionObserver.Directory);
            var filesValid = new List<string>();
            foreach (var filepath in files)
                if (filepath.EndsWith(".wav"))
                    filesValid.Add(filepath);
            ListViewItem[] items = new ListViewItem[filesValid.Count];
            for (int i = 0; i < filesValid.Count; i++)
                items[i] = new ListViewItem(Path.GetFileName(filesValid[i]));

            fileList.Items.Clear();
            fileList.Items.AddRange(items);
        }

        public void UpdateMute()
        {
            Utils.EnumerateSessions((session) =>
            {
                var mute = false;
                foreach (ListViewItem setting in settingsList.Items)
                {
                    var detectedNames = Utils.GetSessionInfo(session);

                    var settingsProcessName = setting.SubItems[1].Text;
                    var settingsWindowName = setting.SubItems[2].Text;
                    var settingsSessionName = setting.SubItems[3].Text;

                    mute = (
                        LikeOperator.LikeString(detectedNames.ProcessName, settingsProcessName, CompareMethod.Text) &&
                        LikeOperator.LikeString(detectedNames.WindowName, settingsWindowName, CompareMethod.Text) &&
                        LikeOperator.LikeString(detectedNames.SessionName, settingsSessionName, CompareMethod.Text) &&
                        setting.Checked && muteEnabled.Checked
                    );

                    if (mute)
                        break;
                }
                session.SimpleAudioVolume.Mute = mute;
            });
        }

        // event handlers

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateSessionList();
            UpdateFileList();
            UpdateMute();
            sessionObserver?.Update();
        }

        private void OnAddButtonClicked(object sender, EventArgs e)
        {
            using(var form = new AddSettingForm())
            {
                var result = form.ShowDialog();

                switch (result)
                {
                    case DialogResult.OK:
                        var item = new ListViewItem(new string[] { 
                            string.Empty,
                            form.ProcessNameTextBox.Text,
                            form.WindowNameTextBox.Text,
                            form.SessionNameTextBox.Text
                        });
                        settingsList.Items.Add(item);
                        break;
                }
            }
        }

        private void OnRemoveButtonClicked(object sender, EventArgs e)
        {
            foreach (ListViewItem item in settingsList.SelectedItems)
            {
                settingsList.Items.Remove(item);
            }
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            if (sessionObserver != null)
                return;
            using(var deviceEnumerator = Utils.GetDeviceEnumerator())
            {
                var device = Utils.GetDefaultDevice(deviceEnumerator);
                string directory = string.Empty;
                using (var dialog = new FolderBrowserDialog())
                {
                    var result = dialog.ShowDialog();
                    switch (result)
                    {
                        case DialogResult.OK:
                            directory = dialog.SelectedPath;
                            break;
                    }
                }
                if (directory == string.Empty)
                    return;

                AudioSessionControl session = null;
                foreach (AudioSessionControl currentSession in Utils.GetAudioSessions(Utils.GetAudioSessionManager(device)))
                {
                    if (Utils.GetSessionInfo(currentSession).ProcessName == recordProcessTextBox.Text)
                    {
                        session = currentSession;
                        break;
                    }
                }

                if(session == null)
                    return;

                sessionObserver = new SessionObserver(device, session, directory);
                statusTextBox.Text = "Recording...";
            }
        }

        private void OnStopButtonClicked(object sender, EventArgs e)
        {
            statusTextBox.Text = "Idle.";
            sessionObserver?.Dispose();
            sessionObserver = null;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            sessionObserver?.Dispose();
            sessionObserver = null;
            Utils.SaveConfiguration(ConfigCurrentPath, GetConfiguration());
        }

        private void OnMenuExitClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OnMenuAboutClicked(object sender, EventArgs e)
        {
            using (var dialog = new AboutForm())
            {
                dialog.ShowDialog();
            }
        }

        private void OnMenuHelpClicked(object sender, EventArgs e)
        {
            using(var dialog = new HelpForm())
            {
                dialog.ShowDialog();
            }
        }

        private void OnMenuExportSettingsClicked(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON files | *.json";
                switch (dialog.ShowDialog())
                {
                    case DialogResult.OK:
                        Utils.SaveConfiguration(dialog.FileName, GetConfiguration());
                        break;
                }
            }
        }

        private void OnMenuImportSettingsClicked(object sender, EventArgs e)
        {
            using(var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON files | *.json";
                switch (dialog.ShowDialog())
                {
                    case DialogResult.OK:
                        SetConfiguration(Utils.LoadConfiguration(dialog.FileName));
                        break;
                }
            }
        }
    }
}
