using System;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Scene binding only. Serialized fields describe content, not persistent runtime state.</summary>
    public sealed class RegionObjectView : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [SerializeField] private PersistentObjectKind _kind;
        [SerializeField] private Vector2 _footprint = Vector2.one;
        [SerializeField] private bool _blocksNavigation = true;
        [SerializeField] private bool _infiniteResource;
        [SerializeField] private Transform _focusAnchor;
        [SerializeField] private string[] _workChannels = Array.Empty<string>();
        [SerializeField] private bool _tintProxy;
        [SerializeField] private Color _proxyTint = Color.white;
        private readonly System.Collections.Generic.List<RegionWorkQueue> _workQueues = new System.Collections.Generic.List<RegionWorkQueue>();
        private InitialRegion _region;
        private LineRenderer _outline;
        private Material _outlineMaterial;
        /// <summary>
        /// 本视图的区域对象是**它自己注册**的（场景种子），还是**指向领域已存在**的对象（已部署机器）。
        /// 只有前者可以在释放时移除领域对象——机器的存续由花名册与区域决定，视图只是它的呈现。
        /// 用显式来源标志而不是靠 `Model != null` 推断，因为两种来源都有非空 Model。
        /// </summary>
        private bool _ownsModel;
        public bool IsHighlighted { get; private set; }

        public RegionObject Model { get; private set; }
        public Vector3 FocusPosition => _focusAnchor != null ? _focusAnchor.position : transform.position;

        public void Initialize(InitialRegion region)
        {
            if (_region != null) throw new InvalidOperationException("View is already bound.");
            if (region == null) throw new ArgumentNullException(nameof(region));
            Vector3 p = transform.position;
            RegionObject model = region.Register(_kind, _displayName, new Vector2(p.x, p.z),
                _footprint, transform.eulerAngles.y, _blocksNavigation);
            _region = region;
            Model = model;
            _ownsModel = true;
            if (_tintProxy)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", _proxyTint);
                Transform visual = transform.Find("Visual");
                if (visual != null) foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>()) renderer.SetPropertyBlock(block);
            }
            Model.SetPublicState(_kind == PersistentObjectKind.ResourcePoint ? "有效" : "待机", infinite: _infiniteResource);
            foreach (string channel in _workChannels)
                _workQueues.Add(new RegionWorkQueue(region, Model.Id,
                    new Rect(Model.Position - _footprint * .5f, _footprint), channel));
        }

        /// <summary>
        /// Binds this view to a region object that **already exists** — the deployment path for machines.
        ///
        /// 不能复用 <see cref="Initialize"/>：那条路径会用本视图的序列化字段调 <c>region.Register</c>
        /// **新建**一个区域对象，而机器在 <c>DeployMachine</c> 里已经建好了自己的对象，
        /// 再注册一次就会出现两个对象。因此本方法只认已存在的对象，取不到就抛，
        /// 调用方必须先让领域部署成功。
        ///
        /// 同时**不设置公开状态**：机器的公开状态属于领域，视图不该替它决定。
        /// </summary>
        public void BindDeployed(InitialRegion region, PersistentId id)
        {
            if (_region != null) throw new InvalidOperationException("View is already bound.");
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (!id.IsValid) throw new ArgumentException("A valid deployed object id is required.", nameof(id));
            if (!region.TryGet(id, out RegionObject model))
                throw new InvalidOperationException("Bound view requires an existing region object; deploy the machine first.");
            _region = region;
            Model = model;
            _ownsModel = false;
            foreach (string channel in _workChannels)
                _workQueues.Add(new RegionWorkQueue(region, Model.Id,
                    new Rect(Model.Position - Model.Size * .5f, Model.Size), channel));
        }

        public RegionWorkQueue GetWorkChannel(int index) => index >= 0 && index < _workQueues.Count ? _workQueues[index] : null;

        public void SetHighlight(bool hovered, bool selected)
        {
            IsHighlighted = Model != null && Model.IsRegistered && (hovered || selected);
            if (!UnityEngine.Application.isPlaying) return;
            if (_outline == null && IsHighlighted)
            {
                var node = new GameObject("SelectionOutline");
                node.transform.SetParent(transform, false);
                _outline = node.AddComponent<LineRenderer>();
                _outline.useWorldSpace = false; _outline.loop = true; _outline.positionCount = 4;
                // 轮廓尺寸以领域对象的实际占地为准：场景种子两者的值相同，
                // 而绑定到已部署机器时视图的序列化占地可能不是权威值。
                Vector2 footprint = Model != null ? Model.Size : _footprint;
                Vector2 half = footprint * .5f;
                _outline.SetPosition(0, new Vector3(-half.x,.035f,-half.y));
                _outline.SetPosition(1, new Vector3(-half.x,.035f,half.y));
                _outline.SetPosition(2, new Vector3(half.x,.035f,half.y));
                _outline.SetPosition(3, new Vector3(half.x,.035f,-half.y));
                _outlineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _outline.sharedMaterial = _outlineMaterial;
                _outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _outline.receiveShadows = false;
            }
            if (_outline == null) return;
            _outline.enabled = IsHighlighted;
            _outline.widthMultiplier = selected ? .075f : .035f;
            _outlineMaterial.SetColor("_BaseColor", selected ? new Color(1f,.58f,.16f) : Color.white);
        }

        public void Release()
        {
            SetHighlight(false, false);
            foreach (RegionWorkQueue queue in _workQueues) queue.Dispose();
            _workQueues.Clear();
            // 只有本视图自己注册的对象才能由本视图移除。指向已部署机器的视图释放后，
            // 机器必须保持已部署——玩家看到的是「机器还在，只是没有表现」，而不是数据被删掉。
            if (_region != null && Model != null && _ownsModel) _region.Remove(Model.Id);
            _region = null;
            Model = null;
            _ownsModel = false;
        }

        private void OnDestroy()
        {
            Release();
            if (_outlineMaterial != null) Destroy(_outlineMaterial);
        }
    }
}
