﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.util;

namespace VGMToolbox.format.iso
{
    // Thanks to crediar's DiscU.exe output, CarlKenner, soneek
    public class NintendoWiiUOpticalDisc
    {
        
        public static readonly byte[] STANDARD_IDENTIFIER = new byte[] { 0x57, 0x55, 0x50, 0x2D }; // "WUP-"
        public const uint IDENTIFIER_OFFSET = 0;
        public static string FORMAT_DESCRIPTION_STRING = "WiiU Encrypted";
        public static string FORMAT_DESCRIPTION_STRING_DECRYPTED = "WiiU Decrypted";
        
        private static readonly string PROGRAMS_FOLDER =
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "external"));
        public static readonly string WIIU_EXTERNAL_FOLDER = Path.Combine(PROGRAMS_FOLDER, "wiiu");
        public static readonly string COMMON_KEY_PATH = Path.Combine(WIIU_EXTERNAL_FOLDER, "ckey.bin");
        public static readonly string DISC_KEY_FILE_NAME = "disckey.bin";

        public const ulong WIIU_DECRYPTED_AREA_OFFSET = 0x18000;
        public const uint PARTITION_TOC_OFFSET = 0x800;
        public const uint PARTITION_TOC_ENTRY_SIZE = 0x80;

        public static readonly byte[] DECRYPTED_AREA_SIGNATURE = new byte[] { 0xCC, 0xA6, 0xE6, 0x7B };
        public static readonly byte[] PARTITION_FILE_TABLE_SIGNATURE = new byte[] { 0x46, 0x53, 0x54, 0x00 }; // "FST"

        public const string TITLE_TICKET_FILE = "TITLE.TIK";

        public NintendoWiiUEncryptedDiscReader DiscReader { set; get; }

        private byte[] CommonKey;
        private byte[] DiscKey;

        public string GameSerial { set; get; }
        public string GameRevision { set; get; }
        public string GameRegion { set; get; }
        public string SystemMenuVersion { set; get; }

        public long DiscBaseOffset { set; get; }
        public ulong DecryptedAreaOffset { set; get; }
        public bool IsRawDump { set; get; }

        public uint PartitionCount { set; get; }
        public NintendoWiiUPartition[] Partitions { set; get; }

        public ArrayList VolumeArrayList { set; get; }
        public NintendoWiiUOpticalDiscVolume[] Volumes
        {
            get
            {
                return (NintendoWiiUOpticalDiscVolume[])this.VolumeArrayList.ToArray(typeof(NintendoWiiUOpticalDiscVolume));
            }

        }

        public Dictionary<string, NintendoWiiUTitleKeyStruct> TitleKeyHash = 
            new Dictionary<string, NintendoWiiUTitleKeyStruct>();

        public static byte[] GetKeyFromFile(string path)
        {
            byte[] keyArray = null;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(String.Format("Key not found: {0}.{1}", path, Environment.NewLine));
            }
            else
            {
                keyArray = new byte[0x10];

                using (FileStream fs = File.OpenRead(path))
                {
                    fs.Read(keyArray, 0, 0x10);
                }
            }

            return keyArray;
        }

        public static WiiUBlockStructure GetWiiUBlockStructureForOffset(long offset)
        {
            WiiUBlockStructure bs = new WiiUBlockStructure();

            bs.BlockNumber = offset / 0x8000;
            bs.BlockOffset = offset % 0x8000;

            return bs;
        }

        public void Initialize(FileStream isoStream, long offset, bool isRawDump)
        {
            this.CommonKey = GetKeyFromFile(COMMON_KEY_PATH);
            this.DiscKey = GetKeyFromFile(Path.Combine(Path.GetDirectoryName(isoStream.Name), DISC_KEY_FILE_NAME));
            
            this.GameSerial = ByteConversion.GetAsciiText(ParseFile.ParseSimpleOffset(isoStream, this.DiscBaseOffset, 9));
            this.GameRevision = ByteConversion.GetAsciiText(ParseFile.ParseSimpleOffset(isoStream, this.DiscBaseOffset + 0xB, 2));
            this.SystemMenuVersion = ByteConversion.GetAsciiText(ParseFile.ParseSimpleOffset(isoStream, this.DiscBaseOffset + 0xE, 3));
            this.GameRegion = ByteConversion.GetAsciiText(ParseFile.ParseSimpleOffset(isoStream, this.DiscBaseOffset + 0x11, 3));

            this.DiscBaseOffset = offset;
            this.DecryptedAreaOffset = (ulong)this.DiscBaseOffset + WIIU_DECRYPTED_AREA_OFFSET;
            this.DiscReader = new NintendoWiiUEncryptedDiscReader();
            
            this.IsRawDump = isRawDump;
                        
            // initialize partition info
            this.InitializePartions(isoStream);
                        
            // initialize volumes
            this.VolumeArrayList = new ArrayList();
            this.LoadVolumes(isoStream);

        }

        private void InitializePartions(FileStream isoStream)
        {            
            // read and decrypt partition toc
            byte[] PartitionTocBlock = this.DiscReader.GetBytes(isoStream, this.DecryptedAreaOffset, 0x8000, this.DiscKey);

            // verify DiscKey before proceeding
            if (!ParseFile.CompareSegmentUsingSourceOffset(PartitionTocBlock, 0, 4, DECRYPTED_AREA_SIGNATURE))
            {
                throw new FormatException("Partition TOC signature bytes not found.");
            }

            // initialize partitions TOC
            this.InitializePartitionToc(isoStream, PartitionTocBlock);
           
            // parse file table entries (FST block)
            this.ParsePartitionFileTables(isoStream);
        }

        private void InitializePartitionToc(FileStream isoStream, byte[] DecryptedPartitionToc)
        {
            byte[] temp = new byte[4];
    
            // get partition count from decrypted TOC
            this.PartitionCount = ParseFile.ReadUintBE(DecryptedPartitionToc, 0x1C);
        
            // initialize partition array
            this.Partitions = new NintendoWiiUPartition[this.PartitionCount];
        
            // populate partition information from decrypted TOC
            for (uint i = 0; i < this.PartitionCount; i++)
            {
                this.Partitions[i] = new NintendoWiiUPartition();

                // copy partition identifier bytes (SI, UP, GM) from decrypted TOC
                this.Partitions[i].PartitionIdentifier = ParseFile.SimpleArrayCopy(DecryptedPartitionToc, (PARTITION_TOC_OFFSET + (i * PARTITION_TOC_ENTRY_SIZE)), 0x19);

                // convert partition identifier to string for display
                this.Partitions[i].PartitionName = ByteConversion.GetAsciiText(DecryptedPartitionToc, (PARTITION_TOC_OFFSET + (i * PARTITION_TOC_ENTRY_SIZE)));

                // calculate partition offset (relative from WIIU_DECRYPTED_AREA_OFFSET) from decrypted TOC
                temp = ParseFile.SimpleArrayCopy(DecryptedPartitionToc, (PARTITION_TOC_OFFSET + (i * PARTITION_TOC_ENTRY_SIZE) + 0x20), 4);
                this.Partitions[i].PartitionOffset = (ulong)(((ulong)ByteConversion.GetUInt32BigEndian(temp) * 0x8000) - 0x10000);
            }
        }

        private void ParsePartitionFileTables(FileStream isoStream)
        {
            byte[] rawPartitionEntry;
            byte[] fileTableBlock;

            uint entriesOffset;
            uint nameTableOffset;
            uint totalEntries;

            ArrayList partitionEntries;
            NintendoWiiUPartitionEntry partitionEntry;

            uint clusterStart;
            uint clusterSize;

            uint currentFileTableSize;
            uint currentEntryOffset;
            uint currentNameOffset;

            // loop through each partition
            for (uint i = 0; i < this.Partitions.Length; i++)
            {
                partitionEntries = new ArrayList();

                // decrypt file table block 
                if (this.Partitions[i].PartitionName.StartsWith("SI") ||
                    this.Partitions[i].PartitionName.StartsWith("UP") ||
                    this.TitleKeyHash.ContainsKey(this.Partitions[i].PartitionName))
                {
                    if (this.Partitions[i].PartitionName.StartsWith("SI"))
                    {
                        this.Partitions[i].PartitionKey = this.DiscKey;
                        this.Partitions[i].IV = new byte[0x10];
                    }
                    else if (this.Partitions[i].PartitionName.StartsWith("UP"))
                    {
                        this.Partitions[i].PartitionKey = this.DiscKey;
                        this.Partitions[i].IV = new byte[0x10];
                    }
                    else
                    { 
                        this.Partitions[i].PartitionKey = this.TitleKeyHash[this.Partitions[i].PartitionName].DecryptedTitleKey;
                        this.Partitions[i].IV = this.TitleKeyHash[this.Partitions[i].PartitionName].IV;
                    }

                    // set current file table size, increase later if needed
                    currentFileTableSize = 0x8000;
                    
                    // decrypt file table block
                    fileTableBlock = this.DiscReader.GetBytes(isoStream,
                        this.DecryptedAreaOffset + this.Partitions[i].PartitionOffset, currentFileTableSize, this.Partitions[i].PartitionKey);

                    // verify FST magic bytes
                    if (!ParseFile.CompareSegmentUsingSourceOffset(fileTableBlock, 0, 4, PARTITION_FILE_TABLE_SIGNATURE))
                    {
                        throw new FormatException(String.Format("File Table Signature not found for partition: {0}.", this.Partitions[i].PartitionName));
                    }

                    // parse cluster entries
                    this.Partitions[i].PartitionClusterCount = ParseFile.ReadUintBE(fileTableBlock, 8);
                    this.Partitions[i].PartitionClusters = new NintendoWiiUClusterStruct[this.Partitions[i].PartitionClusterCount];

                    for (uint c = 0; c < this.Partitions[i].PartitionClusterCount; c++)
                    {
                        this.Partitions[i].PartitionClusters[c] = new NintendoWiiUClusterStruct();
                        
                        // get cluster start offset
                        clusterStart = ParseFile.ReadUintBE(fileTableBlock, (0x20 + (0x20 * c))) * 0x8000;

                        if (clusterStart > 0) 
                        {
                            this.Partitions[i].PartitionClusters[c].StartOffset = clusterStart - 0x8000; // subtract 0x8000 since seems to use 1-based array
                        }

                        // calculate cluster end offset based on number of blocks value
                        clusterSize = ParseFile.ReadUintBE(fileTableBlock, (0x20 + (0x20 * c) + 4)) * 0x8000;
                        
                        // @TODO: This seems to leave some empty space between clusters.
                        if (clusterSize > 0)
                        {
                            this.Partitions[i].PartitionClusters[c].EndOffset = this.Partitions[i].PartitionClusters[c].StartOffset + clusterSize;
                        }
                    }

                    // goto directory/file records
                    entriesOffset = (ParseFile.ReadUintBE(fileTableBlock, 4) * ParseFile.ReadUintBE(fileTableBlock, 8)) + 0x20;

                    // read first row for root directory and to get record count
                    rawPartitionEntry = ParseFile.SimpleArrayCopy(fileTableBlock, entriesOffset, 0x10);
                    partitionEntry = new NintendoWiiUPartitionEntry(rawPartitionEntry);

                    // save total number of entries
                    totalEntries = partitionEntry.LastRowInDirectory;
                    nameTableOffset = entriesOffset + (totalEntries * 0x10);

                    // get root directory Name (typically null for root dir)
                    partitionEntry.EntryName = ByteConversion.GetAsciiText(fileTableBlock, nameTableOffset + partitionEntry.NameOffset);

                    // add root directory to array list
                    partitionEntries.Add(partitionEntry);

                    // loop through remaining records                
                    for (uint j = 1; j < totalEntries; j++)
                    {
                        // verify we have the full table
                        currentEntryOffset = entriesOffset + (j * 0x10);

                        // increase file table size if needed
                        if ((currentEntryOffset + 0x10) > currentFileTableSize)
                        {
                            currentFileTableSize += 0x8000;
                            fileTableBlock = this.DiscReader.GetBytes(isoStream,
                                this.DecryptedAreaOffset + this.Partitions[i].PartitionOffset, currentFileTableSize, this.Partitions[i].PartitionKey);
                        }

                        // parse partition entry
                        rawPartitionEntry = ParseFile.SimpleArrayCopy(fileTableBlock, currentEntryOffset, 0x10);
                        partitionEntry = new NintendoWiiUPartitionEntry(rawPartitionEntry);

                        // get entry name from file table
                        currentNameOffset = nameTableOffset + partitionEntry.NameOffset;

                        // increase size if needed, will be conservative with name
                        if ((currentNameOffset + 0x200) > currentFileTableSize)
                        {
                            currentFileTableSize += 0x8000;
                            fileTableBlock = this.DiscReader.GetBytes(isoStream,
                                this.DecryptedAreaOffset + this.Partitions[i].PartitionOffset, currentFileTableSize, this.Partitions[i].PartitionKey);
                        }

                        partitionEntry.EntryName = ByteConversion.GetAsciiText(fileTableBlock, currentNameOffset);
                        
                        partitionEntries.Add(partitionEntry);

                    } // for (uint j = 1; j < totalEntries; j++)

                    this.Partitions[i].PartitionEntries = (NintendoWiiUPartitionEntry[])partitionEntries.ToArray(typeof(NintendoWiiUPartitionEntry));

                    // get title keys
                    if (this.Partitions[i].PartitionName.StartsWith("SI"))
                    {
                        this.AddTitleKeysToHashFromPartition(isoStream, this.Partitions[i]);
                    }
                } // if (this.Partitions[i].PartitionName.StartsWith("SI")...
            } // for (uint i = 0; i < this.Partitions.GetLength(0); i++)
        }

        private void AddTitleKeysToHashFromPartition(FileStream isoStream, NintendoWiiUPartition partition)
        {
            NintendoWiiUTitleKeyStruct titleKeyInfo = new NintendoWiiUTitleKeyStruct();
            byte[] titleId;
            byte[] encryptedTitleKey;
            byte[] decryptedTitleKey;
            string partitionName;

            byte[] iv = new byte[0x10];

            // loop through partitions looking for "title.tik"
            for (uint i = 0; i < partition.PartitionEntries.Length; i++)
            {
                // open file
                if (!partition.PartitionEntries[i].IsDirectory &&
                    partition.PartitionEntries[i].EntryName.Equals(TITLE_TICKET_FILE, StringComparison.InvariantCultureIgnoreCase))
                {
                    // extract encrypted title key
                    encryptedTitleKey = this.DiscReader.GetBytes(isoStream, partition.PartitionName, (long)partition.PartitionOffset,
                        (long)partition.PartitionClusters[partition.PartitionEntries[i].StartingCluster].StartOffset, partition.PartitionEntries[i].OffsetWithinCluster + 0x1BF,
                        0x10, partition.PartitionKey);
                    
                    // get title id
                    titleId = this.DiscReader.GetBytes(isoStream, partition.PartitionName, (long)partition.PartitionOffset,
                        (long)partition.PartitionClusters[partition.PartitionEntries[i].StartingCluster].StartOffset, partition.PartitionEntries[i].OffsetWithinCluster + 0x1DC,
                        8, partition.PartitionKey);
                    
                    // decrypt title key
                    Array.Copy(titleId, 0, iv, 0, 8);
                    decryptedTitleKey = AESEngine.Decrypt(encryptedTitleKey, this.CommonKey, iv, CipherMode.CBC, PaddingMode.Zeros);

                    // build partition name for this key
                    partitionName = "GM" + BitConverter.ToString(titleId).Replace("-", String.Empty) + "000000000000";

                    // add to dctionary
                    titleKeyInfo.EncryptedTitleKey = encryptedTitleKey;
                    titleKeyInfo.DecryptedTitleKey = decryptedTitleKey;
                    titleKeyInfo.IV = iv;
                    titleKeyInfo.PartitionName = partitionName;

                    if (!this.TitleKeyHash.ContainsKey(partitionName))
                    {
                        this.TitleKeyHash.Add(partitionName, titleKeyInfo);
                    }
                }

            }
        }

        public void LoadVolumes(FileStream isoStream)
        {
            NintendoWiiUOpticalDiscVolume newVolume;
            for (uint i = 0; i < this.PartitionCount; i++)
            {
                if (this.Partitions[i].PartitionKey != null)
                {                   
                    newVolume = new NintendoWiiUOpticalDiscVolume(this.Partitions[i], this.DiscReader);
                    newVolume.Initialize(isoStream, (long)this.Partitions[i].PartitionOffset, this.IsRawDump);
                    this.VolumeArrayList.Add(newVolume);
                }
            }
        }
    }

    public class NintendoWiiUPartition
    {
        public ulong PartitionOffset { set; get; }
        public byte[] PartitionIdentifier { set; get; }
        public string PartitionName { set; get; }

        public byte[] PartitionKey { set; get; }
        public byte[] IV { set; get; }

        public uint PartitionClusterCount { set; get; }
        public NintendoWiiUClusterStruct[] PartitionClusters { set; get; }

        public NintendoWiiUPartitionEntry[] PartitionEntries { set; get; }


        /*
        public byte[] TitleId { set; get; }
        public byte[] EncryptedTitleKey { set; get; }
        public byte[] DecryptedTitleKey { set; get; }
        public byte CommonKeyIndex { set; get; }

        public long RelativeDataOffset { set; get; }
        public long DataSize { set; get; }
        */
    }

    public class NintendoWiiUPartitionEntry
    {
        // 0x00
        public bool IsDirectory { set; get; }  // 0x00
        public uint NameOffset { set; get; }   // 0x01 - 0x03
        public string EntryName { set; get; }

        // 0x04
        public uint OffsetWithinCluster { set; get; }

        // 0x08
        public uint LastRowInDirectory { set; get; } // Only if IsDirectory is True
        public uint Size { set; get; }               // Only if IsDirectory is False

        // 0x0C
        public ushort Unknown { set; get; }          // 0x0C
        public ushort StartingCluster { set; get; }  // 0x0E

        public NintendoWiiUPartitionEntry(byte[] RawPartitionEntry)
        {
            // check if this is a directory entry
            if (RawPartitionEntry[0] == 1)
            {
                this.IsDirectory = true;
                this.LastRowInDirectory = ParseFile.ReadUintBE(RawPartitionEntry, 8);
            }
            else
            {
                this.IsDirectory = false;
                this.Size = ParseFile.ReadUintBE(RawPartitionEntry, 8);
            }

            this.NameOffset = ParseFile.ReadUintBE(RawPartitionEntry, 0) & 0x00FFFFFF;

            this.OffsetWithinCluster = ParseFile.ReadUintBE(RawPartitionEntry, 4);
            this.OffsetWithinCluster <<= 5;

            this.Unknown = ParseFile.ReadUshortBE(RawPartitionEntry, 0x0C);
            this.StartingCluster = ParseFile.ReadUshortBE(RawPartitionEntry, 0x0E);
        }
    }

    public struct NintendoWiiUTitleKeyStruct
    {
        public byte[] EncryptedTitleKey { set; get; }
        public byte[] DecryptedTitleKey { set; get; }
        public byte[] IV { set; get; }
        public string PartitionName { set; get; }
    }

    public struct NintendoWiiUClusterStruct
    {
        public ulong StartOffset { set; get; }
        public ulong EndOffset { set; get; }
    }


    public class NintendoWiiUOpticalDiscVolume : IVolume
    {
        public NintendoWiiUPartition SourcePartition { set; get; }

        public string VolumeIdentifier { set; get; }
        public string FormatDescription { set; get; }
        public VolumeDataType VolumeType { set; get; }

        public long VolumeBaseOffset { set; get; }
        public long DataOffset { set; get; }        

        public bool IsRawDump { set; get; }
        public NintendoWiiUEncryptedDiscReader DiscReader { set; get; }

        public ArrayList DirectoryStructureArray { set; get; }
        public IDirectoryStructure[] Directories
        {
            set { Directories = value; }
            get
            {
                DirectoryStructureArray.Sort();
                return (IDirectoryStructure[])DirectoryStructureArray.ToArray(typeof(NintendoWiiUOpticalDiscDirectoryStructure));
            }
        }

        public ulong RootDirectoryOffset { set; get; }
        public DateTime ImageCreationTime { set; get; }

        public ulong NameTableOffset { set; get; }

        public NintendoWiiUOpticalDiscVolume(NintendoWiiUPartition partition, 
            NintendoWiiUEncryptedDiscReader discReader)
        {
            this.SourcePartition = partition;            
            this.DiscReader = discReader;
                        
            //this.DiscReader.CurrentDecryptedBlockNumber = -1;
            //this.DiscReader.CurrentDecryptedBlock = null;
        }

        public int CompareTo(object obj)
        {
            if (obj is NintendoWiiUOpticalDiscVolume)
            {
                NintendoWiiUOpticalDiscVolume o = (NintendoWiiUOpticalDiscVolume)obj;

                return this.VolumeIdentifier.CompareTo(o.VolumeIdentifier);
            }

            throw new ArgumentException("object is not an NintendoWiiUOpticalDiscVolume");
        }

        public void Initialize(FileStream isoStream, long offset, bool isRawDump)
        {
            this.VolumeBaseOffset = (long)this.SourcePartition.PartitionOffset;
            this.VolumeIdentifier = this.SourcePartition.PartitionName;

            this.IsRawDump = isRawDump;
            this.DirectoryStructureArray = new ArrayList();

            this.FormatDescription = NintendoWiiUOpticalDisc.FORMAT_DESCRIPTION_STRING;
            this.VolumeType = VolumeDataType.Data;
                   
            this.LoadDirectories(isoStream);
        }

        public void ExtractAll(ref Dictionary<string, FileStream> streamCache, string destinationFolder, bool extractAsRaw)
        {
            foreach (NintendoWiiUOpticalDiscDirectoryStructure ds in this.DirectoryStructureArray)
            {
                ds.Extract(ref streamCache, Path.Combine(destinationFolder, this.VolumeIdentifier), extractAsRaw);
            }
        }

        public void LoadDirectories(FileStream isoStream)
        {
            NintendoWiiUOpticalDiscDirectoryStructure rootDirectory = null;
            uint currentPartitionEntry = 0;
            
            rootDirectory = new NintendoWiiUOpticalDiscDirectoryStructure(
                isoStream, isoStream.Name, this.SourcePartition, ref currentPartitionEntry,
                new DateTime(), String.Empty, this.DiscReader);

            this.DirectoryStructureArray.Add(rootDirectory);
        }
    }

    public class NintendoWiiUOpticalDiscFileStructure : IFileStructure
    {
        public string ParentDirectoryName { set; get; }
        public string SourceFilePath { set; get; }
        public string FileName { set; get; }

        public long VolumeBaseOffset { set; get; }
        public long DataSectionOffset { set; get; }
        public long Lba { set; get; }
        public long Size { set; get; }
        public bool IsRaw { set; get; }
        public CdSectorType FileMode { set; get; }
        public int NonRawSectorSize { set; get; }

        public NintendoWiiUEncryptedDiscReader DiscReader { set; get; }
        public NintendoWiiUPartition ParentPartition { set; get; }
        public uint PartitionEntryId { set; get; }

        //public string PartitionIdentifier { set; get; }
        //public byte[] PartitionKey { set; get; }
        //public byte[] IV { set; get; }

        public DateTime FileDateTime { set; get; }

        public int CompareTo(object obj)
        {
            if (obj is NintendoWiiUOpticalDiscFileStructure)
            {
                NintendoWiiUOpticalDiscFileStructure o = (NintendoWiiUOpticalDiscFileStructure)obj;

                return this.FileName.CompareTo(o.FileName);
            }

            throw new ArgumentException("object is not an NintendoWiiUOpticalDiscFileStructure");
        }

        public NintendoWiiUOpticalDiscFileStructure(string parentDirectoryName,
            string sourceFilePath, string fileName, long volumeBaseOffset,
            long dataSectionOffset, long lba, long size, DateTime fileTime,
            NintendoWiiUEncryptedDiscReader discReader, NintendoWiiUPartition parentPartition, 
            uint partitionEntryId)
        {
            this.ParentDirectoryName = parentDirectoryName;
            this.SourceFilePath = sourceFilePath;
            this.FileName = fileName;
            this.VolumeBaseOffset = volumeBaseOffset;
            this.DataSectionOffset = dataSectionOffset;
            this.Lba = lba;
            this.IsRaw = false;
            this.NonRawSectorSize = -1;
            this.Size = size;
            this.FileMode = CdSectorType.Unknown;
            this.FileDateTime = fileTime;
            this.DiscReader = discReader;
            this.ParentPartition = parentPartition;
            this.PartitionEntryId = partitionEntryId;
        }

        public virtual void Extract(ref Dictionary<string, FileStream> streamCache, string destinationFolder, bool extractAsRaw)
        {
            string destinationFile = Path.Combine(Path.Combine(destinationFolder, this.ParentDirectoryName), this.FileName);

            if (!streamCache.ContainsKey(this.SourceFilePath))
            {
                streamCache[this.SourceFilePath] = File.OpenRead(this.SourceFilePath);
            }

            // setup initial IV
            byte[] firstIV = new byte[0x10];
            byte[] clusterId = BitConverter.GetBytes(this.ParentPartition.PartitionEntries[this.PartitionEntryId].StartingCluster);
            firstIV[0] = clusterId[1];
            firstIV[1] = clusterId[0];

            this.DiscReader.ExtractFile(streamCache[this.SourceFilePath], destinationFile, this.ParentPartition.PartitionName,
                this.VolumeBaseOffset, this.DataSectionOffset, this.Lba, this.Size, this.ParentPartition.PartitionKey,
                firstIV);
        }
    }

    public class NintendoWiiUOpticalDiscDirectoryStructure : IDirectoryStructure
    {
        public long DirectoryRecordOffset { set; get; }
        public string ParentDirectoryName { set; get; }

        public ArrayList SubDirectoryArray { set; get; }
        public IDirectoryStructure[] SubDirectories
        {
            set { this.SubDirectories = value; }
            get
            {
                SubDirectoryArray.Sort();
                return (IDirectoryStructure[])SubDirectoryArray.ToArray(typeof(NintendoWiiUOpticalDiscDirectoryStructure));
            }
        }

        public ArrayList FileArray { set; get; }
        public IFileStructure[] Files
        {
            set { this.Files = value; }
            get
            {
                FileArray.Sort();
                return (IFileStructure[])FileArray.ToArray(typeof(NintendoWiiUOpticalDiscFileStructure));
            }
        }

        public string SourceFilePath { set; get; }
        public string DirectoryName { set; get; }

        public int CompareTo(object obj)
        {
            if (obj is NintendoWiiUOpticalDiscDirectoryStructure)
            {
                NintendoWiiUOpticalDiscDirectoryStructure o = (NintendoWiiUOpticalDiscDirectoryStructure)obj;

                return this.DirectoryName.CompareTo(o.DirectoryName);
            }

            throw new ArgumentException("object is not an NintendoWiiUOpticalDiscDirectoryStructure");
        }

        public void Extract(ref Dictionary<string, FileStream> streamCache, string destinationFolder, bool extractAsRaw)
        {
            string fullDirectoryPath = Path.Combine(destinationFolder, Path.Combine(this.ParentDirectoryName, this.DirectoryName));

            // create directory
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            foreach (NintendoWiiUOpticalDiscFileStructure f in this.FileArray)
            {
                f.Extract(ref streamCache, destinationFolder, extractAsRaw);
            }

            foreach (NintendoWiiUOpticalDiscDirectoryStructure d in this.SubDirectoryArray)
            {
                d.Extract(ref streamCache, destinationFolder, extractAsRaw);
            }
        }

        public NintendoWiiUOpticalDiscDirectoryStructure(
            FileStream isoStream,
            string sourceFilePath,
            NintendoWiiUPartition volumePartition,
            ref uint currentPartitionEntryIndex,
            DateTime creationDateTime,
            string parentDirectory,
            NintendoWiiUEncryptedDiscReader discReader)
        {
            NintendoWiiUOpticalDiscDirectoryStructure subdirectory;
            NintendoWiiUOpticalDiscFileStructure file;
            string nextDirectory;
            uint lastPartitionRowInDirectory;

            this.SourceFilePath = sourceFilePath;
            this.SubDirectoryArray = new ArrayList();
            this.FileArray = new ArrayList();

            if (String.IsNullOrEmpty(parentDirectory))
            {
                this.ParentDirectoryName = volumePartition.PartitionName;
            }
            else
            {
                this.ParentDirectoryName = parentDirectory;
            }

            this.DirectoryName = volumePartition.PartitionEntries[currentPartitionEntryIndex].EntryName;
            nextDirectory = this.ParentDirectoryName + Path.DirectorySeparatorChar + this.DirectoryName;

            // add subdirectories and files
            lastPartitionRowInDirectory = volumePartition.PartitionEntries[currentPartitionEntryIndex].LastRowInDirectory;

            for (++currentPartitionEntryIndex; currentPartitionEntryIndex < lastPartitionRowInDirectory; currentPartitionEntryIndex++)
            {
                // build subdirectory
                if (volumePartition.PartitionEntries[currentPartitionEntryIndex].IsDirectory)
                {
                    subdirectory = new NintendoWiiUOpticalDiscDirectoryStructure(isoStream, sourceFilePath,
                        volumePartition, ref currentPartitionEntryIndex, creationDateTime, nextDirectory, discReader);

                    this.SubDirectoryArray.Add(subdirectory);

                    // decrement to avoid double increment due to recursion, 
                    //   probably a more elegant way to handle, while loop or something...
                    currentPartitionEntryIndex--;
                }
                else  // build file
                {
                    file = new NintendoWiiUOpticalDiscFileStructure(nextDirectory,
                        isoStream.Name, volumePartition.PartitionEntries[currentPartitionEntryIndex].EntryName,
                        (long)volumePartition.PartitionOffset, 
                        (long)volumePartition.PartitionClusters[volumePartition.PartitionEntries[currentPartitionEntryIndex].StartingCluster].StartOffset,
                        volumePartition.PartitionEntries[currentPartitionEntryIndex].OffsetWithinCluster, volumePartition.PartitionEntries[currentPartitionEntryIndex].Size,
                        creationDateTime, discReader, volumePartition, currentPartitionEntryIndex);

                    this.FileArray.Add(file);
                }

            }
        }
    }
    
    public class NintendoWiiUEncryptedDiscReader
    {
        //public string CurrentVolumeIdentifier { set; get; }
        //public long CurrentDecryptedClusterOffset { set; get; }
        //public byte[] CurrentDecryptedBlock { set; get; }
        //public long CurrentDecryptedBlockNumber { set; get; }
        public Rijndael Algorithm { set; get; }

        public NintendoWiiUEncryptedDiscReader()
        {
            //this.CurrentVolumeIdentifier = String.Empty;
            //this.CurrentDecryptedClusterOffset = -1;
            //this.CurrentDecryptedBlock = null;
            //this.CurrentDecryptedBlockNumber = -1;
            this.Algorithm = Rijndael.Create();
        }

        public byte[] GetBytes(FileStream isoStream, ulong offset, ulong size, byte[] key)
        {
            byte[] encryptedChunk = new byte[size];
            byte[] decryptedChunk = new byte[size];
            byte[] IV = new byte[0x10];

            encryptedChunk = ParseFile.ParseSimpleOffset(isoStream, (long)offset, (int)size);
            decryptedChunk = AESEngine.Decrypt(this.Algorithm, encryptedChunk, key, IV, CipherMode.CBC, PaddingMode.Zeros);

            return decryptedChunk;
        }
        
        public byte[] GetBytes(FileStream isoStream, string volumeIdentifier, long volumeOffset,
                long clusterOffset, long fileOffset, long size, byte[] partitionKey)
        {
            byte[] value = new byte[size];
                        
            byte[] encryptedPartitionCluster;
            byte[] decryptedPartitionCluster;
            byte[] clusterIV;

            long bufferLocation = 0;
            long maxCopySize;
            long copySize;

            while (size > 0)
            {
                // get block offset info
                WiiUBlockStructure blockStructure = NintendoWiiUOpticalDisc.GetWiiUBlockStructureForOffset(fileOffset);

                // if ((!this.CurrentVolumeIdentifier.Equals(volumeIdentifier)) ||
                //    (this.CurrentVolumeIdentifier.Equals(volumeIdentifier)) && 
                //    (this.CurrentDecryptedBlockNumber != blockStructure.BlockNumber) &&
                //    (this.CurrentDecryptedClusterOffset != clusterOffset))
                //{

                // read encrypted block
                encryptedPartitionCluster = ParseFile.ParseSimpleOffset(isoStream,
                        (long)NintendoWiiUOpticalDisc.WIIU_DECRYPTED_AREA_OFFSET + volumeOffset + clusterOffset + (blockStructure.BlockNumber * 0x8000),
                        0x8000);

                // descrypt the block
                clusterIV = new byte[0x10];
    
                decryptedPartitionCluster = AESEngine.Decrypt(this.Algorithm, encryptedPartitionCluster,
                                partitionKey, clusterIV, CipherMode.CBC, PaddingMode.Zeros);


                //this.CurrentDecryptedBlock = decryptedPartitionCluster;
                //this.CurrentDecryptedClusterOffset = clusterOffset;
                //this.CurrentDecryptedBlockNumber = blockStructure.BlockNumber;
                //this.CurrentVolumeIdentifier = volumeIdentifier;
                //}

                // copy the decrypted data              
                maxCopySize = 0x8000 - blockStructure.BlockOffset;
                copySize = (size > maxCopySize) ? maxCopySize : size;
                //Array.Copy(this.CurrentDecryptedBlock, blockStructure.BlockOffset, value, bufferLocation, copySize);
                Array.Copy(decryptedPartitionCluster, blockStructure.BlockOffset, value, bufferLocation, copySize);


                // update counters
                size -= copySize;
                bufferLocation += copySize;
                fileOffset += copySize;
            }
            
            return value;
        }

        public void ExtractFile(FileStream isoStream, string destinationPath, string volumeIdentifier, 
            long volumeOffset, long clusterOffset, long fileOffset, long size, byte[] partitionKey,
            byte[] initialIV)
        {
            
            byte[] value = new byte[0x8000];
            byte[] encryptedPartitionCluster;
            byte[] decryptedPartitionCluster;
            
            byte[] clusterIV = initialIV;
            byte[] cipherKey = partitionKey;

            long maxCopySize;
            long copySize;

            // create destination
            string destintionDirectory = Path.GetDirectoryName(destinationPath);

            if (!Directory.Exists(destintionDirectory))
            {
                Directory.CreateDirectory(destintionDirectory);
            }

            using (FileStream outStream = File.Open(destinationPath, FileMode.Create, FileAccess.Write))
            {
                while (size > 0)
                {
                    // get block offset info
                    WiiUBlockStructure blockStructure = NintendoWiiUOpticalDisc.GetWiiUBlockStructureForOffset(fileOffset);

                    //if ((!this.CurrentVolumeIdentifier.Equals(volumeIdentifier)) ||
                    //    (this.CurrentVolumeIdentifier.Equals(volumeIdentifier)) && 
                    //    (this.CurrentDecryptedBlockNumber != blockStructure.BlockNumber) &&
                    //    (this.CurrentDecryptedClusterOffset != clusterOffset))
                    //{

                    // read encrypted block
                    encryptedPartitionCluster = ParseFile.ParseSimpleOffset(isoStream,
                            (long)NintendoWiiUOpticalDisc.WIIU_DECRYPTED_AREA_OFFSET + volumeOffset + clusterOffset + (blockStructure.BlockNumber * 0x8000),
                            0x8000);

                    // decrypt the block
                    decryptedPartitionCluster = AESEngine.Decrypt(this.Algorithm, encryptedPartitionCluster,
                                        cipherKey, ref clusterIV, CipherMode.CBC, PaddingMode.Zeros);
                        

                    //this.CurrentDecryptedBlock = decryptedPartitionCluster;
                    //this.CurrentDecryptedClusterOffset = clusterOffset;
                    //this.CurrentDecryptedBlockNumber = blockStructure.BlockNumber;
                    //this.CurrentVolumeIdentifier = volumeIdentifier;
                    
                    //}

                    // copy the encrypted data
                    maxCopySize = 0x8000 - blockStructure.BlockOffset;
                    copySize = (size > maxCopySize) ? maxCopySize : size;
                    //outStream.Write(this.CurrentDecryptedBlock, (int)blockStructure.BlockOffset, (int)copySize);
                    outStream.Write(decryptedPartitionCluster, (int)blockStructure.BlockOffset, (int)copySize);

                    // update counters
                    size -= copySize;
                    fileOffset += copySize;
                }
              
            } 
        }
    }

    public struct WiiUBlockStructure
    {
        public long BlockNumber { set; get; }
        public long BlockOffset { set; get; }
    }
}