using System.Windows.Forms;

namespace Silencer
{
    /// <summary>
    /// Holds setting related information.
    /// </summary>
    public class SettingInfo : SessionInfo
    {
        public bool Enabled { get; set; }

        public SettingInfo(bool enabled, string processName, string windowName, string sessionName)
            : base(processName, windowName, sessionName)
        {
            Enabled = enabled;
        }

        public new ListViewItem GetListViewItem()
        {
            return new ListViewItem(new string[] {
                string.Empty,
                ProcessName,
                WindowName,
                SessionName
            });
        }
    }
}
