using AutoEra.UI;
using AutoEra.World.Region;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoEra.Input
{
    public struct RegionInputFrame
    {
        public Vector2 Pan, Orbit, Pointer;
        public float Zoom;
        public bool Select, Focus, Cancel, Rotate;
    }

    public interface IRegionInputSource { RegionInputFrame Read(); }

    /// <summary>PC device reads are confined to this replaceable input boundary.</summary>
    public sealed class RegionInputModule : MonoBehaviour
    {
        [SerializeField] private InitialRegionScene _scene;
        [SerializeField] private RegionCameraController _camera;
        [SerializeField] private LayerMask _selectionLayers;
        private IRegionInputSource _source;
        private RegionObjectView _selected;
        private RegionObjectView _hovered;
        private float _lastClickTime = -1f;
        [SerializeField] private RegionFieldAccess _fieldAccess = new RegionFieldAccess();
        private bool _accessible;
        private RegionPlacementPreview _placement;
        /// <summary>本次预览的所有者：本组件创建（true）还是调用方拥有（false）。与 <c>RegionObjectView._ownsModel</c> 同一个判据。</summary>
        private bool _ownsPlacement;
        private MachineDeploymentFlow _flow;
        private LineRenderer _placementLine;
        private Material _placementMaterial;
        public RegionPlacementPreview Placement => _placement;

        /// <summary>最近一次由流程提交的结局；null＝本次落位还没有提交过。</summary>
        public MachineDeploymentOutcome? LastOutcome { get; private set; }

        /// <summary>最近一次提交的失败原因（成功时为 null）。界面直接展示，不各自拼字符串。</summary>
        public string LastReason { get; private set; }

        /// <summary>Read-only manifest of the active bindings; the settings page displays from here.</summary>
        public RegionInputBindingSet Bindings { get; } = RegionInputBindingSet.CreateDefault();

        /// <summary>
        /// 镜头参数的应用目标：设置页把本机参数落到**正在使用的这台镜头**上。
        /// 场景未配置镜头时为 null，设置页据此说明「参数已保存，进入区域后生效」。
        /// </summary>
        public IRegionCameraTarget CameraTarget => _camera;

        /// <summary>
        /// 驱动一次**流程拥有**的落位（界面走这条）：指针移动／旋转喂给流程的预览，
        /// 点击提交时调用 <see cref="MachineDeploymentFlow.TryCommit"/> 而不是预览自己的 `Confirm`——
        /// 否则会绕过流程的结局映射，界面拿到的就只有「成功/失败」而没有可展示的原因。
        /// </summary>
        public void BeginPlacement(MachineDeploymentFlow flow)
        {
            EndPlacement();
            if (flow == null || !flow.IsActive || flow.Preview == null)
                throw new System.ArgumentException("An active deployment flow with a preview is required.", nameof(flow));
            _flow = flow;
            _placement = flow.Preview;
            _ownsPlacement = false;
            LastOutcome = null;
            LastReason = null;
            BuildPlacementOutline(flow.Size);
        }

        /// <summary>自建预览的入口（编辑器证据工具用）：只做校验，不涉及任何领域提交。</summary>
        public void BeginPlacement(Vector2 size, System.Action<Vector2,float> confirmed)
        {
            EndPlacement();
            _placement = new RegionPlacementPreview(_scene.Region, size, confirmed);
            _ownsPlacement = true;
            LastOutcome = null;
            LastReason = null;
            BuildPlacementOutline(size);
        }

        private void BuildPlacementOutline(Vector2 size)
        {
            if (!UnityEngine.Application.isPlaying) return;
            var node = new GameObject("PlacementOutline");
            node.transform.SetParent(transform, false);
            _placementLine = node.AddComponent<LineRenderer>();
            _placementLine.loop = true; _placementLine.positionCount = 4; _placementLine.useWorldSpace = false;
            _placementLine.widthMultiplier = .09f;
            _placementMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _placementLine.sharedMaterial = _placementMaterial;
            _placementLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Vector2 half = size * .5f;
            _placementLine.SetPosition(0,new Vector3(-half.x,0,-half.y));
            _placementLine.SetPosition(1,new Vector3(-half.x,0,half.y));
            _placementLine.SetPosition(2,new Vector3(half.x,0,half.y));
            _placementLine.SetPosition(3,new Vector3(half.x,0,-half.y));
        }

        /// <summary>
        /// 界面「旋转」按钮走这条：与键盘旋转键同一条路径（改的是同一个预览），
        /// 不做任何领域提交。没有进行中的落位时返回 false。
        /// </summary>
        public bool RotatePlacement()
        {
            if (_placement == null) return false;
            _placement.Rotate();
            return true;
        }

        /// <summary>
        /// 界面「确认部署」按钮走这条：与鼠标点击**同一条提交路径**
        /// （<see cref="TryConfirmPlacement"/> → 流程的 <c>TryCommit</c>），成功后结束本次落位。
        /// 失败**不结束**落位——玩家还能继续挪位置重试，这与点击行为一致。
        /// </summary>
        public bool ConfirmPlacement()
        {
            if (!TryConfirmPlacement()) return false;
            EndPlacement();
            return true;
        }

        /// <summary>
        /// 结束当前落位（界面切换页／关闭时调用）。只清掉本模块的引用与轮廓线：
        /// 流程拥有的预览由流程自己释放（那正是 <c>_ownsPlacement</c> 这个判据的用途），
        /// 所以这里**不会**把界面的预览悄悄销毁掉。
        /// </summary>
        public void CancelPlacement() => EndPlacement();

        private void EndPlacement()
        {
            // 只销毁自己建的预览；流程拥有的预览由流程自己管（它的 TryCommit 会处理）。
            if (_ownsPlacement) _placement?.Dispose();
            _placement = null;
            _ownsPlacement = false;
            _flow = null;
            if (_placementLine != null) Destroy(_placementLine.gameObject);
            if (_placementMaterial != null) Destroy(_placementMaterial);
            _placementLine = null; _placementMaterial = null;
        }
        private void OnDestroy() => EndPlacement();

        /// <summary>
        /// 点击提交。走流程时用 <see cref="MachineDeploymentFlow.TryCommit"/> 并把结局留在
        /// <see cref="LastOutcome"/>/<see cref="LastReason"/> 供界面展示；自建预览时保持原有语义。
        /// 失败**不结束**落位——玩家应该能继续挪位置重试，这也是接入前的既有行为。
        /// </summary>
        private bool TryConfirmPlacement()
        {
            if (_placement == null) return false;
            if (_flow == null) return _placement.Confirm();

            bool committed = _flow.TryCommit(out _, out string reason);
            LastReason = committed ? null : reason;
            LastOutcome = _flow.LastOutcome;
            return committed;
        }

        public void SetSource(IRegionInputSource source) => _source = source;
        public bool SelectTarget(RegionObjectView target)
        {
            if (_scene == null || !_scene.Select(target, false)) return false;
            if (_selected != null) _selected.SetHighlight(_selected == _hovered, false);
            if (_selected != target) _accessible = false;
            _selected = target;
            if (_selected != null) _selected.SetHighlight(false, true);
            UpdateFieldAccess(AutoEraUiRuntime.BlocksWorldInput);
            return true;
        }

        // ---------------------------------------------------------------- 定位（聚焦）

        /// <summary>当前选中的现场对象身份；没有有效选中时为 Invalid。</summary>
        public AutoEra.World.Identity.PersistentId SelectedObjectId =>
            _selected != null && _selected.Model != null ? _selected.Model.Id : AutoEra.World.Identity.PersistentId.Invalid;

        /// <summary>现在是否能定位：有选中的对象，而且镜头在手上。</summary>
        public bool CanFocusSelection => _selected != null && _selected.Model != null && _camera != null;

        /// <summary>
        /// 把镜头带到当前选中的现场对象。
        ///
        /// **这是「定位／聚焦」的唯一实现**：F 键、双击对象与界面上的「镜头聚焦／聚焦当前资源点／
        /// 聚焦建筑」按钮全都走这一条。规格把三者写成一件事
        /// （14-WorldBinding：「有效对象双击或F聚焦」；00-通用合同：「输入、Button和快捷键汇入同一意图」），
        /// 各写一份就会出现「按钮聚焦到锚点、双击聚焦到包围盒中心」这种看得见的偏差。
        ///
        /// 返回 false 表示没有可定位的对象——界面据此禁用按钮并说明原因，
        /// 而不是给一个点下去没反应的入口。
        /// </summary>
        public bool FocusSelection()
        {
            if (!CanFocusSelection)
            {
                return false;
            }

            _camera.Focus(_selected.FocusPosition);
            return true;
        }

        private void Awake() { if (_source == null) _source = new DesktopSource(Bindings); }

        private void Update()
        {
            bool blocked = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            Tick(Time.unscaledDeltaTime, blocked);
        }

        public void Tick(float elapsedSeconds, bool blocked)
        {
            if (_source == null || _scene == null || _scene.Region == null || !_scene.Region.IsActive || _camera == null) return;
            RegionInputFrame frame = _source.Read();
            bool managementOpen = AutoEraUiRuntime.BlocksWorldInput;
            UpdateFieldAccess(managementOpen);
            if (frame.Cancel)
            {
                if (!AutoEraUiRuntime.DispatchIntent(AutoEraUiIntent.Cancel))
                { if (_placement != null) EndPlacement(); else SelectTarget(null); }
                return;
            }
            if (blocked || managementOpen)
            {
                if (_hovered != null) _hovered.SetHighlight(false, _hovered == _selected);
                _hovered = null;
                return;
            }
            _camera.Apply(frame.Pan, frame.Orbit, frame.Zoom, elapsedSeconds, _scene.Region.Bounds);
            if (_placement != null && _camera.ViewCamera != null)
            {
                Ray pointer = _camera.ViewCamera.ScreenPointToRay(frame.Pointer);
                if (new Plane(Vector3.up, Vector3.zero).Raycast(pointer, out float distance))
                {
                    Vector3 p = pointer.GetPoint(distance);
                    _placement.Move(new Vector2(p.x,p.z));
                }
                if (frame.Rotate) _placement.Rotate();
                if (_placementLine != null)
                {
                    _placementLine.transform.SetPositionAndRotation(new Vector3(_placement.Position.x,.045f,_placement.Position.y), Quaternion.Euler(0,_placement.Yaw,0));
                    _placementMaterial.SetColor("_BaseColor", _placement.IsValid ? Color.white : Color.red);
                }
                if (frame.Select && TryConfirmPlacement()) EndPlacement();
                return;
            }
            if (_camera.ViewCamera != null)
            {
                RegionObjectView target = null;
                Ray ray = _camera.ViewCamera.ScreenPointToRay(frame.Pointer);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _selectionLayers, QueryTriggerInteraction.Collide))
                    target = hit.collider.GetComponentInParent<RegionObjectView>();
                if (_hovered != target && _hovered != null) _hovered.SetHighlight(false, _hovered == _selected);
                _hovered = target;
                if (target != null) target.SetHighlight(true, target == _selected);
                if (frame.Select)
                {
                    if (target != null && target == _selected && Time.unscaledTime - _lastClickTime < .3f)
                        FocusSelection();
                    SelectTarget(target);
                    _lastClickTime = Time.unscaledTime;
                }
            }
            if (frame.Focus)
                FocusSelection();
            UpdateFieldAccess(false);
        }

        private void UpdateFieldAccess(bool managementOpen)
        {
            if (_selected == null || _selected.Model == null || _camera.ViewCamera == null) _accessible = false;
            else if (_selected.Model.Kind != AutoEra.World.Identity.PersistentObjectKind.Machine) _accessible = true;
            else
            {
                Vector3 target = _selected.FocusPosition;
                Vector3 delta = target - _camera.FocusPosition;
                float distance = new Vector2(delta.x, delta.z).magnitude;
                _accessible = _fieldAccess.Evaluate(_accessible, distance, _camera.ViewCamera.transform.position.y - target.y,
                    _camera.ViewCamera.WorldToViewportPoint(target));
            }
            _scene.ShowFieldAccess(_accessible, managementOpen);
        }

        private sealed class DesktopSource : IRegionInputSource
        {
            private readonly RegionInputBindingSet _bindings;

            public DesktopSource(RegionInputBindingSet bindings)
            {
                _bindings = bindings ?? throw new System.ArgumentNullException(nameof(bindings));
            }

            public RegionInputFrame Read()
            {
                int horizontal = (Held(RegionInputAction.PanRight) ? 1 : 0) - (Held(RegionInputAction.PanLeft) ? 1 : 0);
                int vertical = (Held(RegionInputAction.PanForward) ? 1 : 0) - (Held(RegionInputAction.PanBack) ? 1 : 0);
                bool orbit = Held(RegionInputAction.Orbit);
                return new RegionInputFrame
                {
                    Pan = new Vector2(horizontal, vertical),
                    Orbit = orbit ? new Vector2(UnityEngine.Input.GetAxisRaw("Mouse X"), UnityEngine.Input.GetAxisRaw("Mouse Y")) : Vector2.zero,
                    Pointer = UnityEngine.Input.mousePosition,
                    Zoom = UnityEngine.Input.mouseScrollDelta.y,
                    Select = Pressed(RegionInputAction.Select),
                    Focus = Pressed(RegionInputAction.Focus),
                    Rotate = Pressed(RegionInputAction.Rotate),
                    Cancel = Pressed(RegionInputAction.Cancel)
                };
            }

            private bool Held(RegionInputAction action)
            {
                return _bindings.TryGet(action, out RegionInputBinding binding) && binding.Trigger == RegionInputTrigger.Hold
                    && UnityEngine.Input.GetKey(binding.Key);
            }

            private bool Pressed(RegionInputAction action)
            {
                return _bindings.TryGet(action, out RegionInputBinding binding) && binding.Trigger == RegionInputTrigger.Press
                    && UnityEngine.Input.GetKeyDown(binding.Key);
            }
        }
    }
}
