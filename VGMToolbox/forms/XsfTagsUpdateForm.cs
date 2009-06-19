﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.format;

namespace VGMToolbox.forms
{
    public partial class XsfTagsUpdateForm : Form
    {
        VGMToolbox.util.NodeTagStruct nodeTagInfo;
        IXsfTagFormat vgmData;

        public XsfTagsUpdateForm(VGMToolbox.util.NodeTagStruct pNts)
        {
            this.nodeTagInfo = pNts;
            
            InitializeComponent();

            this.loadCurrentTagInformation();
        }

        private void loadCurrentTagInformation()
        {
            using (FileStream fs =
                File.Open(this.nodeTagInfo.FilePath, FileMode.Open, FileAccess.Read))
            {
                this.vgmData =
                    (IXsfTagFormat)Activator.CreateInstance(Type.GetType(this.nodeTagInfo.ObjectType));
                this.vgmData.Initialize(fs, this.nodeTagInfo.FilePath);

                this.tbGame.Text = this.vgmData.GetGameTag();
                this.tbArtist.Text = this.vgmData.GetArtistTag();
                this.tbCopyright.Text = this.vgmData.GetCopyrightTag();
                this.tbGenre.Text = this.vgmData.GetGenreTag();
                this.tbYear.Text = this.vgmData.GetYearTag();
                this.tbXsfBy.Text = this.vgmData.GetXsfByTag();
                
                this.tbTitle.Text = this.vgmData.GetTitleTag();
                this.tbLength.Text = this.vgmData.GetLengthTag();
                this.tbFade.Text = this.vgmData.GetFadeTag();
                this.tbVolume.Text = this.vgmData.GetVolumeTag();

                this.tbComments.Text = this.vgmData.GetCommentTag();
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                this.vgmData.SetGameTag(this.tbGame.Text);
                this.vgmData.SetArtistTag(this.tbArtist.Text);
                this.vgmData.SetCopyrightTag(this.tbCopyright.Text);
                this.vgmData.SetGenreTag(this.tbGenre.Text);
                this.vgmData.SetYearTag(this.tbYear.Text);
                this.vgmData.SetXsfByTag(this.tbXsfBy.Text);

                this.vgmData.SetTitleTag(this.tbTitle.Text);
                this.vgmData.SetLengthTag(this.tbLength.Text);
                this.vgmData.SetFadeTag(this.tbFade.Text);
                this.vgmData.SetVolumeTag(this.tbVolume.Text);

                this.vgmData.SetCommentTag(this.tbComments.Text);

                this.vgmData.UpdateTags();


                // MessageBox.Show(String.Format("Tags for \"{0}\" have been updated.  Changes will not be displayed in the tree until you add the files again.", Path.GetFileName(this.vgmData.FilePath)));
                MessageBox.Show(String.Format("Update complete, updates will not be reflected in the tree until the files are added again.",
                    Path.GetFileName(this.vgmData.FilePath)));

                this.Close();
                this.Dispose();
            }
            catch (Exception _ex)
            {
                MessageBox.Show(_ex.Message, "Error Updating Tags");
            }
            
        }
    }
}
