using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceShooter
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

    

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Form1 form = new Form1();

            if (checkBox1.Checked) {
               
                Options.bg_player.Play(); 

            }
            else {
               
                Options.bg_player.Stop();
            }
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {

        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены что хотите выйти?");
            if (result == DialogResult.No) 
            { 
                e.Cancel = true;
            }
            else
            {

            }
       
        }
    }
}
