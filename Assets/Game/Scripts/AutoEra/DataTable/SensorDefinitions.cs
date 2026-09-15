//------------------------------------------------------------
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：__DATA_TABLE_CREATE_TIME__
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;
#if ENABLE_OBFUZ
[Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName | Obfuz.ObfuzScope.MethodName)]
#endif
namespace AutoEra.DataTable
{
/// <summary>
/// SensorDefinitions
/// </summary>
public class SensorDefinitions : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    /// Component row identifier
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// ComponentModelId
        /// </summary>
        public int ComponentModelId
        {
            get;
            private set;
        }

        /// <summary>
        /// Level
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        /// <summary>
        /// Kind
        /// </summary>
        public string Kind
        {
            get;
            private set;
        }

        /// <summary>
        /// IntervalMilliseconds
        /// </summary>
        public int IntervalMilliseconds
        {
            get;
            private set;
        }

        /// <summary>
        /// Range
        /// </summary>
        public float Range
        {
            get;
            private set;
        }

        /// <summary>
        /// ComputeCost
        /// </summary>
        public int ComputeCost
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            ComponentModelId = int.Parse(columnStrings[index++]);
            Level = int.Parse(columnStrings[index++]);
            Kind = columnStrings[index++];
            IntervalMilliseconds = int.Parse(columnStrings[index++]);
            Range = float.Parse(columnStrings[index++]);
            ComputeCost = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    ComponentModelId = binaryReader.Read7BitEncodedInt32();
                    Level = binaryReader.Read7BitEncodedInt32();
                    Kind = binaryReader.ReadString();
                    IntervalMilliseconds = binaryReader.Read7BitEncodedInt32();
                    Range = binaryReader.ReadSingle();
                    ComputeCost = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
}
