
namespace thermo_dynamics
{
    partial class FormPTProfile
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
            this.buttonReadMineralProperty = new System.Windows.Forms.Button();
            this.textBoxMineralProperty = new System.Windows.Forms.TextBox();
            this.openFileDialogReadMineralProperty = new System.Windows.Forms.OpenFileDialog();
            this.dataGridViewPTProfile = new System.Windows.Forms.DataGridView();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.textBoxProfileName = new System.Windows.Forms.TextBox();
            this.buttonReadPTProfile = new System.Windows.Forms.Button();
            this.buttonExportPTProfile = new System.Windows.Forms.Button();
            this.openFileDialogReadPTProfile = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogExportPTProfile = new System.Windows.Forms.SaveFileDialog();
            this.ColumnPressure = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTemperature = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPTProfile)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonReadMineralProperty
            // 
            this.buttonReadMineralProperty.Location = new System.Drawing.Point(321, 8);
            this.buttonReadMineralProperty.Name = "buttonReadMineralProperty";
            this.buttonReadMineralProperty.Size = new System.Drawing.Size(124, 29);
            this.buttonReadMineralProperty.TabIndex = 0;
            this.buttonReadMineralProperty.Text = "ReadMineral...";
            this.buttonReadMineralProperty.UseVisualStyleBackColor = true;
            this.buttonReadMineralProperty.Click += new System.EventHandler(this.buttonReadMineralProperty_Click);
            // 
            // textBoxMineralProperty
            // 
            this.textBoxMineralProperty.Location = new System.Drawing.Point(12, 12);
            this.textBoxMineralProperty.Name = "textBoxMineralProperty";
            this.textBoxMineralProperty.ReadOnly = true;
            this.textBoxMineralProperty.Size = new System.Drawing.Size(303, 22);
            this.textBoxMineralProperty.TabIndex = 1;
            this.textBoxMineralProperty.Text = "MineralName";
            // 
            // openFileDialogReadMineralProperty
            // 
            this.openFileDialogReadMineralProperty.Filter = "MineralParam Files|*.mine";
            // 
            // dataGridViewPTProfile
            // 
            this.dataGridViewPTProfile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPTProfile.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnPressure,
            this.ColumnTemperature});
            this.dataGridViewPTProfile.Location = new System.Drawing.Point(12, 87);
            this.dataGridViewPTProfile.Name = "dataGridViewPTProfile";
            this.dataGridViewPTProfile.RowHeadersWidth = 51;
            this.dataGridViewPTProfile.RowTemplate.Height = 24;
            this.dataGridViewPTProfile.Size = new System.Drawing.Size(304, 477);
            this.dataGridViewPTProfile.TabIndex = 2;
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Location = new System.Drawing.Point(322, 139);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(123, 36);
            this.buttonCalculate.TabIndex = 4;
            this.buttonCalculate.Text = "Calculate...";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click);
            // 
            // textBoxProfileName
            // 
            this.textBoxProfileName.Location = new System.Drawing.Point(12, 47);
            this.textBoxProfileName.Name = "textBoxProfileName";
            this.textBoxProfileName.Size = new System.Drawing.Size(303, 22);
            this.textBoxProfileName.TabIndex = 6;
            this.textBoxProfileName.Text = "ProfileName";
            // 
            // buttonReadPTProfile
            // 
            this.buttonReadPTProfile.Location = new System.Drawing.Point(321, 43);
            this.buttonReadPTProfile.Name = "buttonReadPTProfile";
            this.buttonReadPTProfile.Size = new System.Drawing.Size(124, 29);
            this.buttonReadPTProfile.TabIndex = 5;
            this.buttonReadPTProfile.Text = "ReadProfile...";
            this.buttonReadPTProfile.UseVisualStyleBackColor = true;
            this.buttonReadPTProfile.Click += new System.EventHandler(this.buttonReadPTProfile_Click);
            // 
            // buttonExportPTProfile
            // 
            this.buttonExportPTProfile.Location = new System.Drawing.Point(322, 87);
            this.buttonExportPTProfile.Name = "buttonExportPTProfile";
            this.buttonExportPTProfile.Size = new System.Drawing.Size(123, 36);
            this.buttonExportPTProfile.TabIndex = 7;
            this.buttonExportPTProfile.Text = "ExportPT...";
            this.buttonExportPTProfile.UseVisualStyleBackColor = true;
            this.buttonExportPTProfile.Click += new System.EventHandler(this.buttonExportPTProfile_Click);
            // 
            // openFileDialogReadPTProfile
            // 
            this.openFileDialogReadPTProfile.DefaultExt = "ptp";
            this.openFileDialogReadPTProfile.Filter = "PTProfile Files|*.ptp";
            // 
            // saveFileDialogExportPTProfile
            // 
            this.saveFileDialogExportPTProfile.DefaultExt = "ptp";
            this.saveFileDialogExportPTProfile.Filter = "PTProfile Files|*.ptp";
            // 
            // ColumnPressure
            // 
            this.ColumnPressure.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnPressure.HeaderText = "P[GPa]";
            this.ColumnPressure.MinimumWidth = 6;
            this.ColumnPressure.Name = "ColumnPressure";
            // 
            // ColumnTemperature
            // 
            this.ColumnTemperature.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnTemperature.HeaderText = "T[K]";
            this.ColumnTemperature.MinimumWidth = 6;
            this.ColumnTemperature.Name = "ColumnTemperature";
            // 
            // FormPTProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 576);
            this.Controls.Add(this.buttonExportPTProfile);
            this.Controls.Add(this.textBoxProfileName);
            this.Controls.Add(this.buttonReadPTProfile);
            this.Controls.Add(this.buttonCalculate);
            this.Controls.Add(this.dataGridViewPTProfile);
            this.Controls.Add(this.textBoxMineralProperty);
            this.Controls.Add(this.buttonReadMineralProperty);
            this.Name = "FormPTProfile";
            this.Text = "FormPTProfile";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPTProfile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonReadMineralProperty;
        private System.Windows.Forms.TextBox textBoxMineralProperty;
        private System.Windows.Forms.OpenFileDialog openFileDialogReadMineralProperty;
        private System.Windows.Forms.DataGridView dataGridViewPTProfile;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.TextBox textBoxProfileName;
        private System.Windows.Forms.Button buttonReadPTProfile;
        private System.Windows.Forms.Button buttonExportPTProfile;
        private System.Windows.Forms.OpenFileDialog openFileDialogReadPTProfile;
        private System.Windows.Forms.SaveFileDialog saveFileDialogExportPTProfile;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPressure;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTemperature;
    }
}