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
/// Resource point definitions with capacity, cache, production parameters and integrity
/// </summary>
public class ResourcePointDefinitions : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    /// Stable row identifier
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// Resource point model ID
        /// </summary>
        public int ModelId
        {
            get;
            private set;
        }

        /// <summary>
        /// Localization key
        /// </summary>
        public string NameKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Farmland planting capacity
        /// </summary>
        public int PlantCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// Temporary output cache capacity
        /// </summary>
        public int CacheCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// Total resource reserve (number or 'Infinite')
        /// </summary>
        public string TotalReserve
        {
            get;
            private set;
        }

        /// <summary>
        /// Production rate description
        /// </summary>
        public string ProductionRate
        {
            get;
            private set;
        }

        /// <summary>
        /// Structure integrity limit
        /// </summary>
        public double MaximumIntegrity
        {
            get;
            private set;
        }

        /// <summary>
        /// Resource availability state
        /// </summary>
        public string Availability
        {
            get;
            private set;
        }

        /// <summary>
        /// Prefab relative path
        /// </summary>
        public string Prefab
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
            ModelId = int.Parse(columnStrings[index++]);
            NameKey = columnStrings[index++];
            PlantCapacity = int.Parse(columnStrings[index++]);
            CacheCapacity = int.Parse(columnStrings[index++]);
            TotalReserve = columnStrings[index++];
            ProductionRate = columnStrings[index++];
            MaximumIntegrity = double.Parse(columnStrings[index++]);
            Availability = columnStrings[index++];
            Prefab = columnStrings[index++];

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    ModelId = binaryReader.Read7BitEncodedInt32();
                    NameKey = binaryReader.ReadString();
                    PlantCapacity = binaryReader.Read7BitEncodedInt32();
                    CacheCapacity = binaryReader.Read7BitEncodedInt32();
                    TotalReserve = binaryReader.ReadString();
                    ProductionRate = binaryReader.ReadString();
                    MaximumIntegrity = binaryReader.ReadDouble();
                    Availability = binaryReader.ReadString();
                    Prefab = binaryReader.ReadString();
                }
            }

            return true;
        }
}
}
