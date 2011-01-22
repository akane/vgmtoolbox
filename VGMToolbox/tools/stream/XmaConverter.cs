﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3615
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 
namespace VGMToolbox.tools.stream
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

    public partial class XmaConverterSettings
    {

        private Header headerField;

        private bool useXmaParseField;

        private XmaParseParameters xmaParseParametersField;

        private bool addRiffHeaderField;

        private RiffParameters riffParametersField;

        private bool createPosFileField;

        private PosFileParameters posFileParametersField;

        private WavConversionParameters wavConversionParametersField;

        private string notesOrWarningsField;

        /// <remarks/>
        public Header Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public bool UseXmaParse
        {
            get
            {
                return this.useXmaParseField;
            }
            set
            {
                this.useXmaParseField = value;
            }
        }

        /// <remarks/>
        public XmaParseParameters XmaParseParameters
        {
            get
            {
                return this.xmaParseParametersField;
            }
            set
            {
                this.xmaParseParametersField = value;
            }
        }

        /// <remarks/>
        public bool AddRiffHeader
        {
            get
            {
                return this.addRiffHeaderField;
            }
            set
            {
                this.addRiffHeaderField = value;
            }
        }

        /// <remarks/>
        public RiffParameters RiffParameters
        {
            get
            {
                return this.riffParametersField;
            }
            set
            {
                this.riffParametersField = value;
            }
        }

        /// <remarks/>
        public bool CreatePosFile
        {
            get
            {
                return this.createPosFileField;
            }
            set
            {
                this.createPosFileField = value;
            }
        }

        /// <remarks/>
        public PosFileParameters PosFileParameters
        {
            get
            {
                return this.posFileParametersField;
            }
            set
            {
                this.posFileParametersField = value;
            }
        }

        /// <remarks/>
        public WavConversionParameters WavConversionParameters
        {
            get
            {
                return this.wavConversionParametersField;
            }
            set
            {
                this.wavConversionParametersField = value;
            }
        }

        /// <remarks/>
        public string NotesOrWarnings
        {
            get
            {
                return this.notesOrWarningsField;
            }
            set
            {
                this.notesOrWarningsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class WavConversionParameters
    {

        private bool useXmaEncodeField;

        private bool useToWavField;

        /// <remarks/>
        public bool UseXmaEncode
        {
            get
            {
                return this.useXmaEncodeField;
            }
            set
            {
                this.useXmaEncodeField = value;
            }
        }

        /// <remarks/>
        public bool UseToWav
        {
            get
            {
                return this.useToWavField;
            }
            set
            {
                this.useToWavField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PosFileParameters
    {

        private bool useStaticStartOffsetField;

        private bool useDynamicStartOffsetField;

        private string startOffsetStaticField;

        private string startOffsetOffsetField;

        private string startOffsetOffsetSizeField;

        private Endianness startOffsetOffsetEndianessField;

        private bool startOffsetOffsetEndianessFieldSpecified;

        private string startOffsetCalculationField;

        private bool useStaticStartOffset1Field;

        private bool useDynamicStartOffset1Field;

        private string startOffsetStatic1Field;

        private string startOffsetOffset1Field;

        private string startOffsetOffsetSize1Field;

        private Endianness startOffsetOffsetEndianess1Field;

        private bool startOffsetOffsetEndianess1FieldSpecified;

        private string endOffsetCalculationField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public bool UseStaticStartOffset
        {
            get
            {
                return this.useStaticStartOffsetField;
            }
            set
            {
                this.useStaticStartOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public bool UseDynamicStartOffset
        {
            get
            {
                return this.useDynamicStartOffsetField;
            }
            set
            {
                this.useDynamicStartOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string StartOffsetStatic
        {
            get
            {
                return this.startOffsetStaticField;
            }
            set
            {
                this.startOffsetStaticField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer", Order = 3)]
        public string StartOffsetOffset
        {
            get
            {
                return this.startOffsetOffsetField;
            }
            set
            {
                this.startOffsetOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer", Order = 4)]
        public string StartOffsetOffsetSize
        {
            get
            {
                return this.startOffsetOffsetSizeField;
            }
            set
            {
                this.startOffsetOffsetSizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public Endianness StartOffsetOffsetEndianess
        {
            get
            {
                return this.startOffsetOffsetEndianessField;
            }
            set
            {
                this.startOffsetOffsetEndianessField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartOffsetOffsetEndianessSpecified
        {
            get
            {
                return this.startOffsetOffsetEndianessFieldSpecified;
            }
            set
            {
                this.startOffsetOffsetEndianessFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public string StartOffsetCalculation
        {
            get
            {
                return this.startOffsetCalculationField;
            }
            set
            {
                this.startOffsetCalculationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UseStaticStartOffset", Order = 7)]
        public bool UseStaticStartOffset1
        {
            get
            {
                return this.useStaticStartOffset1Field;
            }
            set
            {
                this.useStaticStartOffset1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UseDynamicStartOffset", Order = 8)]
        public bool UseDynamicStartOffset1
        {
            get
            {
                return this.useDynamicStartOffset1Field;
            }
            set
            {
                this.useDynamicStartOffset1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StartOffsetStatic", Order = 9)]
        public string StartOffsetStatic1
        {
            get
            {
                return this.startOffsetStatic1Field;
            }
            set
            {
                this.startOffsetStatic1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StartOffsetOffset", DataType = "integer", Order = 10)]
        public string StartOffsetOffset1
        {
            get
            {
                return this.startOffsetOffset1Field;
            }
            set
            {
                this.startOffsetOffset1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StartOffsetOffsetSize", DataType = "integer", Order = 11)]
        public string StartOffsetOffsetSize1
        {
            get
            {
                return this.startOffsetOffsetSize1Field;
            }
            set
            {
                this.startOffsetOffsetSize1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StartOffsetOffsetEndianess", Order = 12)]
        public Endianness StartOffsetOffsetEndianess1
        {
            get
            {
                return this.startOffsetOffsetEndianess1Field;
            }
            set
            {
                this.startOffsetOffsetEndianess1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartOffsetOffsetEndianess1Specified
        {
            get
            {
                return this.startOffsetOffsetEndianess1FieldSpecified;
            }
            set
            {
                this.startOffsetOffsetEndianess1FieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 13)]
        public string EndOffsetCalculation
        {
            get
            {
                return this.endOffsetCalculationField;
            }
            set
            {
                this.endOffsetCalculationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    public enum Endianness
    {

        /// <remarks/>
        little,

        /// <remarks/>
        big,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RiffParameters
    {

        private bool useStaticFrequencyField;

        private bool getFrequencyFromRiffHeaderField;

        private string frequencyStaticField;

        private bool useStaticChannelsField;

        private bool getChannelsFromRiffHeaderField;

        private string channelStaticField;

        /// <remarks/>
        public bool UseStaticFrequency
        {
            get
            {
                return this.useStaticFrequencyField;
            }
            set
            {
                this.useStaticFrequencyField = value;
            }
        }

        /// <remarks/>
        public bool GetFrequencyFromRiffHeader
        {
            get
            {
                return this.getFrequencyFromRiffHeaderField;
            }
            set
            {
                this.getFrequencyFromRiffHeaderField = value;
            }
        }

        /// <remarks/>
        public string FrequencyStatic
        {
            get
            {
                return this.frequencyStaticField;
            }
            set
            {
                this.frequencyStaticField = value;
            }
        }

        /// <remarks/>
        public bool UseStaticChannels
        {
            get
            {
                return this.useStaticChannelsField;
            }
            set
            {
                this.useStaticChannelsField = value;
            }
        }

        /// <remarks/>
        public bool GetChannelsFromRiffHeader
        {
            get
            {
                return this.getChannelsFromRiffHeaderField;
            }
            set
            {
                this.getChannelsFromRiffHeaderField = value;
            }
        }

        /// <remarks/>
        public string ChannelStatic
        {
            get
            {
                return this.channelStaticField;
            }
            set
            {
                this.channelStaticField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class XmaParseParameters
    {

        private XmaType xmaTypeField;

        private bool xmaTypeFieldSpecified;

        private bool rebuildXmaField;

        private bool rebuildXmaFieldSpecified;

        private bool ignoreXmaParseErrorsField;

        private bool ignoreXmaParseErrorsFieldSpecified;

        private bool useStaticStartOffsetField;

        private bool useDynamicStartOffsetField;

        private bool setStartOffsetAfterRiffHeaderField;

        private string startOffsetStaticField;

        private string startOffsetOffsetField;

        private string startOffsetOffsetSizeField;

        private Endianness startOffsetOffsetEndianessField;

        private bool startOffsetOffsetEndianessFieldSpecified;

        private bool useStaticBlockSizeField;

        private bool useDynamicBlockSizeField;

        private string blockSizeStaticField;

        private string blockSizeOffsetField;

        private string blockSizeOffsetSizeField;

        private Endianness blockSizeOffsetEndianessField;

        private bool blockSizeOffsetEndianessFieldSpecified;

        private bool useStaticDataSizeField;

        private bool useDynamicDataSizeField;

        private bool getDataSizeFromRiffHeaderField;

        private string dataSizeStaticField;

        private string dataSizeOffsetField;

        private string dataSizeOffsetSizeField;

        private Endianness dataSizeOffsetEndianessField;

        private bool dataSizeOffsetEndianessFieldSpecified;

        /// <remarks/>
        public XmaType XmaType
        {
            get
            {
                return this.xmaTypeField;
            }
            set
            {
                this.xmaTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool XmaTypeSpecified
        {
            get
            {
                return this.xmaTypeFieldSpecified;
            }
            set
            {
                this.xmaTypeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool RebuildXma
        {
            get
            {
                return this.rebuildXmaField;
            }
            set
            {
                this.rebuildXmaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RebuildXmaSpecified
        {
            get
            {
                return this.rebuildXmaFieldSpecified;
            }
            set
            {
                this.rebuildXmaFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool IgnoreXmaParseErrors
        {
            get
            {
                return this.ignoreXmaParseErrorsField;
            }
            set
            {
                this.ignoreXmaParseErrorsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IgnoreXmaParseErrorsSpecified
        {
            get
            {
                return this.ignoreXmaParseErrorsFieldSpecified;
            }
            set
            {
                this.ignoreXmaParseErrorsFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool UseStaticStartOffset
        {
            get
            {
                return this.useStaticStartOffsetField;
            }
            set
            {
                this.useStaticStartOffsetField = value;
            }
        }

        /// <remarks/>
        public bool UseDynamicStartOffset
        {
            get
            {
                return this.useDynamicStartOffsetField;
            }
            set
            {
                this.useDynamicStartOffsetField = value;
            }
        }

        /// <remarks/>
        public bool SetStartOffsetAfterRiffHeader
        {
            get
            {
                return this.setStartOffsetAfterRiffHeaderField;
            }
            set
            {
                this.setStartOffsetAfterRiffHeaderField = value;
            }
        }

        /// <remarks/>
        public string StartOffsetStatic
        {
            get
            {
                return this.startOffsetStaticField;
            }
            set
            {
                this.startOffsetStaticField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string StartOffsetOffset
        {
            get
            {
                return this.startOffsetOffsetField;
            }
            set
            {
                this.startOffsetOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string StartOffsetOffsetSize
        {
            get
            {
                return this.startOffsetOffsetSizeField;
            }
            set
            {
                this.startOffsetOffsetSizeField = value;
            }
        }

        /// <remarks/>
        public Endianness StartOffsetOffsetEndianess
        {
            get
            {
                return this.startOffsetOffsetEndianessField;
            }
            set
            {
                this.startOffsetOffsetEndianessField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartOffsetOffsetEndianessSpecified
        {
            get
            {
                return this.startOffsetOffsetEndianessFieldSpecified;
            }
            set
            {
                this.startOffsetOffsetEndianessFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool UseStaticBlockSize
        {
            get
            {
                return this.useStaticBlockSizeField;
            }
            set
            {
                this.useStaticBlockSizeField = value;
            }
        }

        /// <remarks/>
        public bool UseDynamicBlockSize
        {
            get
            {
                return this.useDynamicBlockSizeField;
            }
            set
            {
                this.useDynamicBlockSizeField = value;
            }
        }

        /// <remarks/>
        public string BlockSizeStatic
        {
            get
            {
                return this.blockSizeStaticField;
            }
            set
            {
                this.blockSizeStaticField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string BlockSizeOffset
        {
            get
            {
                return this.blockSizeOffsetField;
            }
            set
            {
                this.blockSizeOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string BlockSizeOffsetSize
        {
            get
            {
                return this.blockSizeOffsetSizeField;
            }
            set
            {
                this.blockSizeOffsetSizeField = value;
            }
        }

        /// <remarks/>
        public Endianness BlockSizeOffsetEndianess
        {
            get
            {
                return this.blockSizeOffsetEndianessField;
            }
            set
            {
                this.blockSizeOffsetEndianessField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BlockSizeOffsetEndianessSpecified
        {
            get
            {
                return this.blockSizeOffsetEndianessFieldSpecified;
            }
            set
            {
                this.blockSizeOffsetEndianessFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool UseStaticDataSize
        {
            get
            {
                return this.useStaticDataSizeField;
            }
            set
            {
                this.useStaticDataSizeField = value;
            }
        }

        /// <remarks/>
        public bool UseDynamicDataSize
        {
            get
            {
                return this.useDynamicDataSizeField;
            }
            set
            {
                this.useDynamicDataSizeField = value;
            }
        }

        /// <remarks/>
        public bool GetDataSizeFromRiffHeader
        {
            get
            {
                return this.getDataSizeFromRiffHeaderField;
            }
            set
            {
                this.getDataSizeFromRiffHeaderField = value;
            }
        }

        /// <remarks/>
        public string DataSizeStatic
        {
            get
            {
                return this.dataSizeStaticField;
            }
            set
            {
                this.dataSizeStaticField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string DataSizeOffset
        {
            get
            {
                return this.dataSizeOffsetField;
            }
            set
            {
                this.dataSizeOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string DataSizeOffsetSize
        {
            get
            {
                return this.dataSizeOffsetSizeField;
            }
            set
            {
                this.dataSizeOffsetSizeField = value;
            }
        }

        /// <remarks/>
        public Endianness DataSizeOffsetEndianess
        {
            get
            {
                return this.dataSizeOffsetEndianessField;
            }
            set
            {
                this.dataSizeOffsetEndianessField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataSizeOffsetEndianessSpecified
        {
            get
            {
                return this.dataSizeOffsetEndianessFieldSpecified;
            }
            set
            {
                this.dataSizeOffsetEndianessFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    public enum XmaType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
    }
}