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
            saveFileDialogExportCSV.InitialDirectory = FormMain.ResultsDirPath;
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

        public static void ShowResultsSummary(VProfileCalculator v, MixtureMethod method=MixtureMethod.Hill)
        {
            var f = new FormShowResults();
            f.textBoxMineral.Text = $"{v.Name} ({Enum.GetName(typeof(MixtureMethod), method)})";
            f.textBoxProfile.Text = $"{v.Elem1.GivenP.ToString("0.00")} GPa, {v.Elem1.GivenT} K";
            var dt = new DataTable();
            var columTag = "Param";
            dt.Columns.Add(columTag, typeof(string));
            var columnElem2 = v.Elem2.Name;
            dt.Columns.Add(columnElem2, typeof(double));
            v.Elem1RatioList.ForEach(ratio =>
            {
                dt.Columns.Add((ratio*100.0d).ToString("0.00"), typeof(double));
            });
            var columnElem1 = v.Elem1.Name;
            dt.Columns.Add(columnElem1, typeof(double));
            var rowDensity = dt.NewRow();
            rowDensity[columTag] = ResultSummary.ColumnStrs[5];
            rowDensity[columnElem2] = v.Elem2.Density;
            rowDensity[columnElem1] = v.Elem1.Density;
            var rowVolume = dt.NewRow();
            rowVolume[columTag] = ResultSummary.ColumnStrs[6];
            rowVolume[columnElem2] = v.Elem2.Volume;
            rowVolume[columnElem1] = v.Elem1.Volume;
            var rowVp = dt.NewRow();
            rowVp[columTag] = ResultSummary.ColumnStrs[2];
            rowVp[columnElem2] = v.Elem2.Vp;
            rowVp[columnElem1] = v.Elem1.Vp;
            var rowVs = dt.NewRow();
            rowVs[columTag] = ResultSummary.ColumnStrs[3];
            rowVs[columnElem2] = v.Elem2.Vs;
            rowVs[columnElem1] = v.Elem1.Vs;
            var rowVb = dt.NewRow();
            rowVb[columTag] = ResultSummary.ColumnStrs[4];
            rowVb[columnElem2] = v.Elem2.Vb;
            rowVb[columnElem1] = v.Elem1.Vb;
            var rowKS = dt.NewRow();
            rowKS[columTag] = ResultSummary.ColumnStrs[7];
            rowKS[columnElem2] = v.Elem2.KS;
            rowKS[columnElem1] = v.Elem1.KS;
            var rowKT = dt.NewRow();
            rowKT[columTag] = ResultSummary.ColumnStrs[8];
            rowKT[columnElem2] = v.Elem2.KT;
            rowKT[columnElem1] = v.Elem1.KT;
            var rowGS = dt.NewRow();
            rowGS[columTag] = ResultSummary.ColumnStrs[9];
            rowGS[columnElem2] = v.Elem2.GS;
            rowGS[columnElem1] = v.Elem1.GS;
            switch (method)
            {
                case MixtureMethod.Hill:
                    v.HillResults().ForEach(summary =>
                    {
                        var tag = (summary.elem1Ratio * 100.0d).ToString("0.00");
                        rowDensity[tag] = summary.ret.Density;
                        rowVolume[tag] = summary.ret.Volume;
                        rowVp[tag] = summary.ret.Vp;
                        rowVs[tag] = summary.ret.Vs;
                        rowVb[tag] = summary.ret.Vb;
                        rowKS[tag] = summary.ret.KS;
                        rowKT[tag] = summary.ret.KT;
                        rowGS[tag] = summary.ret.GS;
                    });
                    break;
                case MixtureMethod.Voigt:
                    v.VoigtResults().ForEach(summary =>
                    {
                        var tag = (summary.elem1Ratio * 100.0d).ToString("0.00");
                        rowDensity[tag] = summary.ret.Density;
                        rowVolume[tag] = summary.ret.Volume;
                        rowVp[tag] = summary.ret.Vp;
                        rowVs[tag] = summary.ret.Vs;
                        rowVb[tag] = summary.ret.Vb;
                        rowKS[tag] = summary.ret.KS;
                        rowKT[tag] = summary.ret.KT;
                        rowGS[tag] = summary.ret.GS;
                    });
                    break;
                case MixtureMethod.Reuss:
                    v.ReussResults().ForEach(summary =>
                    {
                        var tag = (summary.elem1Ratio * 100.0d).ToString("0.00");
                        rowDensity[tag] = summary.ret.Density;
                        rowVolume[tag] = summary.ret.Volume;
                        rowVp[tag] = summary.ret.Vp;
                        rowVs[tag] = summary.ret.Vs;
                        rowVb[tag] = summary.ret.Vb;
                        rowKS[tag] = summary.ret.KS;
                        rowKT[tag] = summary.ret.KT;
                        rowGS[tag] = summary.ret.GS;
                    });
                    break;
                case MixtureMethod.HS:
                    v.HSResults().ForEach(summary =>
                    {
                        var tag = (summary.elem1Ratio * 100.0d).ToString("0.00");
                        rowDensity[tag] = summary.ret.Density;
                        rowVolume[tag] = summary.ret.Volume;
                        rowVp[tag] = summary.ret.Vp;
                        rowVs[tag] = summary.ret.Vs;
                        rowVb[tag] = summary.ret.Vb;
                        rowKS[tag] = summary.ret.KS;
                        rowKT[tag] = summary.ret.KT;
                        rowGS[tag] = summary.ret.GS;
                    });
                    break;
                default:
                    break;
            }

            dt.Rows.Add(rowDensity);
            dt.Rows.Add(rowVolume);
            dt.Rows.Add(rowVp);
            dt.Rows.Add(rowVs);
            dt.Rows.Add(rowVb);
            dt.Rows.Add(rowKS);
            dt.Rows.Add(rowKT);
            dt.Rows.Add(rowGS);

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
