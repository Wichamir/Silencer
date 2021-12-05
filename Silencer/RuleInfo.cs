using System.Windows.Forms;

namespace Silencer
{
    /// <summary>
    /// Holds rule related information.
    /// </summary>
    public class RuleInfo
    {
        public bool Enabled { get; set; } = false;
        public string ProcessName { get; set; } = string.Empty;
        public string WindowName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;

        public static RuleInfo Empty { get => new RuleInfo(); }

        public RuleInfo() { }

        public RuleInfo(bool enabled, string processName, string windowName, string sessionName)
        {
            Enabled = enabled;
            ProcessName = processName;
            WindowName = windowName;
            SessionName = sessionName;
        }
    }
}
