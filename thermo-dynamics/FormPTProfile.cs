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
    public partial class FormPTProfile : Form
    {
        public FormPTProfile()
        {
            InitializeComponent();
        }

        private MineralParams Mineral;

        public static void ShowForm()
        {
            var f = new FormPTProfile();
            f.ShowDialog();
            f.Dispose();
        }

        private void buttonReadMineralProperty_Click(object sender, EventArgs e)
        {
            if(openFileDialogReadMineralProperty.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogReadMineralProperty.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            Mineral = new MineralParams();
            if (MineralParams.ImportJson(jsonText, out Mineral))
            {
                textBoxMineralProperty.Text = Mineral.ParamSymbol;
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        private List<PTData> DataGridViewToPTDatas()
        {
            return dataGridViewPTProfile.Rows.Cast<DataGridViewRow>()
                .Where(row => !object.Equals(row.Cells[0].Value, null) && !object.Equals(row.Cells[1].Value, null))
                .Where(row => double.TryParse(row.Cells[0].Value.ToString(), out double test) && double.TryParse(row.Cells[1].Value.ToString(), out double test1))
                .Select(
                    row => new PTData
                    {
                        Pressure = double.Parse(row.Cells[ColumnPressure.Index].Value.ToString()),
                        Temperature = double.Parse(row.Cells[ColumnTemperature.Index].Value.ToString())
                    }).ToList();
        }

        private void buttonExportPTProfile_Click(object sender, EventArgs e)
        {
            if(saveFileDialogExportPTProfile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var profile = new PTProfile
            {
                Name = textBoxProfileName.Text,
                Profile = DataGridViewToPTDatas(),
            };
            File.WriteAllText(saveFileDialogExportPTProfile.FileName, profile.ExportJson());
        }

        private void buttonReadPTProfile_Click(object sender, EventArgs e)
        {
            if(openFileDialogReadPTProfile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogReadPTProfile.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            var ret = new PTProfile();
            if (PTProfile.ImportJson(jsonText, out ret))
            {
                dataGridViewPTProfile.Rows.Clear();
                ret.Profile.ForEach(pt =>
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dataGridViewPTProfile);
                    row.Cells[ColumnPressure.Index].Value = pt.Pressure.ToString();
                    row.Cells[ColumnTemperature.Index].Value = pt.Temperature.ToString();
                    dataGridViewPTProfile.Rows.Add(row);
                });
                textBoxProfileName.Text = ret.Name;
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            var ptProfile = new PTProfile
            {
                Name = textBoxProfileName.Text,
                Profile = DataGridViewToPTDatas(),
            };

            var profileCalculator = new PTProfileCalculator(Mineral, ptProfile);
            FormShowResults.ShowResultsSummary(profileCalculator);
        }
    }
}
