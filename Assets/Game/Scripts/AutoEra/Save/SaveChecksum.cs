using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AutoEra.Save
{
    /// <summary>
    /// 存档内容校验值。
    ///
    /// **刻意不哈希序列化后的 JSON**：JSON 的字段顺序、缩进和转义都会随序列化器实现变化，
    /// 那样算出来的校验值会在一次无害的重序列化之后全部失效——把「换个写法」误报成「存档损坏」。
    /// 所以这里按**显式规范化的载荷**计算：字段之间用不会出现在两侧的换行分隔，
    /// 内容长度单独参与，最后 SHA-256 取小写十六进制。
    ///
    /// 校验的字段就是规格列出的那一组（槽位ID、保存UTC时间、世界时间、摘要、内容长度、内容），
    /// 所以任何一处被改动都会被判定出来。
    /// </summary>
    public static class SaveChecksum
    {
        /// <summary>按记录字段计算校验值。</summary>
        public static string Compute(SaveSlotRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return Compute(record.Version, record.SlotIndex, record.SavedUtcTicks,
                record.WorldTimeMilliseconds, record.Summary, record.ContentJson);
        }

        public static string Compute(int version, int slotIndex, long savedUtcTicks,
            long worldTimeMilliseconds, string summary, string contentJson)
        {
            string payload = string.Join("\n", new[]
            {
                version.ToString(CultureInfo.InvariantCulture),
                slotIndex.ToString(CultureInfo.InvariantCulture),
                savedUtcTicks.ToString(CultureInfo.InvariantCulture),
                worldTimeMilliseconds.ToString(CultureInfo.InvariantCulture),
                summary ?? string.Empty,
                (contentJson ?? string.Empty).Length.ToString(CultureInfo.InvariantCulture),
                contentJson ?? string.Empty,
            });

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var text = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    text.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return text.ToString();
            }
        }

        /// <summary>恒定时间比较，避免把校验写成「逐字符提前返回」。</summary>
        public static bool Matches(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual) ||
                expected.Length != actual.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                difference |= expected[i] ^ actual[i];
            }

            return difference == 0;
        }
    }
}
