using System.Windows.Forms;

namespace Silencer
{
    /// <summary>
    /// Holds session related names.
    /// </summary>
    public class SessionInfo
    {
        public string ProcessName { get; set; }
        public string WindowName { get; set; }
        public string SessionName { get; set; }

        public SessionInfo(string processName, string windowName, string sessionName)
        {
            ProcessName = processName;
            WindowName = windowName;
            SessionName = sessionName;
        }

        public ListViewItem GetListViewItem()
        {
            return new ListViewItem(new string[] {
                ProcessName,
                WindowName,
                SessionName
            });
        }
    }
}
