namespace USBMonitor
{
    partial class Setting
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.saveButton = new System.Windows.Forms.Button();
            this.dir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dirDialogButton = new System.Windows.Forms.Button();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.hidemsg = new System.Windows.Forms.CheckBox();
            this.version = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.autorun = new System.Windows.Forms.CheckBox();
            this.autorunhide = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.multirun = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(242, 181);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(142, 38);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "保存设置 (&S)";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // dir
            // 
            this.dir.Location = new System.Drawing.Point(112, 11);
            this.dir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dir.Name = "dir";
            this.dir.Size = new System.Drawing.Size(155, 25);
            this.dir.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "存储目录：";
            // 
            // dirDialogButton
            // 
            this.dirDialogButton.Location = new System.Drawing.Point(291, 11);
            this.dirDialogButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dirDialogButton.Name = "dirDialogButton";
            this.dirDialogButton.Size = new System.Drawing.Size(100, 29);
            this.dirDialogButton.TabIndex = 4;
            this.dirDialogButton.Text = "浏览 ...";
            this.dirDialogButton.UseVisualStyleBackColor = true;
            this.dirDialogButton.Click += new System.EventHandler(this.dirDialogButton_Click);
            // 
            // folderBrowser
            // 
            this.folderBrowser.Description = "请选择一个文件夹用于存储从存储设备自动复制的文件";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkColor = System.Drawing.Color.SteelBlue;
            this.linkLabel1.Location = new System.Drawing.Point(130, 242);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(137, 15);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "作者：信息室Zengw";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.SteelBlue;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // hidemsg
            // 
            this.hidemsg.AutoSize = true;
            this.hidemsg.Location = new System.Drawing.Point(207, 64);
            this.hidemsg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.hidemsg.Name = "hidemsg";
            this.hidemsg.Size = new System.Drawing.Size(119, 19);
            this.hidemsg.TabIndex = 12;
            this.hidemsg.Text = "隐藏程序通知";
            this.hidemsg.UseVisualStyleBackColor = true;
            // 
            // version
            // 
            this.version.AutoSize = true;
            this.version.Location = new System.Drawing.Point(275, 242);
            this.version.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(118, 15);
            this.version.TabIndex = 18;
            this.version.Text = "Version：1.0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(111, 242);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 15);
            this.label3.TabIndex = 20;
            this.label3.Text = "//";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.LinkColor = System.Drawing.Color.SteelBlue;
            this.linkLabel4.Location = new System.Drawing.Point(29, 242);
            this.linkLabel4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(87, 15);
            this.linkLabel4.TabIndex = 21;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "USBMonitor";
            this.linkLabel4.VisitedLinkColor = System.Drawing.Color.SteelBlue;
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // autorun
            // 
            this.autorun.AutoSize = true;
            this.autorun.Location = new System.Drawing.Point(20, 115);
            this.autorun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.autorun.Name = "autorun";
            this.autorun.Size = new System.Drawing.Size(119, 19);
            this.autorun.TabIndex = 24;
            this.autorun.Text = "开机自动启动";
            this.autorun.UseVisualStyleBackColor = true;
            // 
            // autorunhide
            // 
            this.autorunhide.AutoSize = true;
            this.autorunhide.Location = new System.Drawing.Point(207, 115);
            this.autorunhide.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.autorunhide.Name = "autorunhide";
            this.autorunhide.Size = new System.Drawing.Size(134, 19);
            this.autorunhide.TabIndex = 25;
            this.autorunhide.Text = "以隐藏模式启动";
            this.autorunhide.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(197, 138);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(187, 15);
            this.label16.TabIndex = 23;
            this.label16.Text = "（隐藏模式将看不到图标）";
            // 
            // multirun
            // 
            this.multirun.AutoSize = true;
            this.multirun.Location = new System.Drawing.Point(20, 64);
            this.multirun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.multirun.Name = "multirun";
            this.multirun.Size = new System.Drawing.Size(165, 19);
            this.multirun.TabIndex = 27;
            this.multirun.Text = "允许多重运行(多开)";
            this.multirun.UseVisualStyleBackColor = true;
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 285);
            this.Controls.Add(this.multirun);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.autorunhide);
            this.Controls.Add(this.autorun);
            this.Controls.Add(this.linkLabel4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.version);
            this.Controls.Add(this.hidemsg);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.dirDialogButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dir);
            this.Controls.Add(this.saveButton);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(450, 330);
            this.MinimumSize = new System.Drawing.Size(450, 330);
            this.Name = "Setting";
            this.Text = "USBMonitor 设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox dir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button dirDialogButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox hidemsg;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.CheckBox autorun;
        private System.Windows.Forms.CheckBox autorunhide;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox multirun;
    }
}

