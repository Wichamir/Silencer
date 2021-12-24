using System.Windows.Forms;

namespace Silencer
{
    /// <summary>
    /// Holds info about current configuration of the program.
    /// </summary>
    public class Configuration
    {
        public bool MuteEnabled { get; set; }
        public RuleInfo[] Rules { get; set; }
        public string RecordProcessName { get; set; }
        public SettingInfo[] Settings { get; set; }

        public Configuration(bool muteEnabled, RuleInfo[] rules, string recordProcessName, SettingInfo[] settings)
        {
            MuteEnabled = muteEnabled;
            Rules = rules;
            RecordProcessName = recordProcessName;
            Settings = settings;
        }
    }
}
