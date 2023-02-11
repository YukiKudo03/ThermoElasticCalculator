
namespace thermo_dynamics
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonShoMineralInfo = new System.Windows.Forms.Button();
            this.buttonPTProfile = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonShoMineralInfo
            // 
            this.buttonShoMineralInfo.Location = new System.Drawing.Point(13, 22);
            this.buttonShoMineralInfo.Margin = new System.Windows.Forms.Padding(4);
            this.buttonShoMineralInfo.Name = "buttonShoMineralInfo";
            this.buttonShoMineralInfo.Size = new System.Drawing.Size(164, 85);
            this.buttonShoMineralInfo.TabIndex = 0;
            this.buttonShoMineralInfo.Text = "MineralLibrary...";
            this.buttonShoMineralInfo.UseVisualStyleBackColor = true;
            this.buttonShoMineralInfo.Click += new System.EventHandler(this.buttonShoMineralInfo_Click);
            // 
            // buttonPTProfile
            // 
            this.buttonPTProfile.Location = new System.Drawing.Point(185, 22);
            this.buttonPTProfile.Margin = new System.Windows.Forms.Padding(4);
            this.buttonPTProfile.Name = "buttonPTProfile";
            this.buttonPTProfile.Size = new System.Drawing.Size(164, 85);
            this.buttonPTProfile.TabIndex = 1;
            this.buttonPTProfile.Text = "Calculate w/ PTProfile...";
            this.buttonPTProfile.UseVisualStyleBackColor = true;
            this.buttonPTProfile.Click += new System.EventHandler(this.buttonPTProfile_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(357, 22);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(164, 85);
            this.button1.TabIndex = 2;
            this.button1.Text = "Calculate Mixture...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonPTProfile);
            this.Controls.Add(this.buttonShoMineralInfo);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormMain";
            this.Text = "ThermoElasticCalculator";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonShoMineralInfo;
        private System.Windows.Forms.Button buttonPTProfile;
        private System.Windows.Forms.Button button1;
    }
}

