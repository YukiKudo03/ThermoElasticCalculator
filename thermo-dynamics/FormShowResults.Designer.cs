
namespace thermo_dynamics
{
    partial class FormShowResults
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewShowData = new System.Windows.Forms.DataGridView();
            this.labelMineral = new System.Windows.Forms.Label();
            this.textBoxMineral = new System.Windows.Forms.TextBox();
            this.textBoxProfile = new System.Windows.Forms.TextBox();
            this.labelProfile = new System.Windows.Forms.Label();
            this.buttonExportCSV = new System.Windows.Forms.Button();
            this.saveFileDialogExportCSV = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShowData)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewShowData
            // 
            this.dataGridViewShowData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewShowData.Location = new System.Drawing.Point(12, 53);
            this.dataGridViewShowData.Name = "dataGridViewShowData";
            this.dataGridViewShowData.RowHeadersWidth = 51;
            this.dataGridViewShowData.RowTemplate.Height = 24;
            this.dataGridViewShowData.Size = new System.Drawing.Size(1413, 508);
            this.dataGridViewShowData.TabIndex = 0;
            // 
            // labelMineral
            // 
            this.labelMineral.AutoSize = true;
            this.labelMineral.Location = new System.Drawing.Point(12, 20);
            this.labelMineral.Name = "labelMineral";
            this.labelMineral.Size = new System.Drawing.Size(55, 15);
            this.labelMineral.TabIndex = 1;
            this.labelMineral.Text = "Mineral:";
            // 
            // textBoxMineral
            // 
            this.textBoxMineral.Location = new System.Drawing.Point(73, 17);
            this.textBoxMineral.Name = "textBoxMineral";
            this.textBoxMineral.ReadOnly = true;
            this.textBoxMineral.Size = new System.Drawing.Size(496, 22);
            this.textBoxMineral.TabIndex = 2;
            // 
            // textBoxProfile
            // 
            this.textBoxProfile.Location = new System.Drawing.Point(687, 17);
            this.textBoxProfile.Name = "textBoxProfile";
            this.textBoxProfile.ReadOnly = true;
            this.textBoxProfile.Size = new System.Drawing.Size(496, 22);
            this.textBoxProfile.TabIndex = 4;
            // 
            // labelProfile
            // 
            this.labelProfile.AutoSize = true;
            this.labelProfile.Location = new System.Drawing.Point(626, 20);
            this.labelProfile.Name = "labelProfile";
            this.labelProfile.Size = new System.Drawing.Size(56, 15);
            this.labelProfile.TabIndex = 3;
            this.labelProfile.Text = "Profile: ";
            // 
            // buttonExportCSV
            // 
            this.buttonExportCSV.Location = new System.Drawing.Point(1220, 14);
            this.buttonExportCSV.Name = "buttonExportCSV";
            this.buttonExportCSV.Size = new System.Drawing.Size(205, 27);
            this.buttonExportCSV.TabIndex = 5;
            this.buttonExportCSV.Text = "Export...";
            this.buttonExportCSV.UseVisualStyleBackColor = true;
            this.buttonExportCSV.Click += new System.EventHandler(this.buttonExportCSV_Click);
            // 
            // saveFileDialogExportCSV
            // 
            this.saveFileDialogExportCSV.DefaultExt = "csv";
            this.saveFileDialogExportCSV.Filter = "csv Files|*.csv";
            // 
            // FormShowResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1437, 573);
            this.Controls.Add(this.buttonExportCSV);
            this.Controls.Add(this.textBoxProfile);
            this.Controls.Add(this.labelProfile);
            this.Controls.Add(this.textBoxMineral);
            this.Controls.Add(this.labelMineral);
            this.Controls.Add(this.dataGridViewShowData);
            this.Name = "FormShowResults";
            this.Text = "Summary";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewShowData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewShowData;
        private System.Windows.Forms.Label labelMineral;
        private System.Windows.Forms.TextBox textBoxMineral;
        private System.Windows.Forms.TextBox textBoxProfile;
        private System.Windows.Forms.Label labelProfile;
        private System.Windows.Forms.Button buttonExportCSV;
        private System.Windows.Forms.SaveFileDialog saveFileDialogExportCSV;
    }
}