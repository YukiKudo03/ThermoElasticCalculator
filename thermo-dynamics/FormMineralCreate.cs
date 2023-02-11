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
    public partial class FormMineralCreate : Form
    {
        public FormMineralCreate()
        {
            InitializeComponent();
        }

        public static void ShowMineralCreatePanel()
        {
            var f = new FormMineralCreate();
            f.ShowDialog();
            f.Dispose();
        }

        private void buttonExportFile_Click(object sender, EventArgs e)
        {
            if(saveFileDialogExportMineralFile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var mineral = GetMineralParams();
            File.WriteAllText(saveFileDialogExportMineralFile.FileName, mineral.ExportJson());
        }

        private void buttonReadFile_Click(object sender, EventArgs e)
        {
            if(openFileDialogReadMineralFile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using(var sr = new StreamReader(openFileDialogReadMineralFile.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            var ret = new MineralParams();
            if(MineralParams.ImportJson(jsonText, out ret))
            {
                SetMineralParams(ret);
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        private MineralParams GetMineralParams()
        {
            return new MineralParams
            {
                MineralName = textBoxMineralName.Text,
                PaperName = textBoxPaper.Text,
                NumAtoms = (int)numericUpDownNumAtoms.Value,
                DebyeTempZero = (double)numericUpDownDebyeTempZero.Value,
                EhtaZero = (double)numericUpDownEthaZero.Value,
                G1Prime = (double)numericUpDownG1Prime.Value,
                G2Prime = (double)numericUpDownG2Prime.Value,
                GZero = (double)numericUpDownGZero.Value,
                GammaZero = (double)numericUpDownGammaZero.Value,
                K1Prime = (double)numericUpDownK1Prime.Value,
                K2Prime = (double)numericUpDownK2Prime.Value,
                KZero = (double)numericUpDownKZero.Value,
                MolarVolume = (double)numericUpDownMolarVolume.Value,
                MolarWeight = (double)numericUpDownMolarWeight.Value,
                QZero = (double)numericUpDownQZero.Value,
                RefTemp = (double)numericUpDownRefTemp.Value,
            };
        }
        private void SetMineralParams(MineralParams mineral)
        {
            textBoxMineralName.Text = mineral.MineralName;
            textBoxPaper.Text = mineral.PaperName;
            numericUpDownNumAtoms.Value = mineral.NumAtoms;
            numericUpDownDebyeTempZero.Value = (decimal)mineral.DebyeTempZero;
            numericUpDownEthaZero.Value = (decimal)mineral.EhtaZero;
            numericUpDownG1Prime.Value = (decimal)mineral.G1Prime;
            numericUpDownG2Prime.Value = (decimal)mineral.G2Prime;
            numericUpDownGZero.Value = (decimal)mineral.GZero;
            numericUpDownGammaZero.Value = (decimal)mineral.GammaZero;
            numericUpDownK1Prime.Value = (decimal)mineral.K1Prime;
            numericUpDownK2Prime.Value = (decimal)mineral.K2Prime;
            numericUpDownKZero.Value = (decimal)mineral.KZero;
            numericUpDownMolarVolume.Value = (decimal)mineral.MolarVolume;
            numericUpDownMolarWeight.Value = (decimal)mineral.MolarWeight;
            numericUpDownQZero.Value = (decimal)mineral.QZero;
            numericUpDownRefTemp.Value = (decimal)mineral.RefTemp;
        }

        private void buttonTestCalc_Click(object sender, EventArgs e)
        {
            var mineral = GetMineralParams();
            var th = new MieGruneisenEOSOptimizer(mineral, (double)numericUpDownCalcTestPressure.Value, (double)numericUpDownCalcTestTemp.Value);
            var thResults = th.ExecOptimize();
            MessageBox.Show($"Vp: {thResults.Vp.ToString("0.00")} m/s, Vs: {thResults.Vs.ToString("0.00")} m/s, ρ: {thResults.Density.ToString("0.000")} g/cm3,  KT: {thResults.KT.ToString("0.00")} GPa, Gs: {thResults.GS.ToString("0.00")} GPa");
        }
    }
}
