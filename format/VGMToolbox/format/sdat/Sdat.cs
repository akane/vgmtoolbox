﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Checksums;

using VGMToolbox.util;
using VGMToolbox.util.ObjectPooling;

namespace VGMToolbox.format.sdat
{
    public class Sdat : IFormat
    {
        public static readonly byte[] ASCII_SIGNATURE = 
            new byte[] { 0x53, 0x44, 0x41, 0x54, 0xFF, 0xFE, 0x00, 0x01 }; // SDAT
        public const string ASCII_SIGNATURE_STRING = "53444154FFFE0001";

        private const string FORMAT_ABBREVIATION = "SDAT";
        private const string HEX_PREFIX = "0x";

        public const string SEQUENCE_FILE_EXTENSION = ".sseq";

        private static readonly byte[] EMPTY_WAVEARC = new byte[] { 0xFF, 0xFF};
        public const int NO_SEQUENCE_RESTRICTION = -1;

        struct SmapFatDataStruct
        {
            public int FileId;
            public UInt32 Offset;
            public UInt32 Size;
            public string Name;
        }

        ///////////////////////////////////
        // Standard NDS Header Information
        /// ///////////////////////////////
        public const int STD_HEADER_SIGNATURE_OFFSET = 0x00;
        public const int STD_HEADER_SIGNATURE_LENGTH = 4;

        public const int STD_HEADER_UNK_CONSTANT_OFFSET = 0x04;
        public const int STD_HEADER_UNK_CONSTANT_LENGTH = 4;

        public const int STD_HEADER_FILE_SIZE_OFFSET = 0x08;
        public const int STD_HEADER_FILE_SIZE_LENGTH = 4;

        public const int STD_HEADER_HEADER_SIZE_OFFSET = 0x0C;
        public const int STD_HEADER_HEADER_SIZE_LENGTH = 2;

        public const int STD_HEADER_NUMBER_OF_SECTIONS_OFFSET = 0x0E;
        public const int STD_HEADER_NUMBER_OF_SECTIONS_LENGTH = 2;

        ///////////////////////////////////
        // SDAT Specific Header Information
        ///////////////////////////////////
        // SYMB
        public const int SDAT_HEADER_SYMB_OFFSET_OFFSET = 0x10;
        public const int SDAT_HEADER_SYMB_OFFSET_LENGTH = 4;
        public const int SDAT_HEADER_SYMB_SIZE_OFFSET = 0x14;
        public const int SDAT_HEADER_SYMB_SIZE_LENGTH = 4;

        // INFO
        public const int SDAT_HEADER_INFO_OFFSET_OFFSET = 0x18;
        public const int SDAT_HEADER_INFO_OFFSET_LENGTH = 4;
        public const int SDAT_HEADER_INFO_SIZE_OFFSET = 0x1C;
        public const int SDAT_HEADER_INFO_SIZE_LENGTH = 4;

        // FAT
        public const int SDAT_HEADER_FAT_OFFSET_OFFSET = 0x20;
        public const int SDAT_HEADER_FAT_OFFSET_LENGTH = 4;
        public const int SDAT_HEADER_FAT_SIZE_OFFSET = 0x24;
        public const int SDAT_HEADER_FAT_SIZE_LENGTH = 4;

        // FILE
        public const int SDAT_HEADER_FILE_OFFSET_OFFSET = 0x28;
        public const int SDAT_HEADER_FILE_OFFSET_LENGTH = 4;
        public const int SDAT_HEADER_FILE_SIZE_OFFSET = 0x2C;
        public const int SDAT_HEADER_FILE_SIZE_LENGTH = 4;

        // UNKNOWN CONSTANT - PADDING?
        public const int SDAT_HEADER_UNK_PADDING_OFFSET = 0x30;
        public const int SDAT_HEADER_UNK_PADDING_LENGTH = 16;

        /////////////
        // Variables
        /////////////
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
                
        private byte[] stdHeaderSignature;
        private byte[] stdHeaderUnkConstant;
        private byte[] stdHeaderFileSize;
        private byte[] stdHeaderHeaderSize;
        private byte[] stdHeaderNumberOfSections;

        private byte[] sdatHeaderSymbOffset;
        private byte[] sdatHeaderSymbSize;

        private byte[] sdatHeaderInfoOffset;
        private byte[] sdatHeaderInfoSize;

        private byte[] sdatHeaderFatOffset;
        private byte[] sdatHeaderFatSize;

        private byte[] sdatHeaderFileOffset;
        private byte[] sdatHeaderFileSize;

        private byte[] sdatHeaderUnkPadding;

        // Tag Hash
        Dictionary<string, string> tagHash = new Dictionary<string, string>();

        // Smap Fat Info Struct
        SmapFatDataStruct[] smapFatInfo;

        // Sections
        private SdatSymbSection symbSection = null;
        private SdatInfoSection infoSection = null;
        private SdatFatSection fatSection = null;
        private SdatFileSection fileSection = null;

        public SdatSymbSection SymbSection { get { return symbSection; } }
        public SdatInfoSection InfoSection { get { return infoSection; } }
        public SdatFatSection FatSection { get { return fatSection; } }
        public SdatFileSection FileSection { get { return fileSection; } }

        // Optimization Arrays
        bool[] fileIdChecklist;

        // METHODS        
        public void Initialize(Stream pStream, string pFilePath)
        { 
            this.filePath = pFilePath;
            
            // SDAT
            stdHeaderSignature = ParseFile.parseSimpleOffset(pStream, STD_HEADER_SIGNATURE_OFFSET, STD_HEADER_SIGNATURE_LENGTH);
            stdHeaderUnkConstant = ParseFile.parseSimpleOffset(pStream, STD_HEADER_UNK_CONSTANT_OFFSET, STD_HEADER_UNK_CONSTANT_LENGTH);
            stdHeaderFileSize = ParseFile.parseSimpleOffset(pStream, STD_HEADER_FILE_SIZE_OFFSET, STD_HEADER_FILE_SIZE_LENGTH);
            stdHeaderHeaderSize = ParseFile.parseSimpleOffset(pStream, STD_HEADER_HEADER_SIZE_OFFSET, STD_HEADER_HEADER_SIZE_LENGTH);
            stdHeaderNumberOfSections = ParseFile.parseSimpleOffset(pStream, STD_HEADER_NUMBER_OF_SECTIONS_OFFSET, STD_HEADER_NUMBER_OF_SECTIONS_LENGTH);

            sdatHeaderSymbOffset = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_SYMB_OFFSET_OFFSET, SDAT_HEADER_SYMB_OFFSET_LENGTH);
            sdatHeaderSymbSize = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_SYMB_SIZE_OFFSET, SDAT_HEADER_SYMB_SIZE_LENGTH);

            sdatHeaderInfoOffset = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_INFO_OFFSET_OFFSET, SDAT_HEADER_INFO_OFFSET_LENGTH);
            sdatHeaderInfoSize = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_INFO_SIZE_OFFSET, SDAT_HEADER_INFO_SIZE_LENGTH);

            sdatHeaderFatOffset = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_FAT_OFFSET_OFFSET, SDAT_HEADER_FAT_OFFSET_LENGTH);
            sdatHeaderFatSize = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_FAT_SIZE_OFFSET, SDAT_HEADER_FAT_SIZE_LENGTH);

            sdatHeaderFileOffset = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_FILE_OFFSET_OFFSET, SDAT_HEADER_FILE_OFFSET_LENGTH);
            sdatHeaderFileSize = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_FILE_SIZE_OFFSET, SDAT_HEADER_FILE_SIZE_LENGTH);

            sdatHeaderUnkPadding = ParseFile.parseSimpleOffset(pStream, SDAT_HEADER_UNK_PADDING_OFFSET, SDAT_HEADER_UNK_PADDING_LENGTH);

            // SYMB Section
            if (BitConverter.ToUInt32(this.sdatHeaderSymbSize, 0) > 0)
            {
                symbSection = new SdatSymbSection();
                symbSection.Initialize(pStream, BitConverter.ToInt32(this.sdatHeaderSymbOffset, 0));
            }
            
            // INFO            
            if (BitConverter.ToUInt32(this.sdatHeaderInfoSize, 0) > 0)
            {
                infoSection = new SdatInfoSection();
                infoSection.Initialize(pStream, BitConverter.ToInt32(this.sdatHeaderInfoOffset, 0));
            }
            
             
            // FAT Section
            if (BitConverter.ToUInt32(this.sdatHeaderFatSize, 0) > 0)
            {
                fatSection = new SdatFatSection();
                fatSection.Initialize(pStream, BitConverter.ToInt32(this.sdatHeaderFatOffset, 0));
            }
            
            // FILE Section
            if (BitConverter.ToUInt32(this.sdatHeaderFileSize, 0) > 0)
            {
                fileSection = new SdatFileSection();
                fileSection.Initialize(pStream, BitConverter.ToInt32(this.sdatHeaderFileOffset, 0));
            }

            //this.initializeTagHash();
        }

        private void addNumberedListToTagHash(string pLabel, string[] pList)
        {
            int fileCount = 0;
            foreach (string s in pList)
            {
                tagHash.Add(pLabel + " " + fileCount.ToString("X4"), s);
                fileCount++;
            }        
        }
        
        private void initializeTagHash()
        {
            #region initializeTagHash - SDAT
            
            tagHash.Add("SDAT - File Size", HEX_PREFIX + BitConverter.ToUInt32(this.stdHeaderFileSize, 0).ToString("X4"));
            tagHash.Add("SDAT - Header Size", HEX_PREFIX + BitConverter.ToUInt16(this.stdHeaderHeaderSize, 0).ToString("X2"));
            tagHash.Add("SDAT - Number of Sections", HEX_PREFIX + BitConverter.ToUInt16(this.stdHeaderNumberOfSections, 0).ToString("X2"));
            tagHash.Add("SDAT - SYMB Offset, Length",
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderSymbOffset, 0).ToString("X4") + ", " +
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderSymbSize, 0).ToString("X4"));
            tagHash.Add("SDAT - INFO Offset, Length",
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderInfoOffset, 0).ToString("X4") + ", " +
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderInfoSize, 0).ToString("X4"));
            tagHash.Add("SDAT - FAT Offset, Length",
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderFatOffset, 0).ToString("X4") + ", " +
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderFatSize, 0).ToString("X4"));
            tagHash.Add("SDAT - FILE Offset, Length",
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderFileOffset, 0).ToString("X4") + ", " +
                HEX_PREFIX + BitConverter.ToUInt32(this.sdatHeaderFileSize, 0).ToString("X4"));
            
            #endregion

            #region initializeTagHash - SYMB (INCOMPLETE)
            
            if (symbSection != null)
            {                
                tagHash.Add("SYMB - Section Size", HEX_PREFIX + BitConverter.ToUInt32(symbSection.StdHeaderSectionSize, 0).ToString("X4"));
                tagHash.Add("SYMB - SEQ Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordSeqOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - SEQARC Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordSeqArcOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - BANK Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordBankOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - WAVEARC Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordWaveArcOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - PLAYER Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordPlayerOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - GROUP Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordGroupOffset, 0).ToString("X4"));
                tagHash.Add("SYMB - PLAYER2 Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordPlayer2Offset, 0).ToString("X4"));
                tagHash.Add("SYMB - STRM Offset", HEX_PREFIX + BitConverter.ToUInt32(symbSection.SymbRecordStrmOffset, 0).ToString("X4"));
                
                addNumberedListToTagHash("SYMB - SEQ", symbSection.SymbSeqFileNames);

                addNumberedListToTagHash("SYMB - BANK", symbSection.SymbBankFileNames);
                addNumberedListToTagHash("SYMB - WAVEARC", symbSection.SymbWaveArcFileNames);
                addNumberedListToTagHash("SYMB - PLAYER", symbSection.SymbPlayerFileNames);
                addNumberedListToTagHash("SYMB - GROUP", symbSection.SymbGroupFileNames);
                addNumberedListToTagHash("SYMB - PLAYER2", symbSection.SymbPlayer2FileNames);
                addNumberedListToTagHash("SYMB - STRM", symbSection.SymbStrmFileNames);                
            }
            
            #endregion

            #region initializeTagHash - FILE (INCOMPLETE)

            if (fileSection != null)
            {
                tagHash.Add("FILE - Total Files", HEX_PREFIX + BitConverter.ToUInt32(fileSection.FileHeaderNumberOfFiles, 0).ToString("X4"));
            }

            #endregion

            #region initializeTagHash - FAT

            if (fatSection != null)
            {
                UInt32 numberOfFatRecords = BitConverter.ToUInt32(fatSection.FatHeaderNumberOfFiles, 0);

                tagHash.Add("FAT - Section Size", HEX_PREFIX + BitConverter.ToUInt32(fatSection.FatHeaderSectionSize, 0).ToString("X4"));
                tagHash.Add("FAT - Number of FAT Records", HEX_PREFIX + numberOfFatRecords.ToString("X4"));

                for (int i = 0; i < numberOfFatRecords; i++)
                {
                    string hashKey = String.Format("FAT - FAT Record {0}{1} Offset, Size", HEX_PREFIX, i.ToString("X4"));
                    string hashValue = String.Format("{0}{1}, {2}{3}", HEX_PREFIX, 
                        BitConverter.ToUInt32(fatSection.SdatFatRecs[i].nOffset, 0).ToString("X4"),
                        HEX_PREFIX, 
                        BitConverter.ToUInt32(fatSection.SdatFatRecs[i].nSize, 0).ToString("X4"));
                    tagHash.Add(hashKey, hashValue);
                }                
            }

            #endregion

            #region initializeTagHash - INFO

            if (infoSection != null)
            {
                int sdatInfoSseqCounter = 0;
                foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
                {
                    tagHash.Add("INFO - SSEQ " + HEX_PREFIX + sdatInfoSseqCounter.ToString("X4") + " FileID", 
                        HEX_PREFIX + BitConverter.ToUInt16(s.fileId, 0).ToString("X2"));
                }
            }
            
            #endregion
        }

        public void ExtractSseqs(Stream pStream, string pOutputPath)
        {
            BinaryWriter bw;
            string fileName = String.Empty;

            int i = 0;
            foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
            {
                if (s.fileId != null)
                {
                    // get file information                
                    int fileId = BitConverter.ToInt16(s.fileId, 0);
                    int fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    int fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nSize, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbSeqFileNames.Length) && 
                        (!String.IsNullOrEmpty(symbSection.SymbSeqFileNames[i])))
                    {
                        fileName = symbSection.SymbSeqFileNames[i] + ".sseq";
                    }
                    else
                    {
                        fileName = String.Format("SSEQ{0}.sseq", fileId.ToString("X4"));
                    }

                    string outputDirectory = Path.Combine(pOutputPath, "seq");
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }
                    fileName = Path.Combine(outputDirectory, fileName);

                    int read = 0;
                    int totalRead = 0;
                    int maxRead;
                    byte[] data = new byte[4096];

                    pStream.Seek(fileOffset, SeekOrigin.Begin);
                    maxRead = fileSize < data.Length ? fileSize : data.Length;

                    bw = new BinaryWriter(File.Create(fileName));

                    while ((maxRead > 0) && (read = pStream.Read(data, 0, maxRead)) > 0)
                    {
                        bw.Write(data, 0, read);

                        totalRead += read;
                        maxRead = (fileSize - totalRead) < data.Length ? (fileSize - totalRead) : data.Length;
                    }

                    bw.Close();
                } // if (s.fileId != null)
                
                i++;
            }
        }
        public void ExtractStrms(Stream pStream, string pOutputPath)
        {
            BinaryWriter bw;
            string fileName = String.Empty;

            int i = 0;
            foreach (SdatInfoSection.SdatInfoStrm s in infoSection.SdatInfoStrms)
            {

                if (s.fileId != null)
                {
                    // get file information
                    int fileId = BitConverter.ToInt16(s.fileId, 0);
                    int fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    int fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nSize, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbStrmFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbStrmFileNames[i])))
                    {
                        fileName = symbSection.SymbStrmFileNames[i] + ".strm";
                    }
                    else
                    {
                        fileName = String.Format("STRM{0}.strm", fileId.ToString("X4"));
                    }

                    string outputDirectory = Path.Combine(pOutputPath, "strm");
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }
                    fileName = Path.Combine(outputDirectory, fileName);

                    int read = 0;
                    int totalRead = 0;
                    int maxRead;
                    byte[] data = new byte[4096];

                    pStream.Seek(fileOffset, SeekOrigin.Begin);
                    maxRead = fileSize < data.Length ? fileSize : data.Length;

                    bw = new BinaryWriter(File.Create(fileName));

                    while ((maxRead > 0) && (read = pStream.Read(data, 0, maxRead)) > 0)
                    {
                        bw.Write(data, 0, read);

                        totalRead += read;
                        maxRead = (fileSize - totalRead) < data.Length ? (fileSize - totalRead) : data.Length;
                    }

                    bw.Close();
                }

                i++;
            }
        }

        public void OptimizeForZlib(int pStartSequence, int pEndSequence)
        {
            fileIdChecklist = new bool[BitConverter.ToUInt32(this.fatSection.FatHeaderNumberOfFiles, 0)];
            
            this.ZeroOutStrms();
            this.ZeroOutSequences(pStartSequence, pEndSequence);
            this.ZeroOutWavArcs(pStartSequence, pEndSequence);
            this.ZeroOutBanks(pStartSequence, pEndSequence);

            this.zeroOutFilesById();
        }

        private void ZeroOutStrms()
        {            
            foreach (SdatInfoSection.SdatInfoStrm s in infoSection.SdatInfoStrms)
            {
                if (s.fileId != null)
                {
                    // get file information
                    int fileId = BitConverter.ToInt16(s.fileId, 0);
                    int fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    int fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nSize, 0);

                    FileUtil.ZeroOutFileChunk(this.filePath, fileOffset, fileSize);   
                }
            }
        }

        private void ZeroOutSequences(int pStartSequence, int pEndSequence)
        {
            UInt16 bankId;
            bool checkSequences = false;
            bool[] sseqIsUsed = new bool[infoSection.SdatInfoSseqs.Length];

            SdatInfoSection.SdatInfoSseq sseqInfo;
            int fileId;

            if ((pStartSequence != NO_SEQUENCE_RESTRICTION) && (pEndSequence != NO_SEQUENCE_RESTRICTION))
            {
                checkSequences = true;
            }

            int i = 0;
            foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
            {
                if ((s.fileId != null) &&
                    ((!checkSequences) ||
                    ((checkSequences) && (i >= pStartSequence) && (i <= pEndSequence))))
                {
                    sseqIsUsed[i] = true;
                }

                i++;
            }

            for (i = 0; i < sseqIsUsed.Length; i++)
            {
                if (sseqIsUsed[i])
                {
                    sseqInfo = infoSection.SdatInfoSseqs[i];

                    if (sseqInfo.fileId != null)
                    {
                        fileId = BitConverter.ToInt16(sseqInfo.fileId, 0);
                        this.fileIdChecklist[fileId] = true;
                    }
                }

                //if (!bankIsUsed[i])
                //{
                //    this.ZeroOutBank((ushort)i);
                //}
            }
        }

        private void ZeroOutWavArcs(int pStartSequence, int pEndSequence)
        {
            UInt16 bankId;
            SdatInfoSection.SdatInfoBank bankInfo;
            bool checkSequences = false;
            bool[] waveArcIsUsed = new bool[infoSection.SdatInfoWaveArcs.Length];

            SdatInfoSection.SdatInfoWaveArc waveArcInfo;
            int fileId;

            if ((pStartSequence != NO_SEQUENCE_RESTRICTION) && (pEndSequence != NO_SEQUENCE_RESTRICTION))
            {
                checkSequences = true;
            }

            int i = 0;
            int j;
            foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
            {
                if ((s.fileId != null) &&
                    ((!checkSequences) ||
                    ((checkSequences) && (i >= pStartSequence) && (i <= pEndSequence))))
                {
                    bankId = BitConverter.ToUInt16(s.bnk, 0);
                    bankInfo = infoSection.SdatInfoBanks[bankId];

                    for (j = 0; j < 4; j++)
                    {
                        if (!ParseFile.CompareSegment(bankInfo.wa[j], 0, EMPTY_WAVEARC))
                        {
                            waveArcIsUsed[BitConverter.ToUInt16(bankInfo.wa[j], 0)] = true;                            
                        }
                    }
                }
                
                i++;
            }

            for (i = 0; i < waveArcIsUsed.Length; i++)
            {
                if (waveArcIsUsed[i])
                {
                    waveArcInfo = infoSection.SdatInfoWaveArcs[i];

                    if (waveArcInfo.fileId != null)
                    {
                        fileId = BitConverter.ToInt16(waveArcInfo.fileId, 0);
                        this.fileIdChecklist[fileId] = true;
                    }
                }                
                
                //if (!waveArcIsUsed[i])
                //{
                //    this.ZeroOutWaveArc((ushort) i);
                //}
            }
        }
        private void ZeroOutWaveArc(UInt16 pWaveArcId)
        {
            SdatInfoSection.SdatInfoWaveArc waveArcInfo = new SdatInfoSection.SdatInfoWaveArc();
            waveArcInfo = infoSection.SdatInfoWaveArcs[pWaveArcId];

            if (waveArcInfo.fileId != null)
            {
                // get file information
                int fileId = BitConverter.ToInt16(waveArcInfo.fileId, 0);
                int fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                int fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nSize, 0);

                FileUtil.ZeroOutFileChunk(this.filePath, fileOffset, fileSize);
            }
        }

        private void ZeroOutBanks(int pStartSequence, int pEndSequence)
        {
            UInt16 bankId;
            bool checkSequences = false;
            bool[] bankIsUsed = new bool[infoSection.SdatInfoBanks.Length];

            SdatInfoSection.SdatInfoBank bankInfo;
            int fileId;

            if ((pStartSequence != NO_SEQUENCE_RESTRICTION) && (pEndSequence != NO_SEQUENCE_RESTRICTION))
            {
                checkSequences = true;
            }

            int i = 0;
            foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
            {
                if ((s.fileId != null) && 
                    ((!checkSequences) ||
                    ((checkSequences) && (i >= pStartSequence) && (i <= pEndSequence))))
                {
                    bankId = BitConverter.ToUInt16(s.bnk, 0);
                    bankIsUsed[bankId] = true;                   
                }

                i++;
            }

            for (i = 0; i < bankIsUsed.Length; i++)
            {
                if (bankIsUsed[i])
                {
                    bankInfo = infoSection.SdatInfoBanks[i];

                    if (bankInfo.fileId != null)
                    {
                        fileId = BitConverter.ToInt16(bankInfo.fileId, 0);
                        this.fileIdChecklist[fileId] = true;
                    }
                }                
                
                //if (!bankIsUsed[i])
                //{
                //    this.ZeroOutBank((ushort)i);
                //}
            }
        }
        private void ZeroOutBank(UInt16 pBankId)
        {
            SdatInfoSection.SdatInfoBank bankInfo = new SdatInfoSection.SdatInfoBank();
            bankInfo = infoSection.SdatInfoBanks[pBankId];

            if (bankInfo.fileId != null)
            {
                // get file information
                int fileId = BitConverter.ToInt16(bankInfo.fileId, 0);
                int fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                int fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[fileId].nSize, 0);

                FileUtil.ZeroOutFileChunk(this.filePath, fileOffset, fileSize);
            }
        }

        private void zeroOutFilesById()
        {
            int fileOffset;
            int fileSize;

            for (int i = 0; i < this.fileIdChecklist.Length; i++)
            {
                if (!this.fileIdChecklist[i])
                {
                    fileOffset = BitConverter.ToInt32(fatSection.SdatFatRecs[i].nOffset, 0);
                    fileSize = BitConverter.ToInt32(fatSection.SdatFatRecs[i].nSize, 0);

                    FileUtil.ZeroOutFileChunk(this.filePath, fileOffset, fileSize);
                }
            }
        }

        public void BuildSmap(string pOutputPath, string pFilePrefix)
        { 
            checkOutputDirectory(pOutputPath);

            this.smapFatInfo = new SmapFatDataStruct[this.FatSection.SdatFatRecs.Length];

            for (int i = 0; i < this.smapFatInfo.Length; i++)
            {
                this.smapFatInfo[i] = new SmapFatDataStruct();
            }

            buildSmapSeq(pOutputPath, pFilePrefix);
            buildSmapSeqArc(pOutputPath, pFilePrefix);
            buildSmapBank(pOutputPath, pFilePrefix);
            buildSmapWaveArc(pOutputPath, pFilePrefix);
            buildSmapStrm(pOutputPath, pFilePrefix);
            buildSmapFat(pOutputPath, pFilePrefix);
        }

        private void buildSmapSeq(string pOutputPath, string pFilePrefix)
        {            
            string smapFileName = pFilePrefix + ".smap";
            string fileName;
            int fileId;

            SmapFatDataStruct fatData;
            
            StreamWriter sw = File.CreateText(Path.Combine(pOutputPath, smapFileName));

            sw.WriteLine(@"# SEQ:");
            sw.WriteLine(@"# label                     number fileID bnk vol cpr ppr ply      hsize       size name");

            int i = 0;
            string lineOut = String.Empty;
            foreach (SdatInfoSection.SdatInfoSseq s in infoSection.SdatInfoSseqs)
            {
                lineOut = String.Empty;

                if (s.fileId != null)
                {
                    fileId = BitConverter.ToInt16(s.fileId, 0);
                    
                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbSeqFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbSeqFileNames[i])))
                    {
                        fileName = symbSection.SymbSeqFileNames[i] + ".sseq";
                    }
                    else
                    {
                        fileName = String.Format("SSEQ{0}.sseq", fileId.ToString("X4"));
                    }

                    lineOut += "  " + Path.GetFileNameWithoutExtension(fileName).PadRight(26).Substring(0, 26);
                    lineOut += i.ToString().PadLeft(6);
                    lineOut += BitConverter.ToInt16(s.fileId, 0).ToString().PadLeft(7);
                    lineOut += BitConverter.ToInt16(s.bnk, 0).ToString().PadLeft(4);
                    lineOut += s.vol[0].ToString().PadLeft(4);
                    lineOut += s.cpr[0].ToString().PadLeft(4);
                    lineOut += s.ppr[0].ToString().PadLeft(4);
                    lineOut += s.ply[0].ToString().PadLeft(4);

                    lineOut += " ".PadLeft(11); // hsize?
                    lineOut += BitConverter.ToInt32(fatSection.SdatFatRecs[BitConverter.ToInt16(s.fileId, 0)].nSize, 0).ToString().PadLeft(11);
                    lineOut += @" \Seq\" + fileName;

                    fatData.FileId = fileId;
                    fatData.Offset = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    fatData.Size = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nSize, 0);
                    fatData.Name = @" \Seq\" + fileName;
                    this.smapFatInfo[fileId] = fatData;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(34);
                }

                sw.WriteLine(lineOut);

                i++;
            }
        
            sw.Close();
            sw.Dispose();
        }

        private void buildSmapSeqArc(string pOutputPath, string pFilePrefix)
        {
            string smapFileName = pFilePrefix + ".smap";
            string fileName;
            int fileId;

            SmapFatDataStruct fatData;

            StreamWriter sw = new StreamWriter(File.Open(Path.Combine(pOutputPath, smapFileName), FileMode.Append, FileAccess.Write));

            sw.WriteLine();
            sw.WriteLine(@"# SEQARC:");
            sw.WriteLine(@"# label                     number fileID                                      size name");

            int i = 0;
            string lineOut = String.Empty;

            foreach (SdatInfoSection.SdatInfoSeqArc s in infoSection.SdatInfoSeqArcs)
            {
                lineOut = String.Empty;

                if (s.fileId != null)
                {
                    fileId = BitConverter.ToInt16(s.fileId, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbSeqArcFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbSeqArcFileNames[i])))
                    {
                        fileName = symbSection.SymbSeqArcFileNames[i] + ".ssar";
                    }
                    else
                    {
                        fileName = String.Format("SSAR{0}.ssar", fileId.ToString("X4"));
                    }

                    lineOut += "  " + Path.GetFileNameWithoutExtension(fileName).PadRight(26).Substring(0, 26);
                    lineOut += i.ToString().PadLeft(6);
                    lineOut += BitConverter.ToInt16(s.fileId, 0).ToString().PadLeft(7);

                    lineOut += BitConverter.ToInt32(fatSection.SdatFatRecs[BitConverter.ToInt16(s.fileId, 0)].nSize, 0).ToString().PadLeft(42);
                    lineOut += @" \SeqArc\" + fileName;

                    fatData.FileId = fileId;
                    fatData.Offset = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    fatData.Size = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nSize, 0);
                    fatData.Name = @" \SeqArc\" + fileName;
                    this.smapFatInfo[fileId] = fatData;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(34);
                }

                sw.WriteLine(lineOut);

                i++;
            }

            sw.Close();
            sw.Dispose();
        }

        private void buildSmapBank(string pOutputPath, string pFilePrefix)
        {
            string smapFileName = pFilePrefix + ".smap";
            string fileName;
            int fileId;

            SmapFatDataStruct fatData;
            
            StreamWriter sw = new StreamWriter(File.Open(Path.Combine(pOutputPath, smapFileName), FileMode.Append, FileAccess.Write));

            sw.WriteLine();
            sw.WriteLine(@"# BANK:");
            sw.WriteLine(@"# label                     number fileID wa0 wa1 wa2 wa3         hsize        size name");

            int i = 0;
            string lineOut = String.Empty;

            foreach (SdatInfoSection.SdatInfoBank s in infoSection.SdatInfoBanks)
            {
                lineOut = String.Empty;

                if (s.fileId != null)
                {
                    fileId = BitConverter.ToInt16(s.fileId, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbBankFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbBankFileNames[i])))
                    {
                        fileName = symbSection.SymbBankFileNames[i] + ".sbnk";
                    }
                    else
                    {
                        fileName = String.Format("SBNK{0}.sbnk", fileId.ToString("X4"));
                    }

                    lineOut += "  " + Path.GetFileNameWithoutExtension(fileName).PadRight(26).Substring(0, 26);
                    lineOut += i.ToString().PadLeft(6);
                    lineOut += BitConverter.ToInt16(s.fileId, 0).ToString().PadLeft(7);

                    for (int j = 0; j < 4; j++)
                    {
                        if (ParseFile.CompareSegment(s.wa[j], 0, EMPTY_WAVEARC))
                        {
                            lineOut += String.Empty.PadLeft(4);
                        }
                        else
                        {
                            lineOut += BitConverter.ToUInt16(s.wa[j], 0).ToString().PadLeft(4);
                        }
                    }

                    lineOut += BitConverter.ToInt32(fatSection.SdatFatRecs[BitConverter.ToInt16(s.fileId, 0)].nSize, 0).ToString().PadLeft(26);
                    lineOut += @" \Bank\" + fileName;

                    fatData.FileId = fileId;
                    fatData.Offset = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    fatData.Size = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nSize, 0);
                    fatData.Name = @" \Bank\" + fileName;
                    this.smapFatInfo[fileId] = fatData;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(34);
                }

                sw.WriteLine(lineOut);

                i++;
            }

            sw.Close();
            sw.Dispose();
        }

        private void buildSmapWaveArc(string pOutputPath, string pFilePrefix)
        {
            string smapFileName = pFilePrefix + ".smap";
            string fileName;
            int fileId;

            SmapFatDataStruct fatData;
            
            StreamWriter sw = new StreamWriter(File.Open(Path.Combine(pOutputPath, smapFileName), FileMode.Append, FileAccess.Write));

            sw.WriteLine();
            sw.WriteLine(@"# WAVEARC:");
            sw.WriteLine(@"# label                     number fileID                                      size name");

            int i = 0;
            string lineOut = String.Empty;

            foreach (SdatInfoSection.SdatInfoWaveArc s in infoSection.SdatInfoWaveArcs)
            {
                lineOut = String.Empty;

                if (s.fileId != null)
                {
                    fileId = BitConverter.ToInt16(s.fileId, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbWaveArcFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbWaveArcFileNames[i])))
                    {
                        fileName = symbSection.SymbWaveArcFileNames[i] + ".swar";
                    }
                    else
                    {
                        fileName = String.Format("SWAR{0}.swar", fileId.ToString("X4"));
                    }

                    lineOut += "  " + Path.GetFileNameWithoutExtension(fileName).PadRight(26).Substring(0, 26);
                    lineOut += i.ToString().PadLeft(6);
                    lineOut += BitConverter.ToInt16(s.fileId, 0).ToString().PadLeft(7);

                    lineOut += BitConverter.ToInt32(fatSection.SdatFatRecs[BitConverter.ToInt16(s.fileId, 0)].nSize, 0).ToString().PadLeft(42);
                    lineOut += @" \WaveArc\" + fileName;

                    fatData.FileId = fileId;
                    fatData.Offset = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    fatData.Size = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nSize, 0);
                    fatData.Name = @" \WaveArc\" + fileName;
                    this.smapFatInfo[fileId] = fatData;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(34);
                }

                sw.WriteLine(lineOut);

                i++;
            }

            sw.Close();
            sw.Dispose();
        }

        private void buildSmapStrm(string pOutputPath, string pFilePrefix)
        {
            string smapFileName = pFilePrefix + ".smap";
            string fileName;
            int fileId;

            SmapFatDataStruct fatData;
            
            StreamWriter sw = new StreamWriter(File.Open(Path.Combine(pOutputPath, smapFileName), FileMode.Append, FileAccess.Write));

            sw.WriteLine();
            sw.WriteLine(@"# STRM:");
            sw.WriteLine(@"# label                     number fileID vol pri ply                          size name");

            int i = 0;
            string lineOut = String.Empty;
            
            foreach (SdatInfoSection.SdatInfoStrm s in infoSection.SdatInfoStrms)
            {
                lineOut = String.Empty;

                if (s.fileId != null)
                {
                    fileId = BitConverter.ToInt16(s.fileId, 0);

                    // get filename, if exists                                
                    if ((symbSection != null) && (i < symbSection.SymbStrmFileNames.Length) &&
                        (!String.IsNullOrEmpty(symbSection.SymbStrmFileNames[i])))
                    {
                        fileName = symbSection.SymbStrmFileNames[i] + ".strm";
                    }
                    else
                    {
                        fileName = String.Format("STRM{0}.strm", fileId.ToString("X4"));
                    }

                    lineOut += "  " + Path.GetFileNameWithoutExtension(fileName).PadRight(26).Substring(0, 26);
                    lineOut += i.ToString().PadLeft(6);
                    lineOut += BitConverter.ToInt16(s.fileId, 0).ToString().PadLeft(7);
                    lineOut += s.vol[0].ToString().PadLeft(4);
                    lineOut += s.pri[0].ToString().PadLeft(4);
                    lineOut += s.ply[0].ToString().PadLeft(4);

                    lineOut += BitConverter.ToInt32(fatSection.SdatFatRecs[BitConverter.ToInt16(s.fileId, 0)].nSize, 0).ToString().PadLeft(30);
                    lineOut += @" \Strm\" + fileName;

                    fatData.FileId = fileId;
                    fatData.Offset = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nOffset, 0);
                    fatData.Size = BitConverter.ToUInt32(fatSection.SdatFatRecs[fileId].nSize, 0);
                    fatData.Name = @" \Strm\" + fileName;
                    this.smapFatInfo[fileId] = fatData;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(34);
                }

                sw.WriteLine(lineOut);

                i++;
            }

            sw.Close();
            sw.Dispose();
        }

        private void buildSmapFat(string pOutputPath, string pFilePrefix)
        {
            string smapFileName = pFilePrefix + ".smap";
            SmapFatDataStruct s;            
            StreamWriter sw = new StreamWriter(File.Open(Path.Combine(pOutputPath, smapFileName), FileMode.Append, FileAccess.Write));

            sw.WriteLine();
            sw.WriteLine(@"# FAT:");
            sw.WriteLine(@"# fileID     offset       size name");

            string lineOut = String.Empty;

            for (int i = 0; i < this.smapFatInfo.Length; i++)
            {
                lineOut = String.Empty;

                s = this.smapFatInfo[i];

                if (!String.IsNullOrEmpty(s.Name))
                {
                    s = this.smapFatInfo[i];
                    lineOut = s.FileId.ToString().PadLeft(8);
                    lineOut += (" 0x" + s.Offset.ToString("X8")).PadLeft(10);
                    lineOut += s.Size.ToString().PadLeft(11);
                    lineOut += "  " + s.Name;
                }
                else
                {
                    lineOut = i.ToString().PadLeft(8);
                }

                sw.WriteLine(lineOut);
            }

            sw.Close();
            sw.Dispose();
        }

        private void checkOutputDirectory(string pOutputPath)
        {
            if (!Directory.Exists(pOutputPath))
            {
                Directory.CreateDirectory(pOutputPath);
            }
        }

        #region IFormat Required Functions
        
        public byte[] GetAsciiSignature()
        {
            return ASCII_SIGNATURE;
        }

        public string GetFileExtensions()
        {
            return null;
        }

        public string GetFormatAbbreviation()
        { 
            return FORMAT_ABBREVIATION;
        }

        public bool IsFileLibrary() { return false; }

        public bool HasMultipleFileExtensions() { return false; }

        public bool UsesLibraries() { return false; }
        public bool IsLibraryPresent() { return true; }

        public Dictionary<string, string> GetTagHash() { return this.tagHash; }
        public void GetDatFileCrc32(ref Crc32 pChecksum) { pChecksum.Reset();}
        
        #endregion

    }        
}

