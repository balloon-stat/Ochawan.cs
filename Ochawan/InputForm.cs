using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ochawan
{
    public partial class InputBox : Form
    {
        public string Str = "放送";
        public InputBox(string title)
        {
            InitializeComponent();
            this.Text = title;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Text != "")
            {
                Str = textBox1.Text;
            }
            else
            {
                Str = "放送";
            }
            if (e.KeyCode == Keys.Enter)
            {
                //e.SuppressKeyPress = true;
                this.Close();
            }
        }
    }
}
