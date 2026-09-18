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
/// Building definitions with construction time, capacity, power consumption and economic data
/// </summary>
public class BuildingDefinitions : DataRowBase
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
        /// Building model ID
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
        /// Auto construction time in seconds
        /// </summary>
        public int ConstructionTimeSeconds
        {
            get;
            private set;
        }

        /// <summary>
        /// Physical item storage capacity
        /// </summary>
        public int StorageCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// Output cache slots for workshop
        /// </summary>
        public int OutputCacheSlots
        {
            get;
            private set;
        }

        /// <summary>
        /// Rated power output for generators
        /// </summary>
        public double RatedPower
        {
            get;
            private set;
        }

        /// <summary>
        /// Battery energy storage capacity
        /// </summary>
        public double EnergyCapacity
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
        /// Construction cost in coins
        /// </summary>
        public int ConstructionPrice
        {
            get;
            private set;
        }

        /// <summary>
        /// Recycle value
        /// </summary>
        public int RecyclePrice
        {
            get;
            private set;
        }

        /// <summary>
        /// Idle power consumption
        /// </summary>
        public double IdlePower
        {
            get;
            private set;
        }

        /// <summary>
        /// Working power consumption
        /// </summary>
        public double WorkingPower
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
            ConstructionTimeSeconds = int.Parse(columnStrings[index++]);
            StorageCapacity = int.Parse(columnStrings[index++]);
            OutputCacheSlots = int.Parse(columnStrings[index++]);
            RatedPower = double.Parse(columnStrings[index++]);
            EnergyCapacity = double.Parse(columnStrings[index++]);
            ProductionRate = columnStrings[index++];
            MaximumIntegrity = double.Parse(columnStrings[index++]);
            ConstructionPrice = int.Parse(columnStrings[index++]);
            RecyclePrice = int.Parse(columnStrings[index++]);
            IdlePower = double.Parse(columnStrings[index++]);
            WorkingPower = double.Parse(columnStrings[index++]);
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
                    ConstructionTimeSeconds = binaryReader.Read7BitEncodedInt32();
                    StorageCapacity = binaryReader.Read7BitEncodedInt32();
                    OutputCacheSlots = binaryReader.Read7BitEncodedInt32();
                    RatedPower = binaryReader.ReadDouble();
                    EnergyCapacity = binaryReader.ReadDouble();
                    ProductionRate = binaryReader.ReadString();
                    MaximumIntegrity = binaryReader.ReadDouble();
                    ConstructionPrice = binaryReader.Read7BitEncodedInt32();
                    RecyclePrice = binaryReader.Read7BitEncodedInt32();
                    IdlePower = binaryReader.ReadDouble();
                    WorkingPower = binaryReader.ReadDouble();
                    Availability = binaryReader.ReadString();
                    Prefab = binaryReader.ReadString();
                }
            }

            return true;
        }
}
}
