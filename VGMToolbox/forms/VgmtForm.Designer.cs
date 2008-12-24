﻿namespace VGMToolbox.forms
{
    partial class VgmtForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.pnlLabels = new System.Windows.Forms.Panel();
            this.lblProgressLabel = new System.Windows.Forms.Label();
            this.lblTimeElapsed = new System.Windows.Forms.Label();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.pnlLabels.SuspendLayout();
            this.pnlTitle.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 371);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(533, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.AutoSize = false;
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(320, 16);
            // 
            // pnlLabels
            // 
            this.pnlLabels.Controls.Add(this.lblProgressLabel);
            this.pnlLabels.Controls.Add(this.lblTimeElapsed);
            this.pnlLabels.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLabels.Location = new System.Drawing.Point(0, 352);
            this.pnlLabels.Name = "pnlLabels";
            this.pnlLabels.Size = new System.Drawing.Size(533, 19);
            this.pnlLabels.TabIndex = 1;
            // 
            // lblProgressLabel
            // 
            this.lblProgressLabel.AutoSize = true;
            this.lblProgressLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblProgressLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblProgressLabel.Location = new System.Drawing.Point(77, 0);
            this.lblProgressLabel.MinimumSize = new System.Drawing.Size(456, 0);
            this.lblProgressLabel.Name = "lblProgressLabel";
            this.lblProgressLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.lblProgressLabel.Size = new System.Drawing.Size(456, 18);
            this.lblProgressLabel.TabIndex = 1;
            this.lblProgressLabel.Text = "Misc Information";
            // 
            // lblTimeElapsed
            // 
            this.lblTimeElapsed.AutoSize = true;
            this.lblTimeElapsed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTimeElapsed.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTimeElapsed.Location = new System.Drawing.Point(0, 0);
            this.lblTimeElapsed.Name = "lblTimeElapsed";
            this.lblTimeElapsed.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.lblTimeElapsed.Size = new System.Drawing.Size(73, 18);
            this.lblTimeElapsed.TabIndex = 0;
            this.lblTimeElapsed.Text = "Elapsed Time";
            // 
            // tbOutput
            // 
            this.tbOutput.AcceptsReturn = true;
            this.tbOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbOutput.Location = new System.Drawing.Point(0, 275);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbOutput.Size = new System.Drawing.Size(533, 77);
            this.tbOutput.TabIndex = 2;
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(533, 20);
            this.pnlTitle.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(87, 15);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Section Title";
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(463, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(0, 1, 1, 1);
            this.btnCancel.Size = new System.Drawing.Size(70, 22);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 253);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(533, 22);
            this.panel1.TabIndex = 5;
            // 
            // VgmtForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 393);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlTitle);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.pnlLabels);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "VgmtForm";
            this.Text = "VgmtForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.pnlLabels.ResumeLayout(false);
            this.pnlLabels.PerformLayout();
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.StatusStrip statusStrip1;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        protected System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        protected System.Windows.Forms.Panel pnlLabels;
        protected System.Windows.Forms.Label lblProgressLabel;
        protected System.Windows.Forms.Label lblTimeElapsed;
        protected System.Windows.Forms.TextBox tbOutput;
        protected System.Windows.Forms.Panel pnlTitle;
        protected System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel1;
    }
}