using System;
using System.Collections.Generic;
using AutoEra.DataTable;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.Logistics;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AutoEra.ResourcePoints
{
    /// <summary>Loads actual installed tool prefabs through GF entities and validates their mechanical reach.</summary>
    public sealed class RegionProductionTools : IDisposable, IProductionWorkContact, IResourceTransferContact
    {
        private sealed class MachineView
        { internal MachineInstance Machine; internal GameObject Root; internal ProductionToolMounts Mounts; }
        private sealed class Tool
        { internal ComponentInstance Component; internal EntityParams Pending; internal int EntityId; internal GameObject Root; internal MotionRig Rig;
            internal MotionJointBinding Yaw, Slide, Rotor, Lift; internal Renderer[] Renderers, HeadRenderers;
            internal MotionExecutor Motion; internal string ExecutionId; internal string[] JointIds; internal bool Claimed;
            internal float Feed, Axial, AxialExtent, Radial, Radius;
            internal MotionJointBinding[] ArmJoints; internal float[] ArmAngles;
            internal Transform Mount; internal string Error; }
        private readonly InitialRegion _region;
        private readonly IReadOnlyList<RegionProductionFacility> _facilities;
        private readonly List<Bounds> _treeObstacles = new List<Bounds>();
        private readonly List<RegionObject> _regionObjects = new List<RegionObject>();
        private readonly Dictionary<PersistentId, MachineView> _views = new Dictionary<PersistentId, MachineView>();
        private readonly Dictionary<PersistentId, Tool> _tools = new Dictionary<PersistentId, Tool>();
        private readonly List<PersistentId> _remove = new List<PersistentId>();
        private readonly string _entityGroup;
        private bool _dirty, _disposed;
        public int ReadyCount { get { int count = 0; foreach (var tool in _tools.Values) if (tool.Rig != null && tool.Error == null) count++; return count; } }
        public string GetReadinessReason(PersistentId component)
        {
            if (!_tools.TryGetValue(component, out var tool)) return "工具尚未登记；机器视图数=" + _views.Count;
            if (tool.Error != null) return tool.Error;
            if (tool.Pending != null) return "等待实体加载，实体ID=" + tool.EntityId;
            if (tool.Root == null) return "工具实体未回调";
            if (tool.Rig == null) return "工具缺少MotionRig";
            return "就绪";
        }
        public string GetContactGeometry(PersistentId component)
        { if (!_tools.TryGetValue(component, out var tool)) return "未登记";
            return "feed=" + tool.Feed + " axial=" + tool.Axial + "/" + tool.AxialExtent + " radial=" + tool.Radial + "/" + tool.Radius; }
        public RegionProductionTools(InitialRegion region, string entityGroup, IReadOnlyList<RegionProductionFacility> facilities)
        { _region = region ?? throw new ArgumentNullException(nameof(region)); _entityGroup = entityGroup; _facilities = facilities ?? throw new ArgumentNullException(nameof(facilities));
            _region.ObjectsChanged += RefreshRegionObjects; RefreshRegionObjects(); GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnFailure); }
        private void RefreshRegionObjects()
        { _regionObjects.Clear(); foreach (var value in _region.Objects) _regionObjects.Add(value); }
        public void RegisterMachineView(MachineInstance machine, GameObject root)
        {
            if (_disposed || machine == null || root == null) return;
            if (_views.TryGetValue(machine.Id, out var prior)) { if (prior.Root == root) return; prior.Machine.Changed -= OnMachineChanged; }
            _views[machine.Id] = new MachineView { Machine = machine, Root = root, Mounts = root.GetComponent<ProductionToolMounts>() };
            machine.Changed += OnMachineChanged; _dirty = true;
        }
        private void OnMachineChanged(MachineInstance machine) { _dirty = true; }
        public void Reconcile()
        {
            if (!_dirty || _disposed) return; _dirty = false; _remove.Clear();
            foreach (var pair in _tools)
                if (!_views.TryGetValue(pair.Value.Component.OwnerId, out var view) || view.Root == null || !view.Machine.Deployed || !Installed(view.Machine, pair.Value.Component.Id)) _remove.Add(pair.Key);
            foreach (var id in _remove) Remove(id);
            foreach (var view in _views.Values)
            {
                if (view.Root == null || !view.Machine.Deployed) continue;
                for (int i = 0; i < view.Machine.Definition.EffectorSlots; i++)
                {
                    var component = view.Machine.GetComponent(HardwareKind.Effector, i);
                    if (component == null || component.Definition.Id != 2201 && component.Definition.Id != 2203 && component.Definition.Id != 2204 || _tools.ContainsKey(component.Id)) continue;
                    var tool = new Tool { Component = component, Mount = view.Mounts != null ? view.Mounts.Read(i) : null };
                    _tools.Add(component.Id, tool);
                    var table = GF.DataTable.GetDataTable<ComponentDefinitions>();
                    // Runtime definitions carry ModelId; the loaded table is keyed by ModelId * 10 + Level.
                    var row = table?.GetDataRow(checked(component.Definition.Id * 10 + component.Definition.Level));
                    if (tool.Mount == null || row == null || row.Availability != "Ready" || string.IsNullOrWhiteSpace(row.Prefab))
                    { tool.Error = "正式工具资产或安装锚点未就绪"; continue; }
                    var parameters = EntityParams.Create(tool.Mount.position, tool.Mount.eulerAngles); tool.Pending = parameters; tool.EntityId = parameters.Id;
                    parameters.OnShowCallback = logic =>
                    {
                        tool.Pending = null;
                        if (_disposed || !_tools.TryGetValue(component.Id, out var current) || !ReferenceEquals(current, tool) || tool.Mount == null)
                        { GF.Entity.HideEntitySafe(tool.EntityId); return; }
                        tool.Root = logic.gameObject; tool.Root.transform.SetParent(tool.Mount, false); tool.Root.transform.localPosition = Vector3.zero; tool.Root.transform.localRotation = Quaternion.identity;
                        foreach (var collider in tool.Root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                        tool.Rig = tool.Root.GetComponent<MotionRig>(); tool.Renderers = tool.Root.GetComponentsInChildren<Renderer>(true);
                        bool saw = component.Definition.Id == 2203; bool arm = component.Definition.Id == 2201;
                        if (arm)
                        {
                            if (tool.Rig == null || !tool.Rig.TryValidate(out _) || !tool.Rig.TryGetBinding("base_yaw",out tool.Yaw) || !tool.Rig.TryGetBinding("wrist_roll",out tool.Rotor))
                            { tool.Error = "正式机械臂动作锚点缺失"; return; }
                            string[] armIds = { "wrist_pitch", "elbow_pitch", "shoulder_pitch", "base_yaw" }; tool.ArmJoints = new MotionJointBinding[armIds.Length]; tool.ArmAngles = new float[armIds.Length];
                            for (int j=0;j<armIds.Length;j++)
                                if (!tool.Rig.TryGetBinding(armIds[j],out tool.ArmJoints[j]) || !tool.Rotor.JointTransform.IsChildOf(tool.ArmJoints[j].JointTransform))
                                { tool.Error = "机械臂关节未控制实际末端"; return; }
                        }
                        else if (tool.Rig == null || !tool.Rig.TryValidate(out _) || !tool.Rig.TryGetBinding("mount_yaw", out tool.Yaw) ||
                            !tool.Rig.TryGetBinding(saw ? "feed_slide" : "press_slide", out tool.Slide) || !tool.Rig.TryGetBinding(saw ? "saw_spindle" : "drill_rotor", out tool.Rotor) ||
                            saw && !tool.Rig.TryGetBinding("lift_rail", out tool.Lift)) tool.Error = "正式工具的动作锚点不完整";
                        if (tool.Error == null)
                        {
                            if (!arm && (!tool.Slide.JointTransform.IsChildOf(tool.Yaw.JointTransform) || !tool.Rotor.JointTransform.IsChildOf(tool.Slide.JointTransform) ||
                                saw && !tool.Slide.JointTransform.IsChildOf(tool.Lift.JointTransform)))
                            { tool.Error = "正式工具关节未控制实际刀盘或钻头"; return; }
                            tool.HeadRenderers = tool.Rotor.JointTransform.GetComponentsInChildren<Renderer>(true);
                            if (tool.HeadRenderers.Length == 0) { tool.Error = "正式刀盘或钻头缺少接触几何"; return; }
                            tool.Motion = tool.Root.GetComponent<MotionExecutor>() ?? tool.Root.AddComponent<MotionExecutor>();
                            tool.Motion.Configure(tool.Rig); tool.ExecutionId = "production:" + component.Id.Value;
                            tool.JointIds = new string[tool.Rig.JointBindings.Count];
                            for (int j = 0; j < tool.JointIds.Length; j++) tool.JointIds[j] = tool.Rig.JointBindings[j].StableId;
                        }
                    };
                    GF.Entity.ShowEntity<ProductionToolEntity>(row.Prefab, _entityGroup, tool.EntityId, parameters);
                }
            }
        }
        private static bool Installed(MachineInstance machine, PersistentId component)
        { for (int i = 0; i < machine.Definition.EffectorSlots; i++) if (machine.GetComponent(HardwareKind.Effector, i)?.Id == component) return true; return false; }
        private void OnFailure(object sender, GameEventArgs args)
        {
            var failure = (ShowEntityFailureEventArgs)args;
            foreach (var tool in _tools.Values)
                if (tool.EntityId == failure.EntityId && tool.Pending != null)
                { ReferencePool.Release(tool.Pending); tool.Pending = null; tool.Error = "工具实体加载失败：" + failure.ErrorMessage; break; }
        }
        public bool TryContact(ComponentInstance component, PersistentId resourcePoint, Rect workArea, PersistentId treeTarget, Vector3 target, double height, out string reason)
        {
            reason = null;
            if (!_tools.TryGetValue(component.Id, out var tool) || tool.Root == null || tool.Rig == null || tool.Error != null)
            { reason = tool?.Error ?? "等待正式工具实体加载"; return false; }
            if (!_region.TryGet(component.OwnerId, out var machine)) { reason = "机器不在现场"; return false; }
            if (!tool.Claimed)
            {
                if (!tool.Motion.TryPrepare(tool.ExecutionId, tool.JointIds)) { reason = "工具动作关节由其它动作占用"; return false; }
                tool.Claimed = true;
            }
            bool saw = tool.Lift != null;
            SafePose(tool);
            Vector3 work = target + Vector3.up * (float)height;
            if (saw)
            {
                Vector3 local = tool.Root.transform.InverseTransformPoint(work);
                float yaw = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                if (yaw < tool.Yaw.MinimumValue || yaw > tool.Yaw.MaximumValue) { reason = "树超出锯盘转向范围"; return false; }
                Rotate(tool.Yaw, yaw);
                float axisY = tool.Lift.JointTransform.parent.TransformVector(tool.Lift.LocalAxis.normalized).y;
                if (Mathf.Abs(axisY) < .001f) { reason = "锯盘升降锚点无效"; return false; }
                float lift = (work.y - tool.Rotor.JointTransform.position.y) / axisY;
                if (lift < tool.Lift.MinimumValue || lift > tool.Lift.MaximumValue) { reason = "切割面超出真实升降范围"; return false; }
                Translate(tool.Lift, lift);
                Vector3 direction = tool.Slide.JointTransform.parent.TransformVector(tool.Slide.LocalAxis.normalized);
                float feed = Vector3.Dot(work - tool.Rotor.JointTransform.position, direction);
                Translate(tool.Slide, Mathf.Clamp(feed, tool.Slide.MinimumValue, tool.Slide.MaximumValue));
                Bounds bounds = BoundsOf(tool, true);
                float radius = Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
                Vector3 axis = tool.Rotor.JointTransform.TransformDirection(tool.Rotor.LocalAxis.normalized);
                Vector3 delta = work - tool.Rotor.JointTransform.position;
                float axial = Vector3.Dot(delta, axis);
                float axialExtent = Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z)));
                tool.Feed = feed; tool.Axial = axial; tool.AxialExtent = axialExtent; tool.Radial = (delta - axis * axial).magnitude; tool.Radius = radius;
                if (Mathf.Abs(axial) > axialExtent + .01f || (delta - axis * axial).magnitude > radius)
                { reason = "树超出锯盘进给和刀盘接触范围"; return false; }
            }
            else
            {
                target = new Vector3(tool.Rotor.JointTransform.position.x, target.y, tool.Rotor.JointTransform.position.z);
                if (!workArea.Contains(new Vector2(target.x, target.z))) { reason = "钻头工作轴不在资源点有效作业区"; return false; }
                Vector3 axis = tool.Slide.JointTransform.parent.TransformVector(tool.Slide.LocalAxis.normalized);
                Bounds before = BoundsOf(tool, true);
                if (Mathf.Abs(axis.y) < .001f) { reason = "钻头压钻锚点无效"; return false; }
                float press = (target.y - before.min.y) / axis.y;
                if (press < tool.Slide.MinimumValue || press > tool.Slide.MaximumValue) { reason = "地面超出真实压钻范围"; return false; }
                Translate(tool.Slide, press);
                Bounds after = BoundsOf(tool, true);
                if (target.x < after.min.x || target.x > after.max.x || target.z < after.min.z || target.z > after.max.z)
                { reason = "钻头工作轴不在有效地面点"; return false; }
            }
            // Keep-out checks use the actual transformed tool bounds against unrelated footprints.
            Bounds keepout = BoundsOf(tool);
            _treeObstacles.Clear();
            for (int i = 0; i < _facilities.Count; i++) if (_facilities[i] != null) _facilities[i].CollectOtherTreeObstacles(treeTarget, _treeObstacles);
            foreach (var obstacle in _treeObstacles) if (keepout.Intersects(obstacle)) { reason = "工具净空被其它树干占用"; return false; }
            for (int i = 0; i < _regionObjects.Count; i++)
            {
                var other = _regionObjects[i];
                if (other.Id == component.OwnerId || other.Id == resourcePoint || !other.BlocksNavigation) continue;
                if (RegionPlacement.Overlaps(new Vector2(keepout.center.x, keepout.center.z), new Vector2(keepout.size.x, keepout.size.z), 0,
                    other.Position, other.Size, other.Yaw)) { reason = "工具净空被其他设施或机器占用"; return false; }
            }
            return true;
        }
        public bool TryReachTransfer(ComponentInstance component, PersistentId endpoint, Vector3 position, double range, out string reason)
        {
            reason = null;
            if (!_tools.TryGetValue(component.Id,out var tool) || tool.Error != null || tool.Root == null || tool.ArmJoints == null)
            { reason = tool?.Error ?? "等待正式机械臂实体"; return false; }
            if (Vector3.Distance(tool.Yaw.JointTransform.position,position) > range) { reason = "装卸点超出机械臂作用距离"; return false; }
            if (!tool.Claimed)
            {
                if (!tool.Motion.TryPrepare(tool.ExecutionId,tool.JointIds)) { reason = "机械臂关节由其他行为占用"; return false; }
                SafePose(tool); Array.Clear(tool.ArmAngles,0,tool.ArmAngles.Length); tool.Claimed = true;
            }
            // CCD uses the actual approved hierarchy, pivot positions, axes and limits.
            // A configuration-clamped analytical answer alone cannot certify physical contact.
            for (int iteration=0;iteration<24;iteration++)
            {
                if ((tool.Rotor.JointTransform.position-position).sqrMagnitude <= .0025f) break;
                for (int i=0;i<tool.ArmJoints.Length;i++)
                {
                    var joint = tool.ArmJoints[i]; Vector3 pivot = joint.JointTransform.position;
                    Vector3 axis = joint.JointTransform.TransformDirection(joint.LocalAxis.normalized);
                    Vector3 current = Vector3.ProjectOnPlane(tool.Rotor.JointTransform.position-pivot,axis);
                    Vector3 desired = Vector3.ProjectOnPlane(position-pivot,axis);
                    if (current.sqrMagnitude < 1e-8 || desired.sqrMagnitude < 1e-8) continue;
                    tool.ArmAngles[i] = Mathf.Clamp(tool.ArmAngles[i]+Vector3.SignedAngle(current,desired,axis),joint.MinimumValue,joint.MaximumValue);
                    Rotate(joint,tool.ArmAngles[i]);
                }
            }
            tool.Radial = Vector3.Distance(tool.Rotor.JointTransform.position,position); tool.Radius = .05f;
            if (tool.Radial > .05f) { reason = "实际机械臂末端无法到达装卸点"; return false; }
            Bounds keepout = BoundsOf(tool); _treeObstacles.Clear();
            for (int i=0;i<_facilities.Count;i++) if (_facilities[i] != null) _facilities[i].CollectOtherTreeObstacles(default,_treeObstacles);
            for (int i=0;i<_treeObstacles.Count;i++) if (keepout.Intersects(_treeObstacles[i])) { reason="机械臂净空被树干占用"; return false; }
            for (int i=0;i<_regionObjects.Count;i++)
            { var other=_regionObjects[i]; if (other.Id==component.OwnerId || other.Id==endpoint || !other.BlocksNavigation) continue;
                if (RegionPlacement.Overlaps(new Vector2(keepout.center.x,keepout.center.z),new Vector2(keepout.size.x,keepout.size.z),0,other.Position,other.Size,other.Yaw))
                { reason="机械臂净空被其它设施占用"; return false; } }
            tool.Motion.TryTransition(tool.ExecutionId,MotionExecutionState.Prepared,MotionExecutionState.Running); return true;
        }
        private static Bounds BoundsOf(Tool tool, bool head = false)
        {
            var bounds = new Bounds(tool.Rotor.JointTransform.position, Vector3.zero); bool first = true;
            foreach (var renderer in head ? tool.HeadRenderers : tool.Renderers) if (renderer != null && renderer.enabled)
            { if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds); }
            return bounds;
        }
        private static void Translate(MotionJointBinding joint, float value) => joint.JointTransform.localPosition = joint.BindLocalPosition + joint.LocalAxis.normalized * value;
        private static void Rotate(MotionJointBinding joint, float value) => joint.JointTransform.localRotation = joint.BindLocalRotation * Quaternion.AngleAxis(value, joint.LocalAxis.normalized);
        private static void SafePose(Tool tool)
        { for (int i = 0; i < tool.Rig.JointBindings.Count; i++) { var joint = tool.Rig.JointBindings[i]; joint.JointTransform.localPosition = joint.SafeLocalPosition; joint.JointTransform.localRotation = joint.SafeLocalRotation; } }
        public void SetWorking(ComponentInstance component, bool working, double elapsedSeconds)
        {
            if (working && _tools.TryGetValue(component.Id, out var tool) && tool.Claimed && tool.Rotor != null)
            {
                tool.Motion.TryTransition(tool.ExecutionId, MotionExecutionState.Prepared, MotionExecutionState.Running);
                Rotate(tool.Rotor, (float)(elapsedSeconds * 720 % 360));
            }
        }
        public void SafeStop(ComponentInstance component)
        { if (_tools.TryGetValue(component.Id, out var tool) && tool.Rig != null && tool.Claimed)
            { tool.Motion.TryTransition(tool.ExecutionId, MotionExecutionState.Running, MotionExecutionState.Recovering);
                SafePose(tool); tool.Motion.TryRelease(tool.ExecutionId); tool.Claimed = false; } }
        private void Remove(PersistentId id)
        { if (!_tools.TryGetValue(id, out var tool)) return; SafeStop(tool.Component); _tools.Remove(id); if (tool.Pending != null) { ReferencePool.Release(tool.Pending); tool.Pending = null; } GF.Entity.HideEntitySafe(tool.EntityId); }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _region.ObjectsChanged -= RefreshRegionObjects; _regionObjects.Clear();
            if (GF.Event != null && GF.Event.Check(ShowEntityFailureEventArgs.EventId, OnFailure)) GF.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnFailure);
            foreach (var view in _views.Values) view.Machine.Changed -= OnMachineChanged;
            _remove.Clear(); foreach (var id in _tools.Keys) _remove.Add(id); foreach (var id in _remove) Remove(id); _views.Clear();
        }
    }
}
