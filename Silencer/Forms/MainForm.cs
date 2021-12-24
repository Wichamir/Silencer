using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

using NAudio.CoreAudioApi;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Silencer.Forms
{
    public partial class MainForm : Form
    {
        public readonly string ConfigDefaultPath = Application.StartupPath + @"\config_default.json";
        public readonly string ConfigCurrentPath = Application.UserAppDataPath + @"\config_current.json";

        private SessionObserver sessionObserver;

        private readonly BindingList<SessionInfo> sessionsList;
        private readonly BindingList<RuleInfo> rulesList;
        private readonly BindingList<AudioFileInfo> filesList;
        private readonly BindingList<SettingInfo> settingsList;

        #region Initialization

        public MainForm()
        {
            sessionsList = new BindingList<SessionInfo>();
            rulesList = new BindingList<RuleInfo>();
            filesList = new BindingList<AudioFileInfo>();
            settingsList = new BindingList<SettingInfo>();

            InitializeComponent();

            Utils.SetDataGridViewDoubleBuffered(sessionsGrid, true);
            Utils.SetDataGridViewDoubleBuffered(rulesGrid, true);
            Utils.SetDataGridViewDoubleBuffered(filesGrid, true);

            InitializeAudioApi();
            InitializeConfiguration();
        }

        public void InitializeAudioApi()
        {
            UpdateSessionsGrid();
            updateTimer.Tick += OnUpdateTimerTick;
        }

        public void InitializeConfiguration()
        {
            // binding
            sessionsGrid.DataSource = sessionsList;
            rulesGrid.DataSource = rulesList;
            filesGrid.DataSource = filesList;

            // loading conf from previous session
            if (File.Exists(ConfigCurrentPath))
                SetConfiguration(Utils.LoadConfiguration(ConfigCurrentPath));
            else if (File.Exists(ConfigDefaultPath))
                SetConfiguration(Utils.LoadConfiguration(ConfigDefaultPath));
        }

        #endregion

        #region Utility

        public void AddSession(AudioSessionControl session)
        {
            sessionsList.Add(Utils.GetSessionInfo(session));
        }

        public Configuration GetConfiguration()
        {
            var rulesArray = new RuleInfo[rulesList.Count];
            rulesList.CopyTo(rulesArray, 0);
            var settingsArray = new SettingInfo[settingsList.Count];
            settingsList.CopyTo(settingsArray, 0);
            return new Configuration(muteEnabled.Checked, rulesArray, recordProcessTextBox.Text, settingsArray);
        }

        public void SetConfiguration(Configuration config)
        {
            if (config == null)
            {
                MessageBox.Show("Config file is invalid!", "Error!");
                return;
            }
            try
            {
                muteEnabled.Checked = config.MuteEnabled;
                rulesList.Clear();
                foreach (var rule in config.Rules)
                    rulesList.Add(rule);
                recordProcessTextBox.Text = config.RecordProcessName;
                settingsList.Clear();
                foreach(var setting in config.Settings)
                {
                    settingsList.Add(setting);
                }
            }
            catch
            {
                MessageBox.Show("Config file may be invalid.", "Error!");
            }
        }

        public SettingInfo GetSettingByName(string name)
        {
            foreach(var setting in settingsList)
            {
                if(setting.Name == name)
                {
                    return setting;
                }
            }
            return null;
        }

        #endregion

        #region Updating

        public void UpdateSessionsGrid()
        {
            sessionsList.Clear();
            Utils.EnumerateSessions((session) => AddSession(session));
        }

        public void UpdateFilesGrid()
        {
            if (sessionObserver == null)
                return;

            var files = Directory.GetFiles(sessionObserver.Directory);
            filesList.Clear();
            foreach (var filepath in files)
                if (filepath.EndsWith(".wav"))
                    filesList.Add(new AudioFileInfo(Path.GetFileName(filepath)));
        }

        public void UpdateMute()
        {
            Utils.EnumerateSessions((session) =>
            {
                var mute = false;
                foreach (var rule in rulesList)
                {
                    var detectedNames = Utils.GetSessionInfo(session);

                    mute = (
                        LikeOperator.LikeString(detectedNames.ProcessName, rule.ProcessName, CompareMethod.Text) &&
                        LikeOperator.LikeString(detectedNames.WindowName, rule.WindowName, CompareMethod.Text) &&
                        LikeOperator.LikeString(detectedNames.SessionName, rule.SessionName, CompareMethod.Text) &&
                        rule.Enabled && muteEnabled.Checked
                    );

                    if (mute)
                        break;
                }
                session.SimpleAudioVolume.Mute = mute;
            });
        }

        #endregion

        #region Event handlers

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateSessionsGrid();
            UpdateFilesGrid();
            UpdateMute();
            sessionObserver?.Update();
        }

        private void OnStartButtonClicked(object sender, EventArgs e)
        {
            if (sessionObserver != null)
                return;
            
            using(var deviceEnumerator = Utils.GetDeviceEnumerator())
            {
                var device = Utils.GetDefaultDevice(deviceEnumerator);

                var directorySetting = GetSettingByName("RecordDirectory");
                string directory = string.Empty;
                if(directorySetting == null)
                {
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

                    settingsList.Add(new SettingInfo("RecordDirectory", directory));
                }
                else
                {
                    directory = directorySetting.Value as string;
                }
                
                if (directory == string.Empty || directory == null)
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

        private void OnMenuSettingsClicked(object sender, EventArgs e)
        {
            using(var dialog = new SettingsForm())
            {
                dialog.SettingsGrid.DataSource = settingsList;
                switch (dialog.ShowDialog())
                {
                    case DialogResult.OK:
                        MessageBox.Show("OK");
                        // TODO
                        break;
                }
            }
        }

        #endregion
    }
}
