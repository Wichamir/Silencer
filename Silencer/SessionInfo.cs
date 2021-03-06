using System.Windows.Forms;

namespace Silencer
{
    /// <summary>
    /// Holds audio session related information.
    /// </summary>
    public class SessionInfo
    {
        public string ProcessName { get; set; } = string.Empty;
        public string WindowName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public bool IsMuted { get; set; } = false;

        public static SessionInfo Empty { get => new SessionInfo(); }

        public SessionInfo() { }

        public SessionInfo(string processName, string windowName, string sessionName, bool isMuted)
        {
            ProcessName = processName;
            WindowName = windowName;
            SessionName = sessionName;
            IsMuted = isMuted;
        }
    }
}
