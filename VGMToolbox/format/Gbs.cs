﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Checksums;

using VGMToolbox.util;
using VGMToolbox.util.ObjectPooling;

namespace VGMToolbox.format
{
    class Gbs : IFormat
    {
        private static readonly byte[] ASCII_SIGNATURE = new byte[] { 0x47, 0x42, 0x53 }; // GBS
        private const string FORMAT_ABBREVIATION = "GBS";

        private const int SIG_OFFSET = 0x00;
        private const int SIG_LENGTH = 0x03;

        private const int VERSION_OFFSET = 0x03;
        private const int VERSION_LENGTH = 0x01;

        private const int TOTAL_SONGS_OFFSET = 0x04;
        private const int TOTAL_SONGS_LENGTH = 0x01;

        private const int STARTING_SONG_OFFSET = 0x05;
        private const int STARTING_SONG_LENGTH = 0x01;

        private const int LOAD_ADDRESS_OFFSET = 0x06;
        private const int LOAD_ADDRESS_LENGTH = 0x02;

        private const int INIT_ADDRESS_OFFSET = 0x08;
        private const int INIT_ADDRESS_LENGTH = 0x02;

        private const int PLAY_ADDRESS_OFFSET = 0x0A;
        private const int PLAY_ADDRESS_LENGTH = 0x02;

        private const int STACK_POINTER_OFFSET = 0x0C;
        private const int STACK_POINTER_LENGTH = 0x02;

        private const int TIMER_MODULO_OFFSET = 0x0E;
        private const int TIMER_MODULO_LENGTH = 0x01;

        private const int TIMER_CONTROL_OFFSET = 0x0F;
        private const int TIMER_CONTROL_LENGTH = 0x01;

        private const int NAME_OFFSET = 0x10;
        private const int NAME_LENGTH = 0x20;

        private const int ARTIST_OFFSET = 0x30;
        private const int ARTIST_LENGTH = 0x20;

        private const int COPYRIGHT_OFFSET = 0x50;
        private const int COPYRIGHT_LENGTH = 0x20;

        private const int DATA_OFFSET = 0x70;

        private byte[] asciiSignature;
        private byte[] versionNumber;
        private byte[] totalSongs;
        private byte[] startingSong;
        private byte[] loadAddress;
        private byte[] initAddress;
        private byte[] playAddress;
        private byte[] stackPointer;
        private byte[] timerModulo;
        private byte[] timerControl;
        private byte[] songName;
        private byte[] songArtist;
        private byte[] songCopyright;
        private byte[] data;

        Dictionary<string, string> tagHash = new Dictionary<string, string>();

        public byte[] AsciiSignature { get { return this.asciiSignature; } }
        public byte[] VersionNumber { get { return this.versionNumber; } }
        public byte[] TotalSongs { get { return this.totalSongs; } }
        public byte[] StartingSong { get { return this.startingSong; } }
        public byte[] LoadAddress { get { return this.loadAddress; } }
        public byte[] InitAddress { get { return this.initAddress; } }
        public byte[] PlayAddress { get { return this.playAddress; } }
        public byte[] StackPointer { get { return this.stackPointer; } }
        public byte[] TimerModulo { get { return this.timerModulo; } }
        public byte[] TimerControl { get { return this.timerControl; } }
        public byte[] SongName { get { return this.songName; } }
        public byte[] SongArtist { get { return this.songArtist; } }
        public byte[] SongCopyright { get { return this.songCopyright; } }
        public byte[] Data { get { return this.data; } }

        #region METHODS
        
        public byte[] getAsciiSignature(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, SIG_OFFSET, SIG_LENGTH);
        }

        public byte[] getVersionNumber(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, VERSION_OFFSET, VERSION_LENGTH);
        }

        public byte[] getTotalSongs(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, TOTAL_SONGS_OFFSET, TOTAL_SONGS_LENGTH);
        }

        public byte[] getStartingSong(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, STARTING_SONG_OFFSET, STARTING_SONG_LENGTH);
        }

        public byte[] getLoadAddress(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, LOAD_ADDRESS_OFFSET, LOAD_ADDRESS_LENGTH);
        }

        public byte[] getInitAddress(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, INIT_ADDRESS_OFFSET, INIT_ADDRESS_LENGTH);
        }

        public byte[] getPlayAddress(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, PLAY_ADDRESS_OFFSET, PLAY_ADDRESS_LENGTH);
        }

        public byte[] getStackPointer(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, STACK_POINTER_OFFSET, STACK_POINTER_LENGTH);
        }

        public byte[] getTimerModulo(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, TIMER_MODULO_OFFSET, TIMER_MODULO_LENGTH);
        }

        public byte[] getTimerControl(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, TIMER_CONTROL_OFFSET, TIMER_CONTROL_LENGTH);
        }
                                                        
        public byte[] getSongName(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, NAME_OFFSET, NAME_LENGTH);
        }

        public byte[] getSongArtist(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, ARTIST_OFFSET, ARTIST_LENGTH);
        }

        public byte[] getSongCopyright(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, COPYRIGHT_OFFSET, COPYRIGHT_LENGTH);
        }

        public byte[] getData(Stream pStream)
        {
            return ParseFile.parseSimpleOffset(pStream, DATA_OFFSET, (int)(pStream.Length - DATA_OFFSET));
        }

        public void Initialize(Stream pStream)
        {
            this.asciiSignature = this.getAsciiSignature(pStream);
            this.versionNumber = this.getVersionNumber(pStream);
            this.totalSongs = this.getTotalSongs(pStream);
            this.startingSong = this.getStartingSong(pStream);
            this.loadAddress = this.getLoadAddress(pStream);
            this.initAddress = this.getInitAddress(pStream);
            this.playAddress = this.getPlayAddress(pStream);            
            this.stackPointer = getStackPointer(pStream);            
            this.timerModulo = getTimerModulo(pStream);
            this.timerControl = getTimerControl(pStream);
            this.songName = this.getSongName(pStream);
            this.songArtist = this.getSongArtist(pStream);
            this.songCopyright = this.getSongCopyright(pStream);
            this.data = this.getData(pStream);

            this.initializeTagHash();
        }

        private void initializeTagHash()
        {
            System.Text.Encoding enc = System.Text.Encoding.ASCII;

            tagHash.Add("Name", enc.GetString(this.songName));
            tagHash.Add("Artist", enc.GetString(this.songArtist));
            tagHash.Add("Copyright", enc.GetString(this.songCopyright));
            tagHash.Add("Total Songs", this.totalSongs[0].ToString());
            tagHash.Add("Starting Song", this.startingSong[0].ToString());
        }

        public void GetDatFileCrc32(string pPath, ref Dictionary<string, ByteArray> pLibHash,
            ref Crc32 pChecksum, bool pUseLibHash)
        {
            pChecksum.Reset();

            pChecksum.Update(versionNumber);
            pChecksum.Update(totalSongs);
            pChecksum.Update(startingSong);
            pChecksum.Update(loadAddress);
            pChecksum.Update(initAddress);
            pChecksum.Update(playAddress);
            pChecksum.Update(stackPointer);
            pChecksum.Update(timerModulo);
            pChecksum.Update(timerControl);
            pChecksum.Update(data);
        }

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

        public bool IsFileLibrary(string pPath)
        {
            return false;
        }

        public Dictionary<string, string> GetTagHash()
        {
            return this.tagHash;
        }

        #endregion
    }
}
