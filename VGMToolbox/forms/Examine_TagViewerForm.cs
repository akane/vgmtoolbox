﻿using System;
using System.Configuration;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools;
using VGMToolbox.util;

namespace VGMToolbox.forms
{
    public partial class Examine_TagViewerForm : TreeViewVgmtForm
    {        
        public Examine_TagViewerForm(TreeNode pTreeNode) : base(pTreeNode) 
        {
            this.lblTitle.Text =
                ConfigurationSettings.AppSettings["Form_ExamineTags_Title"];
            this.btnDoTask.Text =
                ConfigurationSettings.AppSettings["Form_ExamineTags_DoTaskButton"];

            this.tbOutput.Text =
                ConfigurationSettings.AppSettings["Form_ExamineTags_IntroText1"] + System.Environment.NewLine;
            this.tbOutput.Text +=
                ConfigurationSettings.AppSettings["Form_ExamineTags_IntroText2"] + System.Environment.NewLine;

            InitializeComponent();

            this.cbCheckForLibs.Text =
                ConfigurationSettings.AppSettings["Form_ExamineTags_CheckBoxCheckForLibs"];
        }

        private void tbXsfSource_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            int totalFileCount = FileUtil.GetFileCount(s);

            TreeBuilderWorker.TreeBuilderStruct tbStruct = new TreeBuilderWorker.TreeBuilderStruct();
            tbStruct.pPaths = s;
            tbStruct.totalFiles = totalFileCount;
            tbStruct.checkForLibs = cbCheckForLibs.Checked;

            base.backgroundWorker_Execute(tbStruct);
        }
        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }
        private void btnDoTask_Click(object sender, EventArgs e)
        {
            this.treeViewTools.ExpandAll();
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new TreeBuilderWorker();
        }
        protected override string getCancelMessage()
        {
            return ConfigurationSettings.AppSettings["Form_ExamineTags_MessageCancel"];
        }
        protected override string getCompleteMessage()
        {
            return ConfigurationSettings.AppSettings["Form_ExamineTags_MessageComplete"];
        }
        protected override string getBeginMessage()
        {
            return ConfigurationSettings.AppSettings["Form_ExamineTags_MessageBegin"];
        }
    }
}
