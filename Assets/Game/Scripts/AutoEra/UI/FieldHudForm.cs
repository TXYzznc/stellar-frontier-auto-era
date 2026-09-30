using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>
    /// 现场 UI 的会话路由。常驻模块和对象详情各自拥有 UIForm Prefab、读模型和按钮绑定。
    /// 本 Form 不持有任何页面内容或列表模板。
    /// </summary>
    public sealed partial class FieldHudForm : AutoEraShellFormBase
    {
        private InitialRegion _region;
        private AutoEra.Input.RegionInputModule _regionInput;
        private int _residentFormSerialId;
        private int _detailFormSerialId;
        private int _openFieldPage = -1;
        private PersistentId _lastDetailSelectionId = PersistentId.Invalid;
        private long _worldMilliseconds;

        public override bool BlocksWorldInput => false;
        public InitialRegion Region => _region;
        public long WorldMilliseconds => _worldMilliseconds;
        public int OpenFieldPage => _openFieldPage;
        public bool CanLocateSelection => _regionInput != null && _regionInput.CanFocusSelection;
        public bool FocusSelection() => _regionInput != null && _regionInput.FocusSelection();

        protected override void OnAutoEraOpen()
        {
            AutoEraUiSession session = SessionOrNull;
            BindRegion(session?.Region);
            _regionInput = session?.RegionInput;
            _residentFormSerialId = AutoEraUiNavigator.Open(this, UIViews.FieldHudResidentForm);
            OpenFieldPageForSelection();
        }

        protected override void OnAutoEraClose(bool isShutdown) => Release();

        protected override void OnAutoEraRecycle()
        {
            Release();
            base.OnAutoEraRecycle();
        }

        private void Release()
        {
            if (_region != null) _region.SelectionChanged -= OnRegionSelectionChanged;
            _region = null;
            _regionInput = null;
            AutoEraUiNavigator.Close(_detailFormSerialId);
            AutoEraUiNavigator.Close(_residentFormSerialId);
            _detailFormSerialId = 0;
            _residentFormSerialId = 0;
            _openFieldPage = -1;
            _lastDetailSelectionId = PersistentId.Invalid;
        }

        public void BindRegion(InitialRegion region)
        {
            if (_region == region) return;
            if (_region != null) _region.SelectionChanged -= OnRegionSelectionChanged;
            _region = region;
            if (_region != null) _region.SelectionChanged += OnRegionSelectionChanged;
            _lastDetailSelectionId = PersistentId.Invalid;
            OpenFieldPageForSelection();
        }

        public void SetFieldAccess(bool accessible, bool managementOpen)
        {
            // 管理界面的世界输入占用由其自身声明，现场层始终不反向锁住输入。
        }

        public void ShowWorldTime(long worldMilliseconds)
        {
            _worldMilliseconds = worldMilliseconds;
            if (_residentFormSerialId > 0 && GF.UI != null && GF.UI.HasUIForm(_residentFormSerialId))
                (GF.UI.GetUIForm(_residentFormSerialId).Logic as FieldHudResidentForm)?.ShowWorldTime(worldMilliseconds);
            OpenFieldPageForSelection();
        }

        public bool ShowFieldPage(int pageIndex)
        {
            if (pageIndex < 5 || pageIndex > 21)
            {
                Debug.LogWarning($"[AutoEra][FieldHud] 忽略无效详情页 page={pageIndex}");
                return false;
            }
            _openFieldPage = pageIndex;
            if (_detailFormSerialId > 0 && GF.UI != null && GF.UI.HasUIForm(_detailFormSerialId))
            {
                Debug.Log($"[AutoEra][FieldHud] 更新现有详情 Form serial={_detailFormSerialId} page={pageIndex}");
                (GF.UI.GetUIForm(_detailFormSerialId).Logic as FieldHudDetailForm)?.ShowSelectionPage(pageIndex);
            }
            else
            {
                _detailFormSerialId = AutoEraUiNavigator.Open(this, UIViews.FieldHudDetailForm, new AutoEraUiPageRequest(pageIndex));
                Debug.Log($"[AutoEra][FieldHud] 打开详情 Form serial={_detailFormSerialId} page={pageIndex}");
            }
            return _detailFormSerialId > 0;
        }

        private void OnRegionSelectionChanged() => OpenFieldPageForSelection();

        private void OpenFieldPageForSelection()
        {
            PersistentId selected = _region != null ? _region.SelectedId : PersistentId.Invalid;
            if (selected == _lastDetailSelectionId) return;
            _lastDetailSelectionId = selected;
            int page = ResolveFieldPageIndex();
            Debug.Log($"[AutoEra][FieldHud] selection={selected} resolvedPage={page} detailSerial={_detailFormSerialId}");
            if (page < 0)
            {
                AutoEraUiNavigator.Close(_detailFormSerialId);
                _detailFormSerialId = 0;
                _openFieldPage = -1;
                return;
            }
            ShowFieldPage(page);
        }

        private int ResolveFieldPageIndex()
        {
            if (_region == null || !_region.SelectedId.IsValid || !_region.TryGet(_region.SelectedId, out RegionObject obj))
                return -1;
            switch (obj.Kind)
            {
                case PersistentObjectKind.Machine: return 5;
                case PersistentObjectKind.Building: return 14;
                case PersistentObjectKind.ResourcePoint:
                    switch (obj.Name)
                    {
                        case "人工林": return 10;
                        case "地表矿脉": return 11;
                        case "水域": return 12;
                        default: return 9;
                    }
                default: return -1;
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
