﻿using System;
using System.Windows.Forms;

using VGMToolbox.format;
using VGMToolbox.plugin;
using VGMToolbox.tools.stream;

namespace VGMToolbox.forms.stream
{
    public partial class MpegDemuxForm : AVgmtForm
    {
        public MpegDemuxForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "Video Demultiplexer";

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();            
            
            InitializeComponent();

            this.tbOutput.Text = "Demultiplex streams from movies." + Environment.NewLine;
            this.tbOutput.Text += "- Currently supported formats: DVD Video, MPEG1, MPEG2, PAM, PMF, PSS, SFD, USM" + Environment.NewLine;
            this.tbOutput.Text += "- If the MPEG does not work for your file, be sure to try DVD Video, since it can handle specialized audio types." + Environment.NewLine;
            this.tbOutput.Text += "- MKVMerge can be used to add raw .264 data to a container file for playback." + Environment.NewLine;


            this.initializeFormatList();
        }

        private void initializeFormatList()
        {
            this.comboFormat.Items.Clear();
            this.comboFormat.Items.Add("DVD Video");
            this.comboFormat.Items.Add("MPEG");
            this.comboFormat.Items.Add("PAM");
            this.comboFormat.Items.Add("PMF");
            this.comboFormat.Items.Add("PSS");
            this.comboFormat.Items.Add("SFD");
            this.comboFormat.Items.Add("USM");
            this.comboFormat.Items.Add("XMV");

            this.comboFormat.SelectedItem = "DVD Video";
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new MpegDemuxWorker();
        }
        protected override string getCancelMessage()
        {
            return "Demultiplexing Streams...Cancelled";
        }
        protected override string getCompleteMessage()
        {
            return "Demultiplexing Streams...Complete";
        }
        protected override string getBeginMessage()
        {
            return "Demultiplexing Streams...Begin";
        }

        private void MpegDemuxForm_DragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        private void MpegDemuxForm_DragDrop(object sender, DragEventArgs e)
        {
            MpegDemuxWorker.MpegDemuxStruct taskStruct = new MpegDemuxWorker.MpegDemuxStruct();

            // paths
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            taskStruct.SourcePaths = s;

            // format
            taskStruct.SourceFormat = this.comboFormat.SelectedItem.ToString();
            
            // options
            taskStruct.AddHeader = this.cbAddHeader.Checked;
            taskStruct.ExtractAudio = (this.rbExtractAudioAndVideo.Checked || 
                                       this.rbExtractAudioOnly.Checked);
            taskStruct.ExtractVideo = (this.rbExtractAudioAndVideo.Checked ||
                                       this.rbExtractVideoOnly.Checked);

            base.backgroundWorker_Execute(taskStruct);
        }

        private void comboFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboFormat.SelectedItem.ToString())
            { 
                case "PAM":
                case "PMF":
                    this.cbAddHeader.Enabled = true;
                    break;
                default:
                    this.cbAddHeader.Checked = false;
                    this.cbAddHeader.Enabled = false;
                    break;
            }
        }
    }
}
