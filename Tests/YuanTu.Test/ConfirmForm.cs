using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuanTu.Test
{
    public partial class ConfirmForm : Form
    {
        public ConfirmForm()
        {
            InitializeComponent();
        }
        
        public static string Show(string title, bool headless = false)
        {
            if (!headless)
                return null;
            using (var f = new ConfirmForm())
            {
                f.Text = title;
                f.ShowDialog();
                if (!headless)
                    return null;
                return f.richTextBox.Text;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
