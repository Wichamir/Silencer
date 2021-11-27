using System.Windows.Forms;

namespace Silencer
{
    public class Configuration
    {
        public bool MuteEnabled { get; set; }
        public SettingInfo[] Settings { get; set; }
        public string RecordProcessName { get; set; }

        public Configuration(bool muteEnabled, SettingInfo[] settings, string recordProcessName)
        {
            MuteEnabled = muteEnabled;
            Settings = settings;
            RecordProcessName = recordProcessName;
        }

        public ListViewItem[] GetListViewItems()
        {
            var items = new ListViewItem[Settings.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = Settings[i].GetListViewItem();
                items[i].Checked = Settings[i].Enabled;
            }
            return items;
        }
    }
}
