namespace YuanTu.AutoUpdater
{
    partial class DownloadProgress
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			this.labelCurrentItem = new System.Windows.Forms.Label();
			this.buttonOk = new System.Windows.Forms.Button();
			this.panel = new System.Windows.Forms.Panel();
			this.progressBarTotal = new System.Windows.Forms.ProgressBar();
			this.progressBarCurrent = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.labelCurrent = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelCurrentItem
			// 
			this.labelCurrentItem.AutoSize = true;
			this.labelCurrentItem.Location = new System.Drawing.Point(84, 85);
			this.labelCurrentItem.Name = "labelCurrentItem";
			this.labelCurrentItem.Size = new System.Drawing.Size(0, 12);
			this.labelCurrentItem.TabIndex = 0;
			// 
			// buttonOk
			// 
			this.buttonOk.Location = new System.Drawing.Point(403, 145);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(83, 23);
			this.buttonOk.TabIndex = 2;
			this.buttonOk.Text = "取消";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.OnCancel);
			// 
			// panel
			// 
			this.panel.Controls.Add(this.progressBarTotal);
			this.panel.Controls.Add(this.progressBarCurrent);
			this.panel.Controls.Add(this.label1);
			this.panel.Controls.Add(this.labelCurrent);
			this.panel.Controls.Add(this.label5);
			this.panel.Controls.Add(this.label7);
			this.panel.Controls.Add(this.label6);
			this.panel.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(502, 133);
			this.panel.TabIndex = 4;
			// 
			// progressBarTotal
			// 
			this.progressBarTotal.Location = new System.Drawing.Point(33, 87);
			this.progressBarTotal.Name = "progressBarTotal";
			this.progressBarTotal.Size = new System.Drawing.Size(438, 12);
			this.progressBarTotal.Step = 1;
			this.progressBarTotal.TabIndex = 4;
			// 
			// progressBarCurrent
			// 
			this.progressBarCurrent.Location = new System.Drawing.Point(32, 47);
			this.progressBarCurrent.Name = "progressBarCurrent";
			this.progressBarCurrent.Size = new System.Drawing.Size(438, 12);
			this.progressBarCurrent.Step = 1;
			this.progressBarCurrent.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(31, 71);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "总进度:";
			// 
			// labelCurrent
			// 
			this.labelCurrent.AutoSize = true;
			this.labelCurrent.Location = new System.Drawing.Point(31, 32);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = new System.Drawing.Size(59, 12);
			this.labelCurrent.TabIndex = 3;
			this.labelCurrent.Text = "下载进度:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(245, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(137, 12);
			this.label5.TabIndex = 0;
			this.label5.Text = "From:    Remote Server";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(32, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(47, 12);
			this.label7.TabIndex = 0;
			this.label7.Text = "Name:  ";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(33, 111);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(149, 12);
			this.label6.TabIndex = 0;
			this.label6.Text = "Preparing Application...";
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(498, 2);
			this.splitter1.TabIndex = 5;
			this.splitter1.TabStop = false;
			// 
			// splitter2
			// 
			this.splitter2.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new System.Drawing.Point(0, 178);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(498, 2);
			this.splitter2.TabIndex = 6;
			this.splitter2.TabStop = false;
			// 
			// DownloadProgress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(498, 180);
			this.ControlBox = false;
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panel);
			this.Controls.Add(this.labelCurrentItem);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DownloadProgress";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "程序更新中";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCurrentItem;
		private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.ProgressBar progressBarTotal;
        private System.Windows.Forms.ProgressBar progressBarCurrent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCurrent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
    }
}