using System.Windows.Forms;

namespace Silencer
{
    public class Configuration
    {
        public bool MuteEnabled { get; set; }
        public RuleInfo[] Rules { get; set; }
        public string RecordProcessName { get; set; }

        public Configuration(bool muteEnabled, RuleInfo[] rules, string recordProcessName)
        {
            MuteEnabled = muteEnabled;
            Rules = rules;
            RecordProcessName = recordProcessName;
        }
    }
}
