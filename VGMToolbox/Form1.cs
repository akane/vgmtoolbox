﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

using VGMToolbox.auditing;
using VGMToolbox.tools;
using VGMToolbox.util;

namespace VGMToolbox
{
    public partial class Form1 : Form
    {
        DatafileCreatorWorker datCreator;
        RebuilderWorker rebuilder;
        
        public Form1()
        {
            InitializeComponent();
            
            // Show Splash
            //Thread th = new Thread(new ThreadStart(DoSplash));
            //Thread th2 = new Thread(new ThreadStart(DoWelcomeMessage));
            //th2.Start();
            //th.Start();            
            //Thread.Sleep(1000);
            //th.Abort();
            //Thread.Sleep(1000);            

            this.loadLanguages();
        }

        private void btnMdxBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbMdxSourceFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnMdxFindPdx_Click(object sender, EventArgs e)
        {
            if (tbMdxSourceFolder.Text.Length > 0)
            {
                tbOutput.Clear();
                treeViewTools.Nodes.Clear();
                
                TreeNode parentNode = new TreeNode(tbMdxSourceFolder.Text);
                treeViewTools.Nodes.Add(parentNode);
                MdxUtil mdxTools = new MdxUtil();
                mdxTools.getPdxForDir(tbMdxSourceFolder.Text, tbOutput, parentNode, cbMdxCheckPdxExist.Checked);
            }
        }

        private void tbMdxSourceFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void tbMdxSourceFolder_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);                                   
            MdxUtil mdxTools = new MdxUtil();
            TreeNode parentNode;
            int i;
            
            try
            {
                doCleanup();               

                for (i = 0; i < s.Length; i++)
                {
                    if (File.Exists(s[i]))
                    {
                        parentNode = new TreeNode(FileUtil.trimPath(s[i]));
                        treeViewTools.Nodes.Add(parentNode);
                        mdxTools.getPdxForFile(s[i], tbOutput, parentNode, cbMdxCheckPdxExist.Checked);
                    }
                    else if (Directory.Exists(s[i]))
                    {
                        parentNode = new TreeNode(s[i]);
                        treeViewTools.Nodes.Add(parentNode);
                        mdxTools.getPdxForDir(s[i], tbOutput, parentNode, cbMdxCheckPdxExist.Checked);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }                

        private void DoSplash()
        {
            Splash sp = new Splash();
            sp.ShowDialog();           
        }

        private void DoWelcomeMessage()
        {
            // Welcome Message
            SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer();         
            synth.Speak("Welcome to V G M Toolbox!");

        }

        # region DATAFILE CREATOR

        /*
        private void btnDatCreator_BuildDat_Click(object sender, EventArgs e)
        {
            doCleanup();
            
            if (checkDatafileCreatorInputs())
            {
                toolStripStatusLabel1.Text = "Building Datafile...";
                toolStripProgressBar.Maximum = Directory.GetFiles(tbDatCreator_SourceFolder.Text, "*.*", SearchOption.AllDirectories).Length;

                string outputMessage = "";
                Datafile datafile = new Datafile();
                DatCreator datCreator = new DatCreator();
                datafile.header = datCreator.buildHeader(tbDatCreator_Author.Text, tbDatCreator_Category.Text,
                    tbDatCreator_Comment.Text, tbDatCreator_Date.Text, tbDatCreator_Description.Text,
                    tbDatCreator_Email.Text, tbDatCreator_Homepage.Text, tbDatCreator_Name.Text,
                    tbDatCreator_Url.Text, tbDatCreator_Version.Text);

                // datafile.game = datCreator.buildGames(tbDatCreator_SourceFolder.Text, ref outputMessage, cbDir2Dat_UseLibHash.Checked);
                datafile.game = datCreator.buildGames(tbDatCreator_SourceFolder.Text, ref outputMessage, false, toolStripProgressBar, cbDatCreator_UseLessRam.Checked);

                XmlSerializer serializer = new XmlSerializer(typeof(Datafile));
                TextWriter textWriter = new StreamWriter(tbDatCreator_OutputDat.Text);
                serializer.Serialize(textWriter, datafile);
                textWriter.Close();
                textWriter.Dispose();

                tbOutput.Text += outputMessage;
                toolStripStatusLabel1.Text = "Building Datafile...Complete";

                //Cleanup
                datafile = null;
                datCreator = null;
                serializer = null;
                textWriter = null;
            }
        }
        */

        private void btnDatCreator_BuildDat_Click(object sender, EventArgs e)
        {
            doCleanup();

            if (checkDatafileCreatorInputs())
            {
                toolStripStatusLabel1.Text = "Building Datafile...";
                
                DatafileCreatorWorker.GetGameParamsStruct vGetGameParamsStruct = new DatafileCreatorWorker.GetGameParamsStruct();
                vGetGameParamsStruct.pDir = tbDatCreator_SourceFolder.Text;
                vGetGameParamsStruct.pOutputMessage = "";
                vGetGameParamsStruct.pUseLibHash = false;
                vGetGameParamsStruct.totalFiles = Directory.GetFiles(tbDatCreator_SourceFolder.Text, "*.*", SearchOption.AllDirectories).Length;

                datCreator = new DatafileCreatorWorker();
                datCreator.ProgressChanged += backgroundWorker_ReportProgress;
                datCreator.RunWorkerCompleted += datafileCreatorWorker_WorkComplete;
                datCreator.RunWorkerAsync(vGetGameParamsStruct);               
            }
        }
        
        private void btnDatCreator_BrowseSource_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbDatCreator_SourceFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnDatCreator_BrowseDestination_Click(object sender, EventArgs e)
        {
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "XML File (*.xml)|*.xml|Datafile (*.dat)| *.dat";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDatCreator_OutputDat.Text = saveFileDialog1.FileName;
            }
        }

        private void btnDatCreator_Cancel_Click(object sender, EventArgs e)
        {
            if (datCreator!= null && datCreator.IsBusy)
            {
                tbOutput.Text += "CANCEL PENDING...";
                datCreator.CancelAsync();
            }
        }

        private bool checkDatafileCreatorInputs()
        {
            bool ret = true;

            if (!(checkTextBox(tbDatCreator_Name.Text, "Datafile Name") &&
                checkTextBox(tbDatCreator_Description.Text, "Description") &&
                checkTextBox(tbDatCreator_SourceFolder.Text, "Source Directory") &&
                checkTextBox(tbDatCreator_OutputDat.Text, "Destination Datafile") &&
                checkFolderExists(tbDatCreator_SourceFolder.Text, "Source Directory")))
            {
                ret = false;
            }

            return ret;
        }

        # endregion
        
        #region REBUILDER

        private void btnRebuilder_BrowseSourceDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbRebuilder_SourceDir.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnRebuilder_BrowseDestinationDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbRebuilder_DestinationDir.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnRebuilder_BrowseDatafile_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbRebuilder_Datafile.Text = openFileDialog1.FileName;
            }
        }

        private void btnRebuilder_Rebuild_Click(object sender, EventArgs e)
        {
            doCleanup();
            if (checkRebuilderInputs())
            {
                toolStripStatusLabel1.Text = "Rebuilding...";

                datafile dataFile = new datafile();
                XmlSerializer serializer = new XmlSerializer(typeof(datafile));
                TextReader textReader = new StreamReader(tbRebuilder_Datafile.Text);
                dataFile = (datafile)serializer.Deserialize(textReader);
                textReader.Close();

                RebuilderWorker.RebuildSetsStruct vRebuildSetsStruct = new RebuilderWorker.RebuildSetsStruct();
                vRebuildSetsStruct.pSourceDir = tbRebuilder_SourceDir.Text;
                vRebuildSetsStruct.pDestinationDir = tbRebuilder_DestinationDir.Text;
                vRebuildSetsStruct.pDatFile = dataFile;
                vRebuildSetsStruct.pRemoveSource = cbRebuilder_RemoveSource.Checked;
                vRebuildSetsStruct.pOverwriteExisting = cbRebuilder_Overwrite.Checked;
                vRebuildSetsStruct.pCompressOutput = cbRebuilder_CompressOutput.Checked;
                vRebuildSetsStruct.totalFiles = Directory.GetFiles(tbRebuilder_SourceDir.Text, "*.*", SearchOption.AllDirectories).Length;

                rebuilder = new RebuilderWorker();
                rebuilder.ProgressChanged += backgroundWorker_ReportProgress;
                rebuilder.RunWorkerCompleted += rebuilderWorker_WorkComplete;
                rebuilder.RunWorkerAsync(vRebuildSetsStruct);
            }
        }

        private bool checkRebuilderInputs()
        {
            bool ret = true;

            if (checkTextBox(tbRebuilder_SourceDir.Text, "Source Directory") &&
                checkTextBox(tbRebuilder_DestinationDir.Text, "Destination Directory") &&
                checkTextBox(tbRebuilder_Datafile.Text, "Datafile Path") &&
                checkFolderExists(tbRebuilder_SourceDir.Text, "Source Directory") &&
                checkFolderExists(tbRebuilder_DestinationDir.Text, "Destination Directory") &&
                checkFileExists(tbRebuilder_Datafile.Text, "Datafile Path"))
            {

                if (tbRebuilder_SourceDir.Text.Trim().Equals(tbRebuilder_DestinationDir.Text.Trim()))
                {
                    MessageBox.Show("Source directory cannot be the same as the Destination directory");
                    ret = false;
                }
            }
            else
            {
                ret = false;
            }

            return ret;
        }

        private void cbRebuilder_CompressOutput_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRebuilder_CompressOutput.Checked)
            {
                cbRebuilder_Overwrite.Checked = false;
            }
        }

        private void cbRebuilder_Overwrite_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRebuilder_Overwrite.Checked)
            {
                cbRebuilder_CompressOutput.Checked = false;
            }
        }

        private void btnRebuilder_Cancel_Click(object sender, EventArgs e)
        {
            if (rebuilder != null && rebuilder.IsBusy)
            {
                tbOutput.Text += "CANCEL PENDING...";
                rebuilder.CancelAsync();
            }
        }

        # endregion

        # region EXAMINE - XSF

        private void tbXsfSource_DragDrop(object sender, DragEventArgs e)
        {
            doCleanup();

            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            toolStripProgressBar.Maximum = s.Length;
            
            XsfUtil xsfUtils = new XsfUtil();
            TreeNode[] parentNodes = new TreeNode[0];
            string outputMessage = "";

            xsfUtils.buildTreeNode(s, ref parentNodes, ref outputMessage, toolStripProgressBar);
            foreach (TreeNode t in parentNodes)
            {
                treeViewTools.Nodes.Add(t);
            }
            tbOutput.Text = outputMessage;
        }

        private void tbXsfSource_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void btnXsfGetInfo_Click(object sender, EventArgs e)
        {
            tbOutput.Text = XsfUtil.getType(tbXsfSource.Text);
        }

        private void btnPsfBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbXsfSource.Text = folderBrowserDialog.SelectedPath;
            }
        }

        # endregion

        #region MISC

        private void doCleanup()
        {
            tbOutput.Clear();
            treeViewTools.Nodes.Clear();
            doCancelCleanup();
        }

        private void doCancelCleanup()
        {
            treeViewTools.Nodes.Clear();
            toolStripProgressBar.Value = 0;
            lblProgressLabel.Text = String.Empty;
        }

        private bool checkTextBox(string pText, string pFieldName)
        {
            bool ret = true;
            
            if (pText.Trim().Length == 0)
            {
                MessageBox.Show(String.Format("{0} cannot be empty.", pFieldName));
                ret = false;
            }
            return ret;
        }

        private bool checkFolderExists(string pPath, string pFolderName)
        {
            bool ret = true;

            if (!Directory.Exists(pPath))
            {
                MessageBox.Show(String.Format("{0} cannot be found.", pFolderName));
                ret = false;
            }
            return ret;
        }

        private bool checkFileExists(string pPath, string pFileName)
        {
            bool ret = true;

            if (!File.Exists(pPath))
            {
                MessageBox.Show(String.Format("{0} cannot be found.", pFileName));
                ret = false;
            }
            return ret;
        }

        # endregion

        # region ABOUT BOX

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(DoAboutBox));
            th.Start();
        }

        private void DoAboutBox()
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        # endregion

        #region INI File

        private void loadLanguages()
        {
            string iniPath = "languages.ini";

            if (File.Exists(iniPath))
            {
                IniFile iniFile = new IniFile(".\\languages.ini");

                // Datafile Creator
                grpDatCreator_Header.Text = getGuiIniLabel(iniFile, grpDatCreator_Header.Text, "DATAFILE_BUILDER", "HEADER_GROUP_TAG");
                lblDatCreator_HeaderName.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderName.Text, "DATAFILE_BUILDER", "HEADER_NAME_TAG");
                lblDatCreator_HeaderDescription.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderDescription.Text, "DATAFILE_BUILDER", "HEADER_DESCRIPTION_TAG");
                lblDatCreator_HeaderVersion.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderVersion.Text, "DATAFILE_BUILDER", "HEADER_VERSION_TAG");
                lblDatCreator_HeaderAuthor.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderAuthor.Text, "DATAFILE_BUILDER", "HEADER_AUTHOR_TAG");
                lblDatCreator_HeaderComment.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderComment.Text, "DATAFILE_BUILDER", "HEADER_COMMENT_TAG");
                lblDatCreator_HeaderCategory.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderCategory.Text, "DATAFILE_BUILDER", "HEADER_CATEGORY_TAG");
                lblDatCreator_HeaderDate.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderDate.Text, "DATAFILE_BUILDER", "HEADER_DATE_TAG");
                lblDatCreator_HeaderEmail.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderEmail.Text, "DATAFILE_BUILDER", "HEADER_EMAIL_TAG");
                lblDatCreator_HeaderHomepage.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderHomepage.Text, "DATAFILE_BUILDER", "HEADER_HOMEPAGE_TAG");
                lblDatCreator_HeaderUrl.Text = getGuiIniLabel(iniFile, lblDatCreator_HeaderUrl.Text, "DATAFILE_BUILDER", "HEADER_URL_TAG");
                grpDatCreator_Options.Text = getGuiIniLabel(iniFile, grpDatCreator_Options.Text, "DATAFILE_BUILDER", "OPTIONS_GROUP_TAG");
                lblDatCreator_SourceFolder.Text = getGuiIniLabel(iniFile, lblDatCreator_SourceFolder.Text, "DATAFILE_BUILDER", "SOURCE_FOLDER_LABEL");
                lblDatCreator_DestinationFolder.Text = getGuiIniLabel(iniFile, lblDatCreator_DestinationFolder.Text, "DATAFILE_BUILDER", "DESTINATION_FOLDER_LABEL");
                btnDatCreator_BuildDat.Text = getGuiIniLabel(iniFile, btnDatCreator_BuildDat.Text, "DATAFILE_BUILDER", "BUILD_BUTTON");
                btnDatCreator_Cancel.Text = getGuiIniLabel(iniFile, btnDatCreator_Cancel.Text, "DATAFILE_BUILDER", "CANCEL_BUTTON");

                // Rebuilder Text
                grpRebuilder_Directories.Text = getGuiIniLabel(iniFile, grpRebuilder_Directories.Text, "REBUILDER", "DIRECTORY_GROUP_TAG");
                lblRebuilder_SourceDir.Text = getGuiIniLabel(iniFile, lblRebuilder_SourceDir.Text, "REBUILDER", "SOURCE_DIR_TAG");
                lblRebuilder_DestinationDir.Text = getGuiIniLabel(iniFile, lblRebuilder_DestinationDir.Text, "REBUILDER", "DESTINATION_DIR_TAG");
                grpRebuilder_Datafile.Text = getGuiIniLabel(iniFile, grpRebuilder_Datafile.Text, "REBUILDER", "DATAFILE_GROUP_TAG");
                grpRebuilder_Options.Text = getGuiIniLabel(iniFile, grpRebuilder_Options.Text, "REBUILDER", "OPTIONS_GROUP_TAG");
                cbRebuilder_RemoveSource.Text = getGuiIniLabel(iniFile, cbRebuilder_RemoveSource.Text, "REBUILDER", "CHECKBOX_REMOVE_REBUILT");
                cbRebuilder_Overwrite.Text = getGuiIniLabel(iniFile, cbRebuilder_Overwrite.Text, "REBUILDER", "CHECKBOX_OVERWRITE");
                cbRebuilder_CompressOutput.Text = getGuiIniLabel(iniFile, cbRebuilder_CompressOutput.Text, "REBUILDER", "CHECKBOX_COMPRESS_OUTPUT");
                btnRebuilder_Rebuild.Text = getGuiIniLabel(iniFile, btnRebuilder_Rebuild.Text, "REBUILDER", "REBUILD_BUTTON");
                btnRebuilder_Cancel.Text = getGuiIniLabel(iniFile, btnRebuilder_Cancel.Text, "REBUILDER", "CANCEL_BUTTON");

                // EXAMINE - MDX
                grpExamineMdx_PdxDiscovery.Text = getGuiIniLabel(iniFile, grpExamineMdx_PdxDiscovery.Text, "EXAMINE_MDX", "PDX_DISCOVERY_GROUP");
                cbMdxCheckPdxExist.Text = getGuiIniLabel(iniFile, cbMdxCheckPdxExist.Text, "EXAMINE_MDX", "PDX_EXIST_CHECKBOX");
                btnMdxFindPdx.Text = getGuiIniLabel(iniFile, btnMdxFindPdx.Text, "EXAMINE_MDX", "FIND_MDX_BUTTON");

                // EXAMINE - XSF
                grpExamineMdx_XsfExplorer.Text = getGuiIniLabel(iniFile, grpExamineMdx_XsfExplorer.Text, "EXAMINE_XSF", "XSF_EXPLORER_GROUP");
                lblExamineXsf_SourceDirectory.Text = getGuiIniLabel(iniFile, lblExamineXsf_SourceDirectory.Text, "EXAMINE_XSF", "XSF_SOURCE_DIRECTORY");
                btnXsfGetInfo.Text = getGuiIniLabel(iniFile, lblExamineXsf_SourceDirectory.Text, "EXAMINE_XSF", "XSF_INFO_BUTTON");
            }
        }

        // Set to INI value or keep internal default
        private string getGuiIniLabel(IniFile pIniFile, string pLabelText, string pSection, string pKey)
        {
            return pIniFile.IniReadValue(pSection, pKey).Trim().Length > 0 ?
                pIniFile.IniReadValue(pSection, pKey) : pLabelText;
        }

        # endregion
        
        #region BACKGROUND WORKER
        private void backgroundWorker_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != AuditingUtil.IGNORE_PROGRESS)
            {
                toolStripProgressBar.Value = e.ProgressPercentage;
            }

            if (e.UserState != null)
            {
                AuditingUtil.ProgressStruct vProgressStruct = (AuditingUtil.ProgressStruct)e.UserState;
                lblProgressLabel.Text = vProgressStruct.filename == null ? String.Empty : vProgressStruct.filename;
                tbOutput.Text += vProgressStruct.errorMessage == null ? String.Empty : vProgressStruct.errorMessage;
            }
        }

        private void rebuilderWorker_WorkComplete(object sender,
                                     RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                doCancelCleanup();
                toolStripStatusLabel1.Text = "Rebuilding...Cancelled";
                tbOutput.Text += "Operation cancelled.";
            }
            else
            {
                lblProgressLabel.Text = String.Empty;
                toolStripStatusLabel1.Text = "Rebuilding...Complete";
            }
        }

        private void datafileCreatorWorker_WorkComplete(object sender,
                                    RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                doCancelCleanup();
                toolStripStatusLabel1.Text = "Building Datafile...Canceled";
                tbOutput.Text += "Operation canceled.";                
            }
            else
            {
                lblProgressLabel.Text = String.Empty;
                
                datafile dataFile = new datafile();
                dataFile.header = DatafileCreatorWorker.buildHeader(tbDatCreator_Author.Text, tbDatCreator_Category.Text,
                    tbDatCreator_Comment.Text, tbDatCreator_Date.Text, tbDatCreator_Description.Text,
                    tbDatCreator_Email.Text, tbDatCreator_Homepage.Text, tbDatCreator_Name.Text,
                    tbDatCreator_Url.Text, tbDatCreator_Version.Text);

                dataFile.game = (game[])e.Result;

                XmlSerializer serializer = new XmlSerializer(dataFile.GetType());
                TextWriter textWriter = new StreamWriter(tbDatCreator_OutputDat.Text);
                serializer.Serialize(textWriter, dataFile);
                textWriter.Close();
                textWriter.Dispose();

                toolStripStatusLabel1.Text = "Building Datafile...Complete";
            }
        }
        #endregion        
    }
}
