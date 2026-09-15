using System;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class RegionObject
    {
        internal RegionObject(PersistentId id, PersistentObjectKind kind, string name, Vector2 position,
            Vector2 size, float yaw, bool blocksNavigation, AutoEra.Machines.MachineInstance machine = null)
        {
            Id = id; Kind = kind; Name = name; Position = position; Size = size;
            Yaw = yaw; BlocksNavigation = blocksNavigation;
            Machine = machine;
        }

        public PersistentId Id { get; }
        public PersistentObjectKind Kind { get; }
        public string Name { get; private set; }
        /// <summary>Borrowed identity; null for region-owned objects and legacy proxies.</summary>
        public AutoEra.Machines.MachineInstance Machine { get; }
        public Vector2 Position { get; internal set; }
        public Vector2 Size { get; }
        public float Yaw { get; internal set; }
        public bool BlocksNavigation { get; }
        public bool IsRegistered { get; internal set; }
        public string PublicStatus { get; private set; } = "待机";
        public long? PublicResourceAmount { get; private set; }
        public bool ResourceIsInfinite { get; private set; }
        public string WorkSummary { get; private set; } = string.Empty;
        private readonly System.Collections.Generic.SortedDictionary<string, string> _workChannels = new System.Collections.Generic.SortedDictionary<string, string>(StringComparer.Ordinal);
        public event Action<RegionObject> Changed;

        internal void SynchronizeMachineName()
        {
            if (Machine == null || Name == Machine.Name) return;
            Name = Machine.Name;
            Changed?.Invoke(this);
        }

        internal void SetWorkChannel(string channel, PersistentId owner, int waiting, bool remove = false)
        {
            if (remove) _workChannels.Remove(channel);
            else _workChannels[channel] = channel + "：" + (owner.IsValid ? "占用者 " + owner.Value : "空闲") + "；等待 " + waiting;
            WorkSummary = string.Join("\n", _workChannels.Values);
            Changed?.Invoke(this);
        }

        public void SetPublicState(string status, long? resourceAmount = null, bool infinite = false)
        {
            if (string.IsNullOrWhiteSpace(status) || resourceAmount < 0 || (infinite && resourceAmount.HasValue))
                throw new ArgumentException("Invalid public state.");
            PublicStatus = status; PublicResourceAmount = resourceAmount; ResourceIsInfinite = infinite;
            Changed?.Invoke(this);
        }
    }
}
