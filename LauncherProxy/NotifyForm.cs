using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LauncherProxy
{
    public partial class NotifyForm : Form
    {

        Action _action;

        public NotifyForm(Action action)
        {
            InitializeComponent();

            _action = action;
        }

        private void NotifyForm_Load(object sender, EventArgs e)
        {
            //this.Opacity = 0.7D;
            //this.BackColor = Color.Yellow;
            this.Width = 320;
            this.Height = 24;
            this.Top = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _action.Invoke();
            this.Close();
        }
    }
}
