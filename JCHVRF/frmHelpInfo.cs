using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JCHVRF
{
    public partial class frmHelpInfo : System.Windows.Forms.Form
    {
        public frmHelpInfo()
        {
            InitializeComponent();
        }

        public frmHelpInfo(List<string> info, string title)
        {
            InitializeComponent();
            this.Text = title;

            this.textBox1.Text = "";
            foreach (string s in info)
            {
                this.textBox1.Text += Environment.NewLine + s + Environment.NewLine;
            }
        }

        public frmHelpInfo(List<string> info , string title, Point location)
        {
            InitializeComponent();
            this.Text = title;
            this.Location = location;

            this.textBox1.Text = "";
            foreach (string s in info)
            {
                this.textBox1.Text += Environment.NewLine + s + Environment.NewLine;
            }
        }
    }
}
