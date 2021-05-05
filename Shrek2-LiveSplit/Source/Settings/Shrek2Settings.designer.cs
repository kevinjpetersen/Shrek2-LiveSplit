namespace Shrek2_LiveSplit
{
    partial class Shrek2Settings
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.gbStartSplits = new System.Windows.Forms.GroupBox();
            this.tlpStartSplits = new System.Windows.Forms.TableLayoutPanel();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkAutoReset = new System.Windows.Forms.CheckBox();
            this.tlpMain.SuspendLayout();
            this.gbStartSplits.SuspendLayout();
            this.tlpStartSplits.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.gbStartSplits, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(7, 7);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(462, 71);
            this.tlpMain.TabIndex = 0;
            // 
            // gbStartSplits
            // 
            this.gbStartSplits.AutoSize = true;
            this.gbStartSplits.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gbStartSplits.Controls.Add(this.tlpStartSplits);
            this.gbStartSplits.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbStartSplits.Location = new System.Drawing.Point(3, 3);
            this.gbStartSplits.Name = "gbStartSplits";
            this.gbStartSplits.Size = new System.Drawing.Size(456, 65);
            this.gbStartSplits.TabIndex = 5;
            this.gbStartSplits.TabStop = false;
            this.gbStartSplits.Text = "General";
            // 
            // tlpStartSplits
            // 
            this.tlpStartSplits.AutoSize = true;
            this.tlpStartSplits.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpStartSplits.BackColor = System.Drawing.Color.Transparent;
            this.tlpStartSplits.ColumnCount = 1;
            this.tlpStartSplits.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpStartSplits.Controls.Add(this.chkAutoStart, 0, 0);
            this.tlpStartSplits.Controls.Add(this.chkAutoReset, 0, 1);
            this.tlpStartSplits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpStartSplits.Location = new System.Drawing.Point(3, 16);
            this.tlpStartSplits.Name = "tlpStartSplits";
            this.tlpStartSplits.RowCount = 2;
            this.tlpStartSplits.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStartSplits.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpStartSplits.Size = new System.Drawing.Size(450, 46);
            this.tlpStartSplits.TabIndex = 4;
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Checked = true;
            this.chkAutoStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoStart.Location = new System.Drawing.Point(3, 3);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(98, 17);
            this.chkAutoStart.TabIndex = 4;
            this.chkAutoStart.Text = "Automatic Start";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // chkAutoReset
            // 
            this.chkAutoReset.AutoSize = true;
            this.chkAutoReset.Checked = true;
            this.chkAutoReset.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoReset.Location = new System.Drawing.Point(3, 26);
            this.chkAutoReset.Name = "chkAutoReset";
            this.chkAutoReset.Size = new System.Drawing.Size(104, 17);
            this.chkAutoReset.TabIndex = 5;
            this.chkAutoReset.Text = "Automatic Reset";
            this.chkAutoReset.UseVisualStyleBackColor = true;
            // 
            // Shrek2Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMain);
            this.Name = "Shrek2Settings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(476, 487);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.gbStartSplits.ResumeLayout(false);
            this.gbStartSplits.PerformLayout();
            this.tlpStartSplits.ResumeLayout(false);
            this.tlpStartSplits.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.GroupBox gbStartSplits;
        private System.Windows.Forms.TableLayoutPanel tlpStartSplits;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.CheckBox chkAutoReset;
    }
}
