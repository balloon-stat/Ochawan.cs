using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Ochawan
{
    public partial class CommunityForm : Form
    {
        public CommunityForm()
        {
            InitializeComponent();
        }

        private void CommunityForm_Load(object sender, EventArgs e)
        {
            try
            {
                using(var sr = new StreamReader("CommunityFile.txt"))
                {
                    while (sr.Peek() != -1)
                    {
                        dataGridView1.Rows.Add(sr.ReadLine().Split(','));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Message    : " + ex.Message);
                Debug.WriteLine("Type       : " + ex.GetType().FullName);
                Debug.WriteLine("StackTrace : " + ex.StackTrace.ToString());
                MessageBox.Show("CommunityFile.txtが存在しない、または壊れています");
            }
	{
		 
	}
        }

        private void CommunityForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                using (var sw = new StreamWriter("CommunityFile.txt"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells[0].Value == null || (string)row.Cells[0].Value == "")
                            continue;
                        sw.WriteLine( row.Cells[0].Value +
                                "," + row.Cells[1].Value + 
                                "," + row.Cells[2].Value );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Message    : " + ex.Message);
                Debug.WriteLine("Type       : " + ex.GetType().FullName);
                Debug.WriteLine("StackTrace : " + ex.StackTrace.ToString());
                MessageBox.Show("CommunityFile.txtに書き込みできませんでした");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var comID = dataGridView1.Rows[e.RowIndex].Cells[1].Value;
            Process.Start("http://com.nicovideo.jp/live_archives/" + comID);
        }
    }
}
