
namespace thermo_dynamics
{
    partial class FormRockCalculator
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.dataGridViewMinerals = new System.Windows.Forms.DataGridView();
            this.ColumnMineralName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnVolumeRatio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonAddMineral = new System.Windows.Forms.Button();
            this.buttonRemoveMineral = new System.Windows.Forms.Button();
            this.buttonLoadRock = new System.Windows.Forms.Button();
            this.buttonSaveRock = new System.Windows.Forms.Button();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.textBoxRockName = new System.Windows.Forms.TextBox();
            this.labelRockName = new System.Windows.Forms.Label();
            this.numericUpDownPressure = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTemperature = new System.Windows.Forms.NumericUpDown();
            this.labelPressure = new System.Windows.Forms.Label();
            this.labelTemperature = new System.Windows.Forms.Label();
            this.comboBoxMixMethod = new System.Windows.Forms.ComboBox();
            this.labelMixMethod = new System.Windows.Forms.Label();
            this.openFileDialogMineral = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogRock = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogRock = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMinerals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPressure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).BeginInit();
            this.SuspendLayout();
            //
            // labelRockName
            //
            this.labelRockName.AutoSize = true;
            this.labelRockName.Location = new System.Drawing.Point(12, 15);
            this.labelRockName.Name = "labelRockName";
            this.labelRockName.Size = new System.Drawing.Size(80, 15);
            this.labelRockName.TabIndex = 0;
            this.labelRockName.Text = "Rock Name:";
            //
            // textBoxRockName
            //
            this.textBoxRockName.Location = new System.Drawing.Point(98, 12);
            this.textBoxRockName.Name = "textBoxRockName";
            this.textBoxRockName.Size = new System.Drawing.Size(250, 22);
            this.textBoxRockName.TabIndex = 1;
            this.textBoxRockName.Text = "NewRock";
            //
            // buttonLoadRock
            //
            this.buttonLoadRock.Location = new System.Drawing.Point(360, 10);
            this.buttonLoadRock.Name = "buttonLoadRock";
            this.buttonLoadRock.Size = new System.Drawing.Size(100, 27);
            this.buttonLoadRock.TabIndex = 2;
            this.buttonLoadRock.Text = "Load Rock...";
            this.buttonLoadRock.UseVisualStyleBackColor = true;
            this.buttonLoadRock.Click += new System.EventHandler(this.buttonLoadRock_Click);
            //
            // buttonSaveRock
            //
            this.buttonSaveRock.Location = new System.Drawing.Point(470, 10);
            this.buttonSaveRock.Name = "buttonSaveRock";
            this.buttonSaveRock.Size = new System.Drawing.Size(100, 27);
            this.buttonSaveRock.TabIndex = 3;
            this.buttonSaveRock.Text = "Save Rock...";
            this.buttonSaveRock.UseVisualStyleBackColor = true;
            this.buttonSaveRock.Click += new System.EventHandler(this.buttonSaveRock_Click);
            //
            // dataGridViewMinerals
            //
            this.dataGridViewMinerals.AllowUserToAddRows = false;
            this.dataGridViewMinerals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMinerals.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.ColumnMineralName,
                this.ColumnVolumeRatio});
            this.dataGridViewMinerals.Location = new System.Drawing.Point(12, 45);
            this.dataGridViewMinerals.Name = "dataGridViewMinerals";
            this.dataGridViewMinerals.RowHeadersWidth = 30;
            this.dataGridViewMinerals.RowTemplate.Height = 24;
            this.dataGridViewMinerals.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewMinerals.Size = new System.Drawing.Size(440, 350);
            this.dataGridViewMinerals.TabIndex = 4;
            //
            // ColumnMineralName
            //
            this.ColumnMineralName.HeaderText = "Mineral";
            this.ColumnMineralName.MinimumWidth = 6;
            this.ColumnMineralName.Name = "ColumnMineralName";
            this.ColumnMineralName.ReadOnly = true;
            this.ColumnMineralName.Width = 250;
            //
            // ColumnVolumeRatio
            //
            this.ColumnVolumeRatio.HeaderText = "Vol. Ratio";
            this.ColumnVolumeRatio.MinimumWidth = 6;
            this.ColumnVolumeRatio.Name = "ColumnVolumeRatio";
            this.ColumnVolumeRatio.Width = 120;
            //
            // buttonAddMineral
            //
            this.buttonAddMineral.Location = new System.Drawing.Point(460, 45);
            this.buttonAddMineral.Name = "buttonAddMineral";
            this.buttonAddMineral.Size = new System.Drawing.Size(110, 30);
            this.buttonAddMineral.TabIndex = 5;
            this.buttonAddMineral.Text = "Add Mineral...";
            this.buttonAddMineral.UseVisualStyleBackColor = true;
            this.buttonAddMineral.Click += new System.EventHandler(this.buttonAddMineral_Click);
            //
            // buttonRemoveMineral
            //
            this.buttonRemoveMineral.Location = new System.Drawing.Point(460, 81);
            this.buttonRemoveMineral.Name = "buttonRemoveMineral";
            this.buttonRemoveMineral.Size = new System.Drawing.Size(110, 30);
            this.buttonRemoveMineral.TabIndex = 6;
            this.buttonRemoveMineral.Text = "Remove";
            this.buttonRemoveMineral.UseVisualStyleBackColor = true;
            this.buttonRemoveMineral.Click += new System.EventHandler(this.buttonRemoveMineral_Click);
            //
            // labelPressure
            //
            this.labelPressure.AutoSize = true;
            this.labelPressure.Location = new System.Drawing.Point(12, 410);
            this.labelPressure.Name = "labelPressure";
            this.labelPressure.Size = new System.Drawing.Size(52, 15);
            this.labelPressure.TabIndex = 7;
            this.labelPressure.Text = "P[GPa]";
            //
            // numericUpDownPressure
            //
            this.numericUpDownPressure.DecimalPlaces = 2;
            this.numericUpDownPressure.Location = new System.Drawing.Point(70, 408);
            this.numericUpDownPressure.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownPressure.Name = "numericUpDownPressure";
            this.numericUpDownPressure.Size = new System.Drawing.Size(100, 22);
            this.numericUpDownPressure.TabIndex = 8;
            this.numericUpDownPressure.Value = new decimal(new int[] { 10, 0, 0, 0 });
            //
            // labelTemperature
            //
            this.labelTemperature.AutoSize = true;
            this.labelTemperature.Location = new System.Drawing.Point(185, 410);
            this.labelTemperature.Name = "labelTemperature";
            this.labelTemperature.Size = new System.Drawing.Size(30, 15);
            this.labelTemperature.TabIndex = 9;
            this.labelTemperature.Text = "T[K]";
            //
            // numericUpDownTemperature
            //
            this.numericUpDownTemperature.DecimalPlaces = 2;
            this.numericUpDownTemperature.Location = new System.Drawing.Point(220, 408);
            this.numericUpDownTemperature.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numericUpDownTemperature.Name = "numericUpDownTemperature";
            this.numericUpDownTemperature.Size = new System.Drawing.Size(100, 22);
            this.numericUpDownTemperature.TabIndex = 10;
            this.numericUpDownTemperature.Value = new decimal(new int[] { 1500, 0, 0, 0 });
            //
            // labelMixMethod
            //
            this.labelMixMethod.AutoSize = true;
            this.labelMixMethod.Location = new System.Drawing.Point(335, 410);
            this.labelMixMethod.Name = "labelMixMethod";
            this.labelMixMethod.Size = new System.Drawing.Size(52, 15);
            this.labelMixMethod.TabIndex = 11;
            this.labelMixMethod.Text = "Method:";
            //
            // comboBoxMixMethod
            //
            this.comboBoxMixMethod.FormattingEnabled = true;
            this.comboBoxMixMethod.Location = new System.Drawing.Point(393, 407);
            this.comboBoxMixMethod.Name = "comboBoxMixMethod";
            this.comboBoxMixMethod.Size = new System.Drawing.Size(100, 23);
            this.comboBoxMixMethod.TabIndex = 12;
            //
            // buttonCalculate
            //
            this.buttonCalculate.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Bold);
            this.buttonCalculate.Location = new System.Drawing.Point(460, 440);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(110, 35);
            this.buttonCalculate.TabIndex = 13;
            this.buttonCalculate.Text = "Calculate";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click);
            //
            // FormRockCalculator
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 490);
            this.Controls.Add(this.buttonCalculate);
            this.Controls.Add(this.comboBoxMixMethod);
            this.Controls.Add(this.labelMixMethod);
            this.Controls.Add(this.numericUpDownTemperature);
            this.Controls.Add(this.labelTemperature);
            this.Controls.Add(this.numericUpDownPressure);
            this.Controls.Add(this.labelPressure);
            this.Controls.Add(this.buttonRemoveMineral);
            this.Controls.Add(this.buttonAddMineral);
            this.Controls.Add(this.dataGridViewMinerals);
            this.Controls.Add(this.buttonSaveRock);
            this.Controls.Add(this.buttonLoadRock);
            this.Controls.Add(this.textBoxRockName);
            this.Controls.Add(this.labelRockName);
            this.Name = "FormRockCalculator";
            this.Text = "Rock Calculator";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMinerals)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPressure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewMinerals;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMineralName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnVolumeRatio;
        private System.Windows.Forms.Button buttonAddMineral;
        private System.Windows.Forms.Button buttonRemoveMineral;
        private System.Windows.Forms.Button buttonLoadRock;
        private System.Windows.Forms.Button buttonSaveRock;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.TextBox textBoxRockName;
        private System.Windows.Forms.Label labelRockName;
        private System.Windows.Forms.NumericUpDown numericUpDownPressure;
        private System.Windows.Forms.NumericUpDown numericUpDownTemperature;
        private System.Windows.Forms.Label labelPressure;
        private System.Windows.Forms.Label labelTemperature;
        private System.Windows.Forms.ComboBox comboBoxMixMethod;
        private System.Windows.Forms.Label labelMixMethod;
        private System.Windows.Forms.OpenFileDialog openFileDialogMineral;
        private System.Windows.Forms.OpenFileDialog openFileDialogRock;
        private System.Windows.Forms.SaveFileDialog saveFileDialogRock;
    }
}
