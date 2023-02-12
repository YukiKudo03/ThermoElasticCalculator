using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace thermo_dynamics
{
    public partial class FormMain : Form
    {
        public static string MineralDirPath = Path.Combine(Application.StartupPath, "Minerals");
        public static string PTProfileDirPath = Path.Combine(Application.StartupPath, "PTProfiles");
        public static string VProfileDirPath = Path.Combine(Application.StartupPath, "VProfiles");
        public static string ResultsDirPath = Path.Combine(Application.StartupPath, "Results");
        public FormMain()
        {
            InitializeComponent();
            if (Directory.Exists(MineralDirPath))
            {
                Directory.CreateDirectory(MineralDirPath);
            }
            if (Directory.Exists(PTProfileDirPath))
            {
                Directory.CreateDirectory(PTProfileDirPath);
            }
            if (Directory.Exists(VProfileDirPath))
            {
                Directory.CreateDirectory(VProfileDirPath);
            }
            if (Directory.Exists(ResultsDirPath))
            {
                Directory.CreateDirectory(ResultsDirPath);
            }
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
