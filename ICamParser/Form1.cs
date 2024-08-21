using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICamParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "RHRH303/";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var answer = "PAPA303000002010004001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000017000000000000000000000000000000000000000000000000000000000016017004000000000000000000000016000000000000017000000000000037K1800042/";
            var status = answer.Substring(7, 3);
            label5.Text = status == "000" ? "Рабоает" : "Стоит";
        }
    }
}
