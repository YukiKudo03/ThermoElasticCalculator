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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace thermo_dynamics
{
    public partial class FormMixture : Form
    {
        public FormMixture()
        {
            InitializeComponent();
        }

        public static void ShowForm()
        {
            var f = new FormMixture();
            f.ShowDialog();
            f.Dispose();
        }

        public MineralParams Elem1;
        public MineralParams Elem2;

        private void buttonExportVProfile_Click(object sender, EventArgs e)
        {
            if(Elem1 == null || Elem2 == null)
            {
                MessageBox.Show("Mineral must be read.");
                return;
            }
            if(saveFileDialogExportVProfile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string exportJson = new VProfile(DataGridViewToList(), Elem1, Elem2, textBoxVProfile.Text).ExportJson();
            File.WriteAllText(saveFileDialogExportVProfile.FileName, exportJson);

        }

        private List<double> DataGridViewToList()
        {
            return dataGridViewVProfile.Rows.Cast<DataGridViewRow>()
                .Where(row => !object.Equals(row.Cells[0].Value, null))
                .Where(row => double.TryParse(row.Cells[0].Value.ToString(), out double test))
                .Select(row => double.Parse(row.Cells[ColumnVolumeRatio.Index].Value.ToString()) / 100.0d)
                .ToList();
        }

        private void buttonReadElem2_Click(object sender, EventArgs e)
        {
            if (openFileDialogReadElem2.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogReadElem2.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            Elem2 = new MineralParams();
            if (MineralParams.ImportJson(jsonText, out Elem2))
            {
                textBoxMineralPropertyElem2.Text = Elem2.ParamSymbol;
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        private void buttonReadElem1_Click(object sender, EventArgs e)
        {
            if (openFileDialogReadElem1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogReadElem1.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            Elem1 = new MineralParams();
            if (MineralParams.ImportJson(jsonText, out Elem1))
            {
                textBoxMineralPropertyElem1.Text = Elem1.ParamSymbol;
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        public class VProfile
        {
            public VProfile(List<double> ratios, MineralParams elem1, MineralParams elem2, string name)
            {
                Ratios = ratios;
                Elem1 = elem1;
                Elem2 = elem2;
                Name = name;
            }
            public List<double> Ratios { get; set; }
            public MineralParams Elem1 { get; set; }
            public MineralParams Elem2 { get; set; }
            public string Name { get; set; }
            public string ExportJson()
            {
                return JsonSerializer.Serialize<VProfile>(this, new JsonSerializerOptions { WriteIndented = true });
            }

            public static bool ImportJson(string jsonString, out VProfile ret)
            {
                bool succeed = false;
                ret = null;
                try
                {
                    ret = JsonSerializer.Deserialize<VProfile>(jsonString);
                    succeed = true;
                }
                catch
                {

                }

                return succeed;
            }
        }

        private void buttonReadVProfile_Click(object sender, EventArgs e)
        {
            if(openFileDialogReadVProfile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogReadVProfile.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            if (VProfile.ImportJson(jsonText, out VProfile ret))
            {
                dataGridViewVProfile.Rows.Clear();
                ret.Ratios.ForEach(ratio =>
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dataGridViewVProfile);
                    row.Cells[ColumnVolumeRatio.Index].Value = ratio.ToString();
                    dataGridViewVProfile.Rows.Add(row);
                });
                textBoxVProfile.Text = ret.Name;
                Elem1 = ret.Elem1;
                Elem2 = ret.Elem2;
                textBoxMineralPropertyElem1.Text = Elem1.ParamSymbol;
                textBoxMineralPropertyElem2.Text = Elem2.ParamSymbol;
            }
            else
            {
                MessageBox.Show("読み込みに失敗しました");
            }
        }

        private void buttonExecCalculate_Click(object sender, EventArgs e)
        {
            var elem1Res = new MieGruneisenEOSOptimizer(
                Elem1,
                new PTData {
                    Pressure = (double)numericUpDownPressure.Value,
                    Temperature = (double)numericUpDownTemperature.Value,
                });
            var elem2Res = new MieGruneisenEOSOptimizer(
                Elem2,
                new PTData
                {
                    Pressure = (double)numericUpDownPressure.Value,
                    Temperature = (double)numericUpDownTemperature.Value,
                });
            VProfileCalculator vProfileCalculator = new VProfileCalculator(DataGridViewToList(), elem1Res.ExecOptimize().ExportResults(), elem2Res.ExecOptimize().ExportResults(), textBoxVProfile.Text);
            FormShowResults.ShowResultsSummary(vProfileCalculator);
        }
    }
}
