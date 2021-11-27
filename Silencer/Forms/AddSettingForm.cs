using System;
using System.Windows.Forms;

namespace Silencer
{
    public partial class AddSettingForm : Form
    {
        public AddSettingForm()
        {
            InitializeComponent();
        }

        private void OnShown(object sender, EventArgs e)
        {
            ProcessNameTextBox.Focus();
        }
    }
}
