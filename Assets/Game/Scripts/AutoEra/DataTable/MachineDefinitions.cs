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
/// MachineDefinitions
/// </summary>
public class MachineDefinitions : DataRowBase
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
        /// ModelId
        /// </summary>
        public int ModelId
        {
            get;
            private set;
        }

        /// <summary>
        /// NameKey
        /// </summary>
        public string NameKey
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
        /// SensorSlots
        /// </summary>
        public int SensorSlots
        {
            get;
            private set;
        }

        /// <summary>
        /// CoreSlots
        /// </summary>
        public int CoreSlots
        {
            get;
            private set;
        }

        /// <summary>
        /// EffectorSlots
        /// </summary>
        public int EffectorSlots
        {
            get;
            private set;
        }

        /// <summary>
        /// BaseCapacity
        /// </summary>
        public int BaseCapacity
        {
            get;
            private set;
        }

        /// <summary>
        /// CanMove
        /// </summary>
        public bool CanMove
        {
            get;
            private set;
        }

        /// <summary>
        /// CanRotate
        /// </summary>
        public bool CanRotate
        {
            get;
            private set;
        }

        /// <summary>
        /// MaximumIntegrity
        /// </summary>
        public double MaximumIntegrity
        {
            get;
            private set;
        }

        /// <summary>
        /// PurchasePrice
        /// </summary>
        public int PurchasePrice
        {
            get;
            private set;
        }

        /// <summary>
        /// RecyclePrice
        /// </summary>
        public int RecyclePrice
        {
            get;
            private set;
        }

        /// <summary>
        /// IdlePower
        /// </summary>
        public double IdlePower
        {
            get;
            private set;
        }

        /// <summary>
        /// WorkingPower
        /// </summary>
        public double WorkingPower
        {
            get;
            private set;
        }

        /// <summary>
        /// Availability
        /// </summary>
        public string Availability
        {
            get;
            private set;
        }

        /// <summary>
        /// Prefab
        /// </summary>
        public string Prefab
        {
            get;
            private set;
        }

        /// <summary>
        /// 交互占地沿本地X（米）。轴名写死是因为设计文档按「长×宽」记占地（见DEC-147与灰盒规格：轮式载体交互占地2.6×1.8米＝Z2.6×X1.8），不写死轴向会在实现侧再次推错
        /// </summary>
        public double FootprintX
        {
            get;
            private set;
        }

        /// <summary>
        /// 交互占地沿本地Z（米）。0表示该型号尚未配置占地，放置流程须以「未配置占地」拒绝而不是猜一个值
        /// </summary>
        public double FootprintZ
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
            Level = int.Parse(columnStrings[index++]);
            SensorSlots = int.Parse(columnStrings[index++]);
            CoreSlots = int.Parse(columnStrings[index++]);
            EffectorSlots = int.Parse(columnStrings[index++]);
            BaseCapacity = int.Parse(columnStrings[index++]);
            CanMove = bool.Parse(columnStrings[index++]);
            CanRotate = bool.Parse(columnStrings[index++]);
            MaximumIntegrity = double.Parse(columnStrings[index++]);
            PurchasePrice = int.Parse(columnStrings[index++]);
            RecyclePrice = int.Parse(columnStrings[index++]);
            IdlePower = double.Parse(columnStrings[index++]);
            WorkingPower = double.Parse(columnStrings[index++]);
            Availability = columnStrings[index++];
            Prefab = columnStrings[index++];
            FootprintX = double.Parse(columnStrings[index++]);
            FootprintZ = double.Parse(columnStrings[index++]);

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
                    Level = binaryReader.Read7BitEncodedInt32();
                    SensorSlots = binaryReader.Read7BitEncodedInt32();
                    CoreSlots = binaryReader.Read7BitEncodedInt32();
                    EffectorSlots = binaryReader.Read7BitEncodedInt32();
                    BaseCapacity = binaryReader.Read7BitEncodedInt32();
                    CanMove = binaryReader.ReadBoolean();
                    CanRotate = binaryReader.ReadBoolean();
                    MaximumIntegrity = binaryReader.ReadDouble();
                    PurchasePrice = binaryReader.Read7BitEncodedInt32();
                    RecyclePrice = binaryReader.Read7BitEncodedInt32();
                    IdlePower = binaryReader.ReadDouble();
                    WorkingPower = binaryReader.ReadDouble();
                    Availability = binaryReader.ReadString();
                    Prefab = binaryReader.ReadString();
                    FootprintX = binaryReader.ReadDouble();
                    FootprintZ = binaryReader.ReadDouble();
                }
            }

            return true;
        }
}
}
