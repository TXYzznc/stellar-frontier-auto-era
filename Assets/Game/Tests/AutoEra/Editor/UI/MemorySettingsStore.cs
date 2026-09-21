using System.Collections.Generic;
using AutoEra.Settings;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 内存设置存储：测试用的 <see cref="ISettingsStore"/> 实现。
    ///
    /// 它存在的理由就是那条边界本身——设置域的一切读写只经过 `ISettingsStore`，
    /// 所以测试可以完全脱离 `GF.Setting` 验证整条链路，包括「写入失败时值仍保留在内存里」
    /// 这条规格要求（靠 <see cref="FailWrites"/> 制造失败）。
    ///
    /// 放在独立文件里而不是各测试类内部：显示页与声音页的用例都需要它，
    /// 两份实现迟早会在「失败语义」上走样。
    /// </summary>
    public sealed class MemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> _bools = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

        /// <summary>为 true 时 <see cref="Save"/> 报失败——用来验证「失败仍保留内存值」。</summary>
        public bool FailWrites { get; set; }

        /// <summary>落盘次数。用来验证「一次写入只落盘一次」。</summary>
        public int SaveCount { get; private set; }

        public string GetString(string key, string defaultValue) => _strings.TryGetValue(key, out string v) ? v : defaultValue;
        public int GetInt(string key, int defaultValue) => _ints.TryGetValue(key, out int v) ? v : defaultValue;
        public bool GetBool(string key, bool defaultValue) => _bools.TryGetValue(key, out bool v) ? v : defaultValue;
        public float GetFloat(string key, float defaultValue) => _floats.TryGetValue(key, out float v) ? v : defaultValue;

        public void SetString(string key, string value) => _strings[key] = value;
        public void SetInt(string key, int value) => _ints[key] = value;
        public void SetBool(string key, bool value) => _bools[key] = value;
        public void SetFloat(string key, float value) => _floats[key] = value;

        /// <summary>某键是否被写过（用于断言「只该写的键写了、不该写的没写」）。</summary>
        public bool HasKey(string key) =>
            _strings.ContainsKey(key) || _ints.ContainsKey(key) || _bools.ContainsKey(key) || _floats.ContainsKey(key);

        public bool Save(out string error)
        {
            SaveCount++;
            error = FailWrites ? "本机设置写入失败：测试夹具故意让它失败。内存中的值已保留，但重启后可能丢失。" : null;
            return !FailWrites;
        }
    }
}
