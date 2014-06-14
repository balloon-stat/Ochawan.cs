using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ochawan
{
    public partial class IgoForm : Form
    {

        private static IgoForm _instance;
        Graphics grfx;
        enum Turn {Black, White}
        Turn turn = Turn.Black;

        public IgoForm()
        {
            InitializeComponent();
        }
        public static IgoForm Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new IgoForm();
                return _instance;
            }
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            grfx = Graphics.FromImage(pictureBox1.Image);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("({0}, {1})", e.X, e.Y);
            int xpos = (e.X - 24) / 21;
            int ypos = (e.Y - 26) / 21;
            putStone(xpos, ypos);
        }
        public void putStone(int xpos, int ypos)
        {
            if (turn == Turn.Black)
            {
                grfx.FillEllipse(Brushes.Black, xpos * 21 + 24, ypos * 21 + 26, 21, 21);
                turn = Turn.White;
            }
            else
            {
                grfx.FillEllipse(Brushes.White, xpos * 21 + 24, ypos * 21 + 26, 21, 21);
                turn = Turn.Black;
            }
            System.Media.SystemSounds.Asterisk.Play();
            pictureBox1.Refresh();
        }
        public int[] a1ToConvart(string pos, bool i_after, bool single_figure)
        {
            int xpos = pos[0] - 'a';
            if (i_after) xpos -= 1;
            if (single_figure)
                pos = pos.Substring(1, 1);
            else
                pos = pos.Substring(1, 2);
            int ypos = 19 - int.Parse(pos);
            Debug.WriteLine("({0}, {1})", xpos, ypos);
            return new int[]{xpos, ypos};
        }
        public void chatToPut(string chat)
        {
            int[] pos;
            chat = chat.ToLower();
            if (System.Text.RegularExpressions.Regex.IsMatch(chat,
                "^[a-h]1[0-9]"))  //数字が二桁の場合
            {
                pos = a1ToConvart(chat, false, false);
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(chat,
                "^[j-t]1[0-9]"))  //数字が二桁の場合
            {
                pos = a1ToConvart(chat, true, false);
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(chat,
                "^[a-h][1-9]"))   //数字が一桁の場合
            {
                pos = a1ToConvart(chat, false, true);
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(chat,
                "^[j-t][1-9]"))   //数字が一桁の場合
            {
                pos = a1ToConvart(chat, true, true);
            }
            else
                return;

            putStone(pos[0], pos[1]);
        }
    }
}
