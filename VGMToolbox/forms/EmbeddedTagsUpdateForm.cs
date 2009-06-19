﻿using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

using VGMToolbox.format;
using VGMToolbox.util;

namespace VGMToolbox.forms
{
    public partial class EmbeddedTagsUpdateForm : Form
    {
        VGMToolbox.util.NodeTagStruct nodeTagInfo;
        IEmbeddedTagsFormat vgmData;

        public EmbeddedTagsUpdateForm(VGMToolbox.util.NodeTagStruct pNts)
        {
            nodeTagInfo = pNts;
            
            InitializeComponent();

            this.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_WindowTitle"];

            this.grpTags.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_GroupTags"];
            this.lblName.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_LblName"];
            this.lblArtist.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_LblArtist"];
            this.lblCopyright.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_LblCopyright"];
            this.btnUpdate.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_BtnUpdate"];
            this.btnCancel.Text =
                ConfigurationSettings.AppSettings["Form_EmbeddedTags_BtnCancel"];

            loadCurrentTagInformation();
        }

        private void loadCurrentTagInformation()
        {
            using (FileStream fs = 
                File.Open(this.nodeTagInfo.FilePath, FileMode.Open, FileAccess.Read))
            {
                this.vgmData =
                    (IEmbeddedTagsFormat)Activator.CreateInstance(Type.GetType(this.nodeTagInfo.ObjectType));
                this.vgmData.Initialize(fs, this.nodeTagInfo.FilePath);

                this.tbName.Text = this.vgmData.GetSongNameAsText();
                this.tbArtist.Text = this.vgmData.GetArtistAsText();
                this.tbCopyright.Text = this.vgmData.GetCopyrightAsText();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.vgmData.UpdateSongName(this.tbName.Text);
            this.vgmData.UpdateArtist(this.tbArtist.Text);
            this.vgmData.UpdateCopyright(this.tbCopyright.Text);

            // MessageBox.Show(String.Format("Tags for \"{0}\" have been updated.  Changes will not be displayed in the tree until you add the files again.", Path.GetFileName(this.vgmData.FilePath)));
            MessageBox.Show(String.Format(ConfigurationSettings.AppSettings["Form_EmbeddedTags_MessageUpdateComplete"], 
                Path.GetFileName(this.vgmData.FilePath)));
                        
            this.Close();
            this.Dispose();
        }
    }
}
