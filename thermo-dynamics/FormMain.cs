using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thermo_dynamics
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonShoMineralInfo_Click(object sender, EventArgs e)
        {
            FormMineralCreate.ShowMineralCreatePanel();
        }

        private void buttonPTProfile_Click(object sender, EventArgs e)
        {
            FormPTProfile.ShowForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormMixture.ShowForm();
        }
    }
}
