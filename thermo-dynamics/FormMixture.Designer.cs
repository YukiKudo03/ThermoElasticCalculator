
namespace thermo_dynamics
{
    partial class FormMixture
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
            this.textBoxMineralPropertyElem1 = new System.Windows.Forms.TextBox();
            this.buttonReadElem1 = new System.Windows.Forms.Button();
            this.textBoxMineralPropertyElem2 = new System.Windows.Forms.TextBox();
            this.buttonReadElem2 = new System.Windows.Forms.Button();
            this.numericUpDownPressure = new System.Windows.Forms.NumericUpDown();
            this.labelPressure = new System.Windows.Forms.Label();
            this.labelTemperature = new System.Windows.Forms.Label();
            this.numericUpDownTemperature = new System.Windows.Forms.NumericUpDown();
            this.dataGridViewVProfile = new System.Windows.Forms.DataGridView();
            this.ColumnVolumeRatio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonReadVProfile = new System.Windows.Forms.Button();
            this.textBoxVProfile = new System.Windows.Forms.TextBox();
            this.buttonExportVProfile = new System.Windows.Forms.Button();
            this.saveFileDialogExportVProfile = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialogReadElem1 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogReadElem2 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogReadVProfile = new System.Windows.Forms.OpenFileDialog();
            this.buttonExecCalculate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPressure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVProfile)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxMineralPropertyElem1
            // 
            this.textBoxMineralPropertyElem1.Location = new System.Drawing.Point(12, 12);
            this.textBoxMineralPropertyElem1.Name = "textBoxMineralPropertyElem1";
            this.textBoxMineralPropertyElem1.ReadOnly = true;
            this.textBoxMineralPropertyElem1.Size = new System.Drawing.Size(303, 22);
            this.textBoxMineralPropertyElem1.TabIndex = 3;
            this.textBoxMineralPropertyElem1.Text = "MineralName of Elem1";
            // 
            // buttonReadElem1
            // 
            this.buttonReadElem1.Location = new System.Drawing.Point(321, 8);
            this.buttonReadElem1.Name = "buttonReadElem1";
            this.buttonReadElem1.Size = new System.Drawing.Size(124, 29);
            this.buttonReadElem1.TabIndex = 2;
            this.buttonReadElem1.Text = "ReadElem1..";
            this.buttonReadElem1.UseVisualStyleBackColor = true;
            this.buttonReadElem1.Click += new System.EventHandler(this.buttonReadElem1_Click);
            // 
            // textBoxMineralPropertyElem2
            // 
            this.textBoxMineralPropertyElem2.Location = new System.Drawing.Point(12, 40);
            this.textBoxMineralPropertyElem2.Name = "textBoxMineralPropertyElem2";
            this.textBoxMineralPropertyElem2.ReadOnly = true;
            this.textBoxMineralPropertyElem2.Size = new System.Drawing.Size(303, 22);
            this.textBoxMineralPropertyElem2.TabIndex = 5;
            this.textBoxMineralPropertyElem2.Text = "MineralName of Elem2\r\n";
            // 
            // buttonReadElem2
            // 
            this.buttonReadElem2.Location = new System.Drawing.Point(321, 36);
            this.buttonReadElem2.Name = "buttonReadElem2";
            this.buttonReadElem2.Size = new System.Drawing.Size(124, 29);
            this.buttonReadElem2.TabIndex = 4;
            this.buttonReadElem2.Text = "ReadElem2..";
            this.buttonReadElem2.UseVisualStyleBackColor = true;
            this.buttonReadElem2.Click += new System.EventHandler(this.buttonReadElem2_Click);
            // 
            // numericUpDownPressure
            // 
            this.numericUpDownPressure.DecimalPlaces = 2;
            this.numericUpDownPressure.Location = new System.Drawing.Point(70, 75);
            this.numericUpDownPressure.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownPressure.Name = "numericUpDownPressure";
            this.numericUpDownPressure.Size = new System.Drawing.Size(119, 22);
            this.numericUpDownPressure.TabIndex = 6;
            // 
            // labelPressure
            // 
            this.labelPressure.AutoSize = true;
            this.labelPressure.Location = new System.Drawing.Point(12, 77);
            this.labelPressure.Name = "labelPressure";
            this.labelPressure.Size = new System.Drawing.Size(52, 15);
            this.labelPressure.TabIndex = 7;
            this.labelPressure.Text = "P[GPa]";
            // 
            // labelTemperature
            // 
            this.labelTemperature.AutoSize = true;
            this.labelTemperature.Location = new System.Drawing.Point(208, 77);
            this.labelTemperature.Name = "labelTemperature";
            this.labelTemperature.Size = new System.Drawing.Size(43, 15);
            this.labelTemperature.TabIndex = 9;
            this.labelTemperature.Text = "T[K]: ";
            // 
            // numericUpDownTemperature
            // 
            this.numericUpDownTemperature.DecimalPlaces = 2;
            this.numericUpDownTemperature.Location = new System.Drawing.Point(257, 75);
            this.numericUpDownTemperature.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTemperature.Name = "numericUpDownTemperature";
            this.numericUpDownTemperature.Size = new System.Drawing.Size(119, 22);
            this.numericUpDownTemperature.TabIndex = 8;
            // 
            // dataGridViewVProfile
            // 
            this.dataGridViewVProfile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewVProfile.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnVolumeRatio});
            this.dataGridViewVProfile.Location = new System.Drawing.Point(12, 154);
            this.dataGridViewVProfile.Name = "dataGridViewVProfile";
            this.dataGridViewVProfile.RowHeadersWidth = 51;
            this.dataGridViewVProfile.RowTemplate.Height = 24;
            this.dataGridViewVProfile.Size = new System.Drawing.Size(329, 428);
            this.dataGridViewVProfile.TabIndex = 10;
            // 
            // ColumnVolumeRatio
            // 
            this.ColumnVolumeRatio.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnVolumeRatio.HeaderText = "Elem1Vol[vol.%]";
            this.ColumnVolumeRatio.MinimumWidth = 6;
            this.ColumnVolumeRatio.Name = "ColumnVolumeRatio";
            // 
            // buttonReadVProfile
            // 
            this.buttonReadVProfile.Location = new System.Drawing.Point(12, 117);
            this.buttonReadVProfile.Name = "buttonReadVProfile";
            this.buttonReadVProfile.Size = new System.Drawing.Size(140, 31);
            this.buttonReadVProfile.TabIndex = 11;
            this.buttonReadVProfile.Text = "ReadVProfile....";
            this.buttonReadVProfile.UseVisualStyleBackColor = true;
            this.buttonReadVProfile.Click += new System.EventHandler(this.buttonReadVProfile_Click);
            // 
            // textBoxVProfile
            // 
            this.textBoxVProfile.Location = new System.Drawing.Point(158, 122);
            this.textBoxVProfile.Name = "textBoxVProfile";
            this.textBoxVProfile.Size = new System.Drawing.Size(287, 22);
            this.textBoxVProfile.TabIndex = 12;
            this.textBoxVProfile.Text = "VProfile Name...";
            // 
            // buttonExportVProfile
            // 
            this.buttonExportVProfile.Location = new System.Drawing.Point(347, 154);
            this.buttonExportVProfile.Name = "buttonExportVProfile";
            this.buttonExportVProfile.Size = new System.Drawing.Size(98, 38);
            this.buttonExportVProfile.TabIndex = 13;
            this.buttonExportVProfile.Text = "Export...";
            this.buttonExportVProfile.UseVisualStyleBackColor = true;
            this.buttonExportVProfile.Click += new System.EventHandler(this.buttonExportVProfile_Click);
            // 
            // saveFileDialogExportVProfile
            // 
            this.saveFileDialogExportVProfile.DefaultExt = "vpf";
            this.saveFileDialogExportVProfile.Filter = "VProfile Files|*.vpf";
            // 
            // openFileDialogReadElem1
            // 
            this.openFileDialogReadElem1.DefaultExt = "mine";
            this.openFileDialogReadElem1.FileName = "openFileDialog1";
            this.openFileDialogReadElem1.Filter = "MineralParam Files|*.mine";
            // 
            // openFileDialogReadElem2
            // 
            this.openFileDialogReadElem2.DefaultExt = "mine";
            this.openFileDialogReadElem2.FileName = "openFileDialog1";
            this.openFileDialogReadElem2.Filter = "MineralParam Files|*.mine";
            // 
            // openFileDialogReadVProfile
            // 
            this.openFileDialogReadVProfile.DefaultExt = "vpf";
            this.openFileDialogReadVProfile.FileName = "openFileDialog1";
            this.openFileDialogReadVProfile.Filter = "VProfile Files|*.vpf";
            // 
            // buttonExecCalculate
            // 
            this.buttonExecCalculate.Location = new System.Drawing.Point(347, 218);
            this.buttonExecCalculate.Name = "buttonExecCalculate";
            this.buttonExecCalculate.Size = new System.Drawing.Size(98, 38);
            this.buttonExecCalculate.TabIndex = 14;
            this.buttonExecCalculate.Text = "Calculate...";
            this.buttonExecCalculate.UseVisualStyleBackColor = true;
            this.buttonExecCalculate.Click += new System.EventHandler(this.buttonExecCalculate_Click);
            // 
            // FormMixture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 594);
            this.Controls.Add(this.buttonExecCalculate);
            this.Controls.Add(this.buttonExportVProfile);
            this.Controls.Add(this.textBoxVProfile);
            this.Controls.Add(this.buttonReadVProfile);
            this.Controls.Add(this.dataGridViewVProfile);
            this.Controls.Add(this.labelTemperature);
            this.Controls.Add(this.numericUpDownTemperature);
            this.Controls.Add(this.labelPressure);
            this.Controls.Add(this.numericUpDownPressure);
            this.Controls.Add(this.textBoxMineralPropertyElem2);
            this.Controls.Add(this.buttonReadElem2);
            this.Controls.Add(this.textBoxMineralPropertyElem1);
            this.Controls.Add(this.buttonReadElem1);
            this.Name = "FormMixture";
            this.Text = "FormMixture";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPressure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVProfile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxMineralPropertyElem1;
        private System.Windows.Forms.Button buttonReadElem1;
        private System.Windows.Forms.TextBox textBoxMineralPropertyElem2;
        private System.Windows.Forms.Button buttonReadElem2;
        private System.Windows.Forms.NumericUpDown numericUpDownPressure;
        private System.Windows.Forms.Label labelPressure;
        private System.Windows.Forms.Label labelTemperature;
        private System.Windows.Forms.NumericUpDown numericUpDownTemperature;
        private System.Windows.Forms.DataGridView dataGridViewVProfile;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnVolumeRatio;
        private System.Windows.Forms.Button buttonReadVProfile;
        private System.Windows.Forms.TextBox textBoxVProfile;
        private System.Windows.Forms.Button buttonExportVProfile;
        private System.Windows.Forms.SaveFileDialog saveFileDialogExportVProfile;
        private System.Windows.Forms.OpenFileDialog openFileDialogReadElem1;
        private System.Windows.Forms.OpenFileDialog openFileDialogReadElem2;
        private System.Windows.Forms.OpenFileDialog openFileDialogReadVProfile;
        private System.Windows.Forms.Button buttonExecCalculate;
    }
}