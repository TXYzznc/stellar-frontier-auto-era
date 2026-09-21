using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.Motion.Adapter;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.AI;

namespace AutoEra.World.Region
{
    /// <summary>Region-owned navigation data. Rebuilds only after topology changes, never from moving poses.</summary>
    public sealed class RegionNavigation : IDisposable
    {
        private readonly InitialRegion _region;
        private readonly MeshFilter _ground;
        private readonly float _radius, _height;
        private readonly List<NavMeshBuildSource> _sources = new List<NavMeshBuildSource>();
        private readonly Dictionary<PersistentId, RegionMachineNavigationBinding> _bindings = new Dictionary<PersistentId, RegionMachineNavigationBinding>();
        private readonly List<PersistentId> _removed = new List<PersistentId>();
        private NavMeshData _data;
        private NavMeshDataInstance _instance;
        private bool _dirty = true, _disposed;
        private PersistentId _bindingCandidate;
        public bool IsReady { get; private set; }
        public string Error { get; private set; }
        public int BuildCount { get; private set; }
        public int BindingCount => _bindings.Count;

        public RegionNavigation(InitialRegion region, MeshFilter ground, float maximumAgentRadius, float agentHeight)
        {
            _region = region ?? throw new ArgumentNullException(nameof(region));
            if (ground == null || ground.sharedMesh == null || ground.sharedMesh.vertexCount < 3 ||
                !MachineNavigationSettings.Positive(maximumAgentRadius) || !MachineNavigationSettings.Positive(agentHeight))
                throw new ArgumentException("Navigation needs an explicit ground mesh and valid agent dimensions.");
            _ground = ground; _radius = maximumAgentRadius; _height = agentHeight;
            region.ObjectsChanged += OnTopologyChanged;
        }
        private void OnTopologyChanged() { _dirty = true; }

        public bool RebuildIfNeeded()
        {
            if (_disposed || !_region.IsActive || _ground == null) { ReleaseData(); Error = "Navigation region is unavailable."; return false; }
            if (!_dirty) return IsReady;
            _dirty = false; IsReady = false;
            foreach (var binding in _bindings.Values) binding.Navigation.InvalidateRoute();
            NavMeshData next = null;
            try
            {
                _sources.Clear();
                _sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Mesh,
                    sourceObject = _ground.sharedMesh, transform = _ground.transform.localToWorldMatrix, area = 0 });
                float groundY = _ground.transform.position.y;
                foreach (RegionObject item in _region.Objects)
                {
                    if (!item.BlocksNavigation || (item.Kind == PersistentObjectKind.Machine &&
                        (_bindings.ContainsKey(item.Id) || item.Id == _bindingCandidate))) continue;
                    _sources.Add(new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, area = 1,
                        size = new Vector3(item.Size.x, _height * 2, item.Size.y),
                        transform = Matrix4x4.TRS(new Vector3(item.Position.x, groundY + _height, item.Position.y), Quaternion.Euler(0, item.Yaw, 0), Vector3.one) });
                }
                var settings = NavMesh.GetSettingsByID(0); settings.agentRadius = _radius; settings.agentHeight = _height;
                // The default radius/3 voxel is too coarse for the 0.25 m target sampling contract
                // of a 1.6 m carrier: even flat ground can be quantized above that tolerance.
                settings.overrideVoxelSize = true; settings.voxelSize = 0.1f;
                var bounds = new Bounds(new Vector3(_region.Bounds.center.x, groundY, _region.Bounds.center.y),
                    new Vector3(_region.Bounds.width, _height * 6, _region.Bounds.height));
                next = NavMeshBuilder.BuildNavMeshData(settings, _sources, bounds, Vector3.zero, Quaternion.identity);
                if (next == null) throw new InvalidOperationException("Native NavMesh build returned no data.");
                ReleaseData(); _data = next; next = null; _instance = NavMesh.AddNavMeshData(_data);
                if (!_instance.valid) throw new InvalidOperationException("Native NavMesh data could not be attached.");
                foreach (var binding in _bindings.Values)
                    if (!binding.Reattach()) throw new InvalidOperationException("Machine is no longer on the rebuilt navigation surface.");
                BuildCount++; IsReady = true; Error = null; return true;
            }
            catch (Exception error)
            {
                if (next != null) DestroySafely(next);
                ReleaseData(); Error = error.GetType().Name + ": " + error.Message; return false;
            }
        }

        /// <summary>
        /// 编辑态不能调 `Object.Destroy`：Unity 会记录
        /// 「Destroy may not be called from edit mode」**错误日志**，而 EditMode 测试把未预期的
        /// 错误日志当作失败——于是「在编辑器里打开区域场景再释放」这条正常路径会假失败。
        /// 这是真实缺陷而非测试问题：销毁本来就该按运行/编辑态分流。
        /// </summary>
        internal static void DestroySafely(UnityEngine.Object target)
        {
            if (target == null) return;
            if (UnityEngine.Application.isPlaying) UnityEngine.Object.Destroy(target);
            else UnityEngine.Object.DestroyImmediate(target);
        }

        public RegionMachineNavigationBinding Bind(MachineExecutionContext context, GameObject instance, MachineNavigationSettings settings,
            float wheelRadius, float wheelBase)
        {
            if (_disposed || !IsReady || context == null || instance == null ||
                !_region.TryGet(context.Machine.Id, out var model) || !ReferenceEquals(model.Machine, context.Machine) ||
                !context.Machine.Definition.CanMove || _bindings.ContainsKey(context.Machine.Id) ||
                model.Size.magnitude * .5f > _radius ||
                Vector2.Distance(model.Position, new Vector2(instance.transform.position.x, instance.transform.position.z)) > .01f)
                throw new InvalidOperationException("Bind requires a deployed same-identity machine on a ready region surface.");
            _bindingCandidate = context.Machine.Id; _dirty = true;
            try
            {
                if (!RebuildIfNeeded()) throw new InvalidOperationException(Error);
                var binding = new RegionMachineNavigationBinding(context, instance, settings, _radius, _height, wheelRadius, wheelBase);
                _bindings.Add(context.Machine.Id, binding); return binding;
            }
            catch { _dirty = true; throw; }
            finally { _bindingCandidate = PersistentId.Invalid; }
        }
        public void Tick(double now, float deltaSeconds)
        {
            if (_disposed) return;
            _removed.Clear();
            foreach (var pair in _bindings)
                if (!_region.TryGetMachine(pair.Key, out _) || !pair.Value.IsValid)
                { pair.Value.Dispose(); _removed.Add(pair.Key); _dirty = true; }
            foreach (var id in _removed) _bindings.Remove(id);
            // Failed surface rebuild still advances safety/timeout state; agents cannot resume without a valid path.
            RebuildIfNeeded();
            foreach (var binding in _bindings.Values) binding.Tick(now, deltaSeconds);
        }
        private void ReleaseData()
        {
            IsReady = false;
            foreach (var binding in _bindings.Values) binding.Navigation.InvalidateRoute();
            if (_instance.valid) _instance.Remove();
            if (_data != null) DestroySafely(_data);
            _data = null;
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _region.ObjectsChanged -= OnTopologyChanged;
            foreach (var binding in _bindings.Values) binding.Dispose();
            _bindings.Clear(); ReleaseData();
        }
    }

    public sealed class RegionMachineNavigationBinding : IDisposable
    {
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly MachineNavigationMotionAdapter _motion;
        private bool _disposed;
        public MachineNavigation Navigation { get; }
        public bool IsValid => !_disposed && _transform != null && _agent != null;
        internal RegionMachineNavigationBinding(MachineExecutionContext context, GameObject instance, MachineNavigationSettings settings,
            float radius, float height, float wheelRadius, float wheelBase)
        {
            if (instance.GetComponent<NavMeshAgent>() != null) throw new InvalidOperationException("Refusing to take ownership of another navigation agent.");
            _transform = instance.transform;
            var rig = instance.GetComponentInChildren<MotionRig>();
            _motion = new MachineNavigationMotionAdapter(rig, _transform.position, _transform.eulerAngles.y, wheelRadius, wheelBase);
            _agent = instance.AddComponent<NavMeshAgent>();
            try
            {
                var driver = new UnityMachineNavigationDriver(_agent, settings, radius, height, (int)(context.Machine.Id.Value % 80) + 10);
                if (!_agent.isOnNavMesh) throw new InvalidOperationException("Machine instance is not on the navigation surface.");
                Navigation = new MachineNavigation(context, driver, settings);
            }
            catch { RegionNavigation.DestroySafely(_agent); throw; }
        }
        internal bool Reattach() => IsValid && _agent.Warp(_transform.position);
        internal void Tick(double now, float deltaSeconds)
        { Navigation.Tick(now, deltaSeconds); _motion.Sample(_transform.position, _transform.eulerAngles.y); }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Navigation.Dispose();
            if (_agent != null) { _agent.enabled = false; RegionNavigation.DestroySafely(_agent); }
        }
    }
}
