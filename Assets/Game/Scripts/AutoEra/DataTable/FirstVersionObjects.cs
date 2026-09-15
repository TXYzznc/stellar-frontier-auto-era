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
/// Resource coverage metadata; ResourceState is art delivery only, never gameplay/configuration readiness. Name is catalog annotation, not runtime localized UI.
/// </summary>
public class FirstVersionObjects : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    /// Catalog identity
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// Category
        /// </summary>
        public string Category
        {
            get;
            private set;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// DefinitionModelId
        /// </summary>
        public int DefinitionModelId
        {
            get;
            private set;
        }

        /// <summary>
        /// ResourceState
        /// </summary>
        public string ResourceState
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
            Category = columnStrings[index++];
            Name = columnStrings[index++];
            DefinitionModelId = int.Parse(columnStrings[index++]);
            ResourceState = columnStrings[index++];
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
                    Category = binaryReader.ReadString();
                    Name = binaryReader.ReadString();
                    DefinitionModelId = binaryReader.Read7BitEncodedInt32();
                    ResourceState = binaryReader.ReadString();
                    Prefab = binaryReader.ReadString();
                }
            }

            return true;
        }
}
}
