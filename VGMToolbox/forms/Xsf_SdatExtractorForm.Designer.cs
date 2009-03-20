﻿namespace VGMToolbox.forms
{
    partial class Xsf_SdatExtractorForm
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
            this.groupSource = new System.Windows.Forms.GroupBox();
            this.lblDragNDrop = new System.Windows.Forms.Label();
            this.tbNDS_SdatExtractor_Source = new System.Windows.Forms.TextBox();
            this.pnlLabels.SuspendLayout();
            this.pnlTitle.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.groupSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLabels
            // 
            this.pnlLabels.Location = new System.Drawing.Point(0, 422);
            this.pnlLabels.Size = new System.Drawing.Size(874, 19);
            // 
            // pnlTitle
            // 
            this.pnlTitle.Size = new System.Drawing.Size(874, 20);
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(0, 345);
            this.tbOutput.Size = new System.Drawing.Size(874, 77);
            // 
            // pnlButtons
            // 
            this.pnlButtons.Location = new System.Drawing.Point(0, 325);
            this.pnlButtons.Size = new System.Drawing.Size(874, 20);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(814, 0);
            // 
            // btnDoTask
            // 
            this.btnDoTask.Location = new System.Drawing.Point(754, 0);
            // 
            // groupSource
            // 
            this.groupSource.Controls.Add(this.lblDragNDrop);
            this.groupSource.Controls.Add(this.tbNDS_SdatExtractor_Source);
            this.groupSource.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupSource.Location = new System.Drawing.Point(0, 23);
            this.groupSource.Name = "groupSource";
            this.groupSource.Size = new System.Drawing.Size(874, 59);
            this.groupSource.TabIndex = 6;
            this.groupSource.TabStop = false;
            this.groupSource.Text = "Source SDAT";
            // 
            // lblDragNDrop
            // 
            this.lblDragNDrop.AutoSize = true;
            this.lblDragNDrop.Location = new System.Drawing.Point(6, 42);
            this.lblDragNDrop.Name = "lblDragNDrop";
            this.lblDragNDrop.Size = new System.Drawing.Size(147, 13);
            this.lblDragNDrop.TabIndex = 1;
            this.lblDragNDrop.Text = "Drag SDAT(s) to extract here.";
            // 
            // tbNDS_SdatExtractor_Source
            // 
            this.tbNDS_SdatExtractor_Source.AllowDrop = true;
            this.tbNDS_SdatExtractor_Source.Location = new System.Drawing.Point(6, 19);
            this.tbNDS_SdatExtractor_Source.Name = "tbNDS_SdatExtractor_Source";
            this.tbNDS_SdatExtractor_Source.Size = new System.Drawing.Size(282, 20);
            this.tbNDS_SdatExtractor_Source.TabIndex = 0;
            this.tbNDS_SdatExtractor_Source.DragDrop += new System.Windows.Forms.DragEventHandler(this.tbNDS_SdatExtractor_Source_DragDrop);
            this.tbNDS_SdatExtractor_Source.DragEnter += new System.Windows.Forms.DragEventHandler(this.doDragEnter);
            // 
            // Xsf_SdatExtractorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(874, 463);
            this.Controls.Add(this.groupSource);
            this.Name = "Xsf_SdatExtractorForm";
            this.Text = "Xsf_SdatExtractorForm";
            this.Controls.SetChildIndex(this.pnlLabels, 0);
            this.Controls.SetChildIndex(this.pnlTitle, 0);
            this.Controls.SetChildIndex(this.groupSource, 0);
            this.Controls.SetChildIndex(this.tbOutput, 0);
            this.Controls.SetChildIndex(this.pnlButtons, 0);
            this.pnlLabels.ResumeLayout(false);
            this.pnlLabels.PerformLayout();
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.groupSource.ResumeLayout(false);
            this.groupSource.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupSource;
        private System.Windows.Forms.Label lblDragNDrop;
        private System.Windows.Forms.TextBox tbNDS_SdatExtractor_Source;
    }
}