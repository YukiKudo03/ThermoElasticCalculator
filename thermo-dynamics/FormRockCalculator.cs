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
    public partial class FormRockCalculator : Form
    {
        public FormRockCalculator()
        {
            InitializeComponent();
            comboBoxMixMethod.Items.AddRange(Enum.GetNames(typeof(MixtureMethod)));
            comboBoxMixMethod.SelectedIndex = 0;
        }

        public static void ShowForm()
        {
            var f = new FormRockCalculator();
            f.ShowDialog();
            f.Dispose();
        }

        private List<(MineralParams mineral, double ratio)> _mineralEntries = new List<(MineralParams mineral, double ratio)>();

        /// <summary>
        /// Add a mineral from a .mine file
        /// </summary>
        private void buttonAddMineral_Click(object sender, EventArgs e)
        {
            openFileDialogMineral.InitialDirectory = FormMain.MineralDirPath;
            openFileDialogMineral.Filter = "MineralParam Files|*.mine|All Files|*.*";
            if (openFileDialogMineral.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogMineral.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            if (MineralParams.ImportJson(jsonText, out MineralParams mineral))
            {
                var row = new DataGridViewRow();
                row.CreateCells(dataGridViewMinerals);
                row.Cells[ColumnMineralName.Index].Value = mineral.ParamSymbol;
                row.Cells[ColumnVolumeRatio.Index].Value = "0.0";
                row.Tag = mineral;
                dataGridViewMinerals.Rows.Add(row);
            }
            else
            {
                MessageBox.Show("Failed to read mineral file.");
            }
        }

        /// <summary>
        /// Remove selected mineral from list
        /// </summary>
        private void buttonRemoveMineral_Click(object sender, EventArgs e)
        {
            if (dataGridViewMinerals.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewMinerals.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dataGridViewMinerals.Rows.Remove(row);
                    }
                }
            }
        }

        /// <summary>
        /// Load a saved rock composition (.rock file)
        /// </summary>
        private void buttonLoadRock_Click(object sender, EventArgs e)
        {
            openFileDialogRock.InitialDirectory = FormMain.MineralDirPath;
            openFileDialogRock.Filter = "Rock Files|*.rock|All Files|*.*";
            if (openFileDialogRock.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string jsonText;
            using (var sr = new StreamReader(openFileDialogRock.FileName, Encoding.UTF8, true))
            {
                jsonText = sr.ReadToEnd();
            }
            if (RockComposition.ImportJson(jsonText, out RockComposition rock))
            {
                textBoxRockName.Text = rock.Name;
                dataGridViewMinerals.Rows.Clear();
                foreach (var entry in rock.Minerals)
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dataGridViewMinerals);
                    row.Cells[ColumnMineralName.Index].Value = entry.Mineral.ParamSymbol;
                    row.Cells[ColumnVolumeRatio.Index].Value = entry.VolumeRatio.ToString("0.0000");
                    row.Tag = entry.Mineral;
                    dataGridViewMinerals.Rows.Add(row);
                }
            }
            else
            {
                MessageBox.Show("Failed to read rock file.");
            }
        }

        /// <summary>
        /// Save rock composition as .rock file
        /// </summary>
        private void buttonSaveRock_Click(object sender, EventArgs e)
        {
            var rock = BuildRockComposition();
            if (rock == null) return;

            saveFileDialogRock.InitialDirectory = FormMain.MineralDirPath;
            saveFileDialogRock.Filter = "Rock Files|*.rock";
            if (saveFileDialogRock.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            File.WriteAllText(saveFileDialogRock.FileName, rock.ExportJson(), Encoding.UTF8);
            MessageBox.Show("Rock composition saved.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Calculate rock properties at given P-T
        /// </summary>
        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            var rock = BuildRockComposition();
            if (rock == null) return;

            if (rock.Minerals.Count == 0)
            {
                MessageBox.Show("No minerals added.");
                return;
            }

            try
            {
                double pressure = (double)numericUpDownPressure.Value;
                double temperature = (double)numericUpDownTemperature.Value;
                var method = (MixtureMethod)Enum.Parse(typeof(MixtureMethod), comboBoxMixMethod.SelectedItem.ToString());

                var calculator = new RockCalculator(rock, pressure, temperature, method);
                var (mixedResult, individualResults) = calculator.Calculate();

                if (mixedResult == null)
                {
                    MessageBox.Show("Calculation failed.");
                    return;
                }

                // Show results
                FormShowResults.ShowRockResults(rock.Name, method, mixedResult, individualResults);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Calculation error: {ex.Message}");
            }
        }

        private RockComposition BuildRockComposition()
        {
            var rock = new RockComposition { Name = textBoxRockName.Text };
            foreach (DataGridViewRow row in dataGridViewMinerals.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Tag == null) continue;
                var mineral = row.Tag as MineralParams;
                if (mineral == null) continue;

                double ratio = 0;
                if (row.Cells[ColumnVolumeRatio.Index].Value != null)
                {
                    double.TryParse(row.Cells[ColumnVolumeRatio.Index].Value.ToString(), out ratio);
                }
                rock.Minerals.Add(new RockMineralEntry { Mineral = mineral, VolumeRatio = ratio });
            }
            return rock;
        }
    }
}
