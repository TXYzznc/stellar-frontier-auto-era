using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace AutoEra.Save
{
    /// <summary>Owned, detached domain DTOs. Capture sources must never retain or mutate Data after capture.</summary>
    public sealed class WorldSnapshotSection
    {
        public WorldSnapshotSection(string name, int version, object data)
        {
            if (string.IsNullOrWhiteSpace(name) || version <= 0 || data == null) throw new ArgumentException("Invalid snapshot section.");
            Name = name; Version = version; Data = data;
        }
        [JsonProperty("name")] public string Name { get; }
        [JsonProperty("version")] public int Version { get; }
        [JsonProperty("data")] public object Data { get; }
    }

    public sealed class WorldSnapshotDocument
    {
        public const int CurrentVersion = 1;
        public WorldSnapshotDocument(long worldMilliseconds, ulong allocatedThrough, long revision, string summary, IEnumerable<WorldSnapshotSection> sections)
        {
            if (worldMilliseconds < 0 || revision < 0 || sections == null) throw new ArgumentException("Invalid snapshot header.");
            WorldMilliseconds = worldMilliseconds; AllocatedThrough = allocatedThrough; Revision = revision; Summary = summary ?? string.Empty;
            var copy = new List<WorldSnapshotSection>(); var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var section in sections)
            {
                if (section == null || !names.Add(section.Name)) throw new ArgumentException("Duplicate or null snapshot section.");
                copy.Add(section);
            }
            if (copy.Count == 0) throw new ArgumentException("A world snapshot requires domain sections.");
            Sections = copy.AsReadOnly();
        }
        [JsonProperty("worldSchemaVersion")] public int WorldSchemaVersion => CurrentVersion;
        [JsonProperty("worldMilliseconds")] public long WorldMilliseconds { get; }
        [JsonProperty("allocatedThrough")] public ulong AllocatedThrough { get; }
        [JsonProperty("revision")] public long Revision { get; }
        [JsonProperty("summary")] public string Summary { get; }
        [JsonProperty("sections")] public IReadOnlyList<WorldSnapshotSection> Sections { get; }
    }

    public sealed class LoadedWorldSnapshot
    {
        internal LoadedWorldSnapshot(long time, ulong allocated, long revision, string summary, Dictionary<string,JObject> sections)
        { WorldMilliseconds = time; AllocatedThrough = allocated; Revision = revision; Summary = summary; Sections = new ReadOnlyDictionary<string,JObject>(sections); }
        public long WorldMilliseconds { get; }
        public ulong AllocatedThrough { get; }
        public long Revision { get; }
        public string Summary { get; }
        internal IReadOnlyDictionary<string,JObject> Sections { get; }
        public bool ContainsSection(string name) => name != null && Sections.ContainsKey(name);
        public bool TryReadSection<T>(string name, out T data, out string reason) where T : class
        {
            data = null; reason = null;
            if (name == null || !Sections.TryGetValue(name,out var section)) { reason = "缺少领域段：" + name; return false; }
            try
            {
                data = section.ToObject<T>(JsonSerializer.Create(WorldSnapshotCodec.CreateSettings()));
                if (data != null) return true;
                reason = "领域段内容为空：" + name; return false;
            }
            catch (JsonException exception) { reason = "领域段内容无效：" + name + " / " + exception.GetType().Name; return false; }
        }
    }

    /// <summary>Missing DTO fields must not silently turn a saved operation into default idle state.</summary>
    internal sealed class WorldSnapshotFieldContractResolver : DefaultContractResolver
    {
        internal static readonly WorldSnapshotFieldContractResolver Instance = new WorldSnapshotFieldContractResolver();
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization serialization)
        {
            var property = base.CreateProperty(member, serialization);
            if (member is FieldInfo field && field.IsPublic && !field.IsStatic &&
                member.DeclaringType?.Namespace?.StartsWith("AutoEra.", StringComparison.Ordinal) == true &&
                !member.DeclaringType.Namespace.StartsWith("AutoEra.Tests", StringComparison.Ordinal))
                property.Required = field.IsInitOnly ? Required.Always : Required.AllowNull;
            else if (property.Required == Required.Default && member is PropertyInfo &&
                member.DeclaringType?.Namespace?.StartsWith("AutoEra.", StringComparison.Ordinal) == true)
            {
                foreach (var constructor in member.DeclaringType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!constructor.IsDefined(typeof(JsonConstructorAttribute), false)) continue;
                    foreach (var parameter in constructor.GetParameters())
                        if (string.Equals(parameter.Name, member.Name, StringComparison.OrdinalIgnoreCase))
                            property.Required = parameter.ParameterType.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) == null
                                ? Required.Always : Required.AllowNull;
                }
            }
            return property;
        }
    }

    public static class WorldSnapshotCodec
    {
        internal static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor, ReferenceLoopHandling = ReferenceLoopHandling.Error,
                ContractResolver = WorldSnapshotFieldContractResolver.Instance };
            settings.Converters.Add(new WorldSnapshotValueConverter());
            return settings;
        }
        public static string Serialize(WorldSnapshotDocument snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            return JsonConvert.SerializeObject(snapshot, Formatting.None, CreateSettings());
        }
        public static bool TryRead(string json, IReadOnlyDictionary<string,int> requiredSections, out LoadedWorldSnapshot snapshot, out string reason)
        {
            snapshot = null; reason = null;
            if (string.IsNullOrWhiteSpace(json) || requiredSections == null || requiredSections.Count == 0)
            { reason = "存档缺少有效世界内容或领域目录"; return false; }
            try
            {
                var root = JObject.Parse(json, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                if (root["worldSchemaVersion"]?.Type != JTokenType.Integer || root.Value<int>("worldSchemaVersion") != WorldSnapshotDocument.CurrentVersion)
                { reason = "不支持的世界快照版本"; return false; }
                if (root["worldMilliseconds"]?.Type != JTokenType.Integer || root["allocatedThrough"]?.Type != JTokenType.Integer || root["revision"]?.Type != JTokenType.Integer)
                { reason = "世界快照时间或身份修订字段缺失"; return false; }
                long time = root.Value<long>("worldMilliseconds"), revision = root.Value<long>("revision");
                if (!ulong.TryParse(root["allocatedThrough"].ToString(Formatting.None),NumberStyles.None,CultureInfo.InvariantCulture,out ulong allocated))
                { reason = "世界快照永久ID范围无效"; return false; }
                if (time < 0 || revision < 0 || !(root["sections"] is JArray sections))
                { reason = "世界快照头或领域段无效"; return false; }
                if (root["summary"] != null && root["summary"].Type != JTokenType.String)
                { reason = "世界快照概要无效"; return false; }
                var values = new Dictionary<string,JObject>(StringComparer.Ordinal);
                foreach (var token in sections)
                {
                    if (!(token is JObject section) || section["name"]?.Type != JTokenType.String || section["version"]?.Type != JTokenType.Integer || !(section["data"] is JObject data))
                    { reason = "世界快照领域段无效"; return false; }
                    string name = section.Value<string>("name");
                    if (string.IsNullOrWhiteSpace(name) || values.ContainsKey(name))
                    { reason = "世界快照领域段重复或缺少名称"; return false; }
                    if (!requiredSections.TryGetValue(name,out int version) || section.Value<int>("version") != version)
                    { reason = "不支持的领域段或版本：" + name; return false; }
                    values.Add(name,data);
                }
                foreach (var expected in requiredSections)
                    if (!values.ContainsKey(expected.Key)) { reason = "世界快照缺少必需领域段：" + expected.Key; return false; }
                snapshot = new LoadedWorldSnapshot(time,allocated,revision,root.Value<string>("summary") ?? string.Empty,values);
                return true;
            }
            catch (Exception exception) when (exception is JsonException || exception is FormatException || exception is OverflowException || exception is InvalidCastException)
            { reason = "世界快照内容无法解析：" + exception.GetType().Name; return false; }
        }
    }
}
