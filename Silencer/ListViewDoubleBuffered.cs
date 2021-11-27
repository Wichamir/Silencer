using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Silencer
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ListView))]
    class ListViewDoubleBuffered : ListView
    {
        public ListViewDoubleBuffered()
        {
            DoubleBuffered = true; // this fixes flickering issues
        }
    }
}
