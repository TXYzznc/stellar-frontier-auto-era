using System;
using System.Globalization;
using AutoEra.Events;
using AutoEra.World.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AutoEra.Save
{
    /// <summary>Closed value types only. Never traverses Unity objects or Vector.normalized properties.</summary>
    internal sealed class WorldSnapshotValueConverter : JsonConverter
    {
        public override bool CanConvert(Type type) => type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(PersistentId) || type == typeof(CorrelationId) || type == typeof(PersistentObjectReference);
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is PersistentId identity) { writer.WriteValue(identity.Value); return; }
            if (value is CorrelationId correlation) { writer.WriteValue(correlation.Value); return; }
            if (value is PersistentObjectReference reference)
            { writer.WriteStartObject(); writer.WritePropertyName("id"); writer.WriteValue(reference.Id.Value); writer.WritePropertyName("kind"); writer.WriteValue((int)reference.ExpectedKind); writer.WriteEndObject(); return; }
            Vector3 position = value is Vector2 v2 ? new Vector3(v2.x,v2.y,0) : (Vector3)value;
            Check(position.x); Check(position.y); Check(position.z);
            writer.WriteStartObject(); writer.WritePropertyName("x"); writer.WriteValue(position.x); writer.WritePropertyName("y"); writer.WriteValue(position.y);
            if (value is Vector3) { writer.WritePropertyName("z"); writer.WriteValue(position.z); }
            writer.WriteEndObject();
        }
        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (type == typeof(PersistentId)) return new PersistentId(ReadIdentity(token));
            if (type == typeof(CorrelationId)) return new CorrelationId(ReadIdentity(token));
            if (!(token is JObject data)) throw new JsonSerializationException("Invalid snapshot value object.");
            if (type == typeof(PersistentObjectReference))
            {
                if (data["kind"]?.Type != JTokenType.Integer) throw new JsonSerializationException("Missing reference kind.");
                var kind = (PersistentObjectKind)data.Value<int>("kind");
                if (!Enum.IsDefined(typeof(PersistentObjectKind),kind)) throw new JsonSerializationException("Invalid reference kind.");
                return new PersistentObjectReference(new PersistentId(ReadIdentity(data["id"])),kind);
            }
            float x=ReadCoordinate(data["x"]), y=ReadCoordinate(data["y"]);
            if (type == typeof(Vector2)) return new Vector2(x,y);
            return new Vector3(x,y,ReadCoordinate(data["z"]));
        }
        private static ulong ReadIdentity(JToken token)
        {
            if (token?.Type != JTokenType.Integer || !ulong.TryParse(token.ToString(Formatting.None),NumberStyles.None,CultureInfo.InvariantCulture,out ulong identity))
                throw new JsonSerializationException("Invalid snapshot identity.");
            return identity;
        }
        private static float ReadCoordinate(JToken token)
        {
            if (token == null || token.Type != JTokenType.Integer && token.Type != JTokenType.Float) throw new JsonSerializationException("Missing snapshot coordinate.");
            double value=token.Value<double>();
            if (double.IsNaN(value) || double.IsInfinity(value) || value > float.MaxValue || value < -float.MaxValue) throw new JsonSerializationException("Invalid snapshot coordinate.");
            return (float)value;
        }
        private static void Check(float value)
        { if (float.IsNaN(value) || float.IsInfinity(value)) throw new JsonSerializationException("Invalid snapshot coordinate."); }
    }
}
