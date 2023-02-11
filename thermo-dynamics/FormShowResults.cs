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
    public partial class FormShowResults : Form
    {
        public FormShowResults()
        {
            InitializeComponent();
        }

        public static void ShowResultsSummary(PTProfileCalculator calculator)
        {
            var f = new FormShowResults();
            f.textBoxMineral.Text = $"{calculator.Mineral.MineralName} ({calculator.Mineral.PaperName})";
            f.textBoxProfile.Text = calculator.Profile.Name;
            var dt = new DataTable();
            ResultSummary.ColumnStrs.ForEach(headerText =>
            {
                dt.Columns.Add(headerText, typeof(double));
            });
            calculator.DoProfileCalculationAsSummary().ForEach(ret =>
            {
                var row = dt.NewRow();
                row[ResultSummary.ColumnStrs[0]] = Math.Round(ret.GivenP, 2);
                row[ResultSummary.ColumnStrs[1]] = Math.Round(ret.GivenT,2);
                row[ResultSummary.ColumnStrs[2]] = Math.Round(ret.Vp,2);
                row[ResultSummary.ColumnStrs[3]] = Math.Round(ret.Vs,2);
                row[ResultSummary.ColumnStrs[4]] = Math.Round(ret.Vb,2);
                row[ResultSummary.ColumnStrs[5]] = Math.Round(ret.Density, 4);
                row[ResultSummary.ColumnStrs[6]] = Math.Round(ret.Volume,4);
                row[ResultSummary.ColumnStrs[7]] = Math.Round(ret.KS,2);
                row[ResultSummary.ColumnStrs[8]] = Math.Round(ret.KT,2);
                row[ResultSummary.ColumnStrs[9]] = Math.Round(ret.GS,2);
                row[ResultSummary.ColumnStrs[10]] = Math.Round(ret.Alpha,4);
                row[ResultSummary.ColumnStrs[11]] = Math.Round(ret.DebyeTemp,2);
                row[ResultSummary.ColumnStrs[12]] = Math.Round(ret.Gamma,3);
                row[ResultSummary.ColumnStrs[13]] = Math.Round(ret.EthaS,2);
                row[ResultSummary.ColumnStrs[14]] = Math.Round(ret.Q,2);
                dt.Rows.Add(row);
            });
            f.dataGridViewShowData.AllowUserToAddRows = false;
            f.dataGridViewShowData.DataSource = dt;
            f.dataGridViewShowData.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => { column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; });
            f.dataGridViewShowData.ReadOnly = true;
            f.ShowDialog();
            f.Dispose();
        }

        public static void ShowResultsSummary(VProfileCalculator v)
        {
            var f = new FormShowResults();
            f.textBoxMineral.Text = $"{v.Name} (Hill)";
            f.textBoxProfile.Text = $"{v.Elem1.GivenP} GPa, {v.Elem1.GivenT} K";
            var dt = new DataTable();
            dt.Columns.Add("Elem2", typeof(string));
            v.Elem1RatioList.ForEach(ratio =>
            {
                dt.Columns.Add(ratio.ToString("0.00"), typeof(double));
            });
            dt.Columns.Add("Elem1", typeof(string));
            var rowDensity = dt.NewRow();
            rowDensity["Elem2"] = ResultSummary.ColumnStrs[5];
            var rowVolume = dt.NewRow();
            rowVolume["Elem2"] = ResultSummary.ColumnStrs[6];
            var rowVp = dt.NewRow();
            rowVp["Elem2"] = ResultSummary.ColumnStrs[2];
            var rowVs = dt.NewRow();
            rowVs["Elem2"] = ResultSummary.ColumnStrs[3];
            var rowVb = dt.NewRow();
            rowVb["Elem2"] = ResultSummary.ColumnStrs[4];
            var rowKS = dt.NewRow();
            rowKS["Elem2"] = ResultSummary.ColumnStrs[7];
            var rowKT = dt.NewRow();
            rowKT["Elem2"] = ResultSummary.ColumnStrs[8];
            var rowGS = dt.NewRow();
            rowGS["Elem2"] = ResultSummary.ColumnStrs[9];

            v.HillResults().ForEach(hill => 
            {
                var tag = hill.elem1Ratio.ToString("0.00");
                rowDensity[tag] = hill.ret.Density;
                rowVolume[tag] = hill.ret.Volume;
                rowVp[tag] = hill.ret.Vp;
                rowVs[tag] = hill.ret.Vs;
                rowVb[tag] = hill.ret.Vb;
                rowKS[tag] = hill.ret.KS;
                rowKT[tag] = hill.ret.KT;
                rowGS[tag] = hill.ret.GS;                
            });

            f.dataGridViewShowData.AllowUserToAddRows = false;
            f.dataGridViewShowData.DataSource = dt;
            f.dataGridViewShowData.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => { column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; });
            f.dataGridViewShowData.ReadOnly = true;
            f.ShowDialog();
            f.Dispose();
        }

        private void buttonExportCSV_Click(object sender, EventArgs e)
        {
            if(saveFileDialogExportCSV.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if(ExportCSV(saveFileDialogExportCSV.FileName, dataGridViewShowData))
            {
                MessageBox.Show("Exported.", Application.ProductName,  MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failured.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ExportCSV(string filePath, DataGridView dgv)
        {
            StreamWriter sw = null;
            bool ret = false;
            try
            {
                sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                string s = "";

                for (int iCol = 0; iCol < dgv.Columns.Count; iCol++)
                {
                    String sCell = dgv.Columns[iCol].HeaderCell.Value.ToString();

                    if (iCol > 0)
                    {
                        s += ",";
                    }

                    s += quoteCommaReplacer(sCell);
                }
                sw.WriteLine(s);

                int maxRowsCount = dgv.Rows.Count;
                if (dgv.AllowUserToAddRows)
                {
                    maxRowsCount = maxRowsCount - 1;
                }

                for (int iRow = 0; iRow < maxRowsCount; iRow++)
                {
                    s = "";

                    for (int iCol = 0; iCol < dgv.Columns.Count; iCol++)
                    {
                        String sCell = dgv[iCol, iRow].Value.ToString();

                        if (iCol > 0)
                        {
                            s += ",";
                        }

                        s += quoteCommaReplacer(sCell);

                    }
                    sw.WriteLine(s);
                }
                ret = true;
            }
            catch
            {

            }
            finally
            {
                sw.Close();
            }
            return ret;
        }

        private string quoteCommaReplacer(string sCell)
        {
            const string QUOTE = @""""; // 「"」
            const string COMMA = @",";  // 「,」

            string[] a = new string[] { QUOTE, COMMA };

            if (a.Any(sCell.Contains))
            {
                sCell = sCell.Replace(QUOTE, QUOTE + QUOTE);
                sCell = QUOTE + sCell + QUOTE;
            }
            return sCell;
        }
    }
}
