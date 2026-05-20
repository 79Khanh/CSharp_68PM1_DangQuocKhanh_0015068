using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLySinhVien
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        private void quảnLíSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UcQLSV qlsv = new UcQLSV();
            panel1.Controls.Clear();
            panel1.Controls.Add(qlsv);
        }

        private void quanlisinhvien_Load(object sender, EventArgs e)
        {
            UcQLSV qlsv = new UcQLSV();
            panel1.Controls.Clear();
            panel1.Controls.Add(qlsv);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void quảnLýLớpHọcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UcQLLH qllh = new UcQLLH();
            panel1.Controls.Clear();
            panel1.Controls.Add(qllh);
        }
    }
}
