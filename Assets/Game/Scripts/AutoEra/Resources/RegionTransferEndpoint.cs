using System;
using AutoEra.World;
using AutoEra.World.Region;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.Logistics
{
    /// <summary>Explicit authoring of a real loading point and its legal docking area.</summary>
    public sealed class RegionTransferEndpoint : MonoBehaviour
    {
        [SerializeField] private ResourceTransferConfig _config;
        [SerializeField] private Transform _contact;
        [SerializeField] private Transform _dock;
        [SerializeField] private bool _warehouse;
        private InitialRegion _region;
        private RegionObject _object;
        public ResourceTransferRules Rules { get; private set; }
        public AutoEraWorldSession World { get; private set; }
        public RegionWorkQueue Queue { get; private set; }
        public PersistentId Id => _object?.Id ?? default;
        public CargoOwner Owner { get; private set; }
        public bool IsAvailable => World != null && World.IsActive && _region != null && _region.IsActive && _object != null && _object.IsRegistered;
        public Vector3 ContactPosition => _contact.position;
        public Vector3 DockPosition => _dock.position;
        public float FacingYaw => Quaternion.LookRotation(Vector3.ProjectOnPlane(ContactPosition-DockPosition,Vector3.up)).eulerAngles.y;
        public bool Warehouse => _warehouse;
        public void ConfigureForEditor(ResourceTransferConfig config, Transform contact, Transform dock, bool warehouse)
        { _config = config; _contact = contact; _dock = dock; _warehouse = warehouse; }
        public void Initialize(AutoEraWorldSession world, InitialRegion region, RegionObject value)
        {
            if (World != null || _config == null || _contact == null || _dock == null || world == null || region == null || value == null) throw new InvalidOperationException("Explicit transfer endpoint required.");
            World = world; _region = region; _object = value; Rules = _config.Read(); Owner = new CargoOwner(_warehouse ? CargoOwnerKind.Receiver : CargoOwnerKind.WorldFree, value.Id);
            if (!world.Resources.Authority.TryReadContainer(Owner, out var container) || !container.IsAvailable) throw new InvalidOperationException("Authoritative transfer container missing.");
            Queue = new RegionWorkQueue(region, value.Id, new Rect(new Vector2(_dock.position.x-.4f,_dock.position.z-.4f), new Vector2(.8f,.8f)), "装卸");
        }
        public void Release() { Queue?.Dispose(); Queue = null; World = null; _region = null; _object = null; Rules = null; }
        private void OnDestroy() => Release();
    }
}
