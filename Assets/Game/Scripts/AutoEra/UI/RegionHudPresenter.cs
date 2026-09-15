using System;
using System.Globalization;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>Projects public region state only; does not grant sensor access or settle production.</summary>
    public sealed class RegionHudPresenter : IDisposable
    {
        private readonly InitialRegion _region;
        private RegionObject _selected;
        private bool _disposed;

        public RegionHudPresenter(InitialRegion region)
        {
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _region.SelectionChanged += RefreshSelection;
            _region.ObjectsChanged += RefreshSummary;
            RefreshSelection();
        }

        public string ObjectSummary { get; private set; } = string.Empty;
        public int ObjectCount => _disposed ? 0 : _region.Count;
        public bool HasSelection => !_disposed && _region.IsActive && _selected != null && _selected.IsRegistered;
        public event Action Changed;

        public void RefreshSelection()
        {
            if (_disposed) return;
            if (_selected != null) _selected.Changed -= OnSelectedChanged;
            _region.TryGet(_region.SelectedId, out _selected);
            if (_selected != null) _selected.Changed += OnSelectedChanged;
            RefreshSummary();
        }

        private void OnSelectedChanged(RegionObject obj) => RefreshSummary();

        private void RefreshSummary()
        {
            if (!HasSelection) ObjectSummary = string.Empty;
            else
            {
                string kind = _selected.Kind == PersistentObjectKind.Machine ? "机器" :
                    _selected.Kind == PersistentObjectKind.Building ? "建筑" : "资源点";
                ObjectSummary = string.Format(CultureInfo.InvariantCulture,
                    "{0}\n{1} · {2}\n位置：{3:0.0}, {4:0.0}",
                    _selected.Name, kind, _selected.PublicStatus, _selected.Position.x, _selected.Position.y);
                if (_selected.ResourceIsInfinite) ObjectSummary += "\n资源：无限";
                else if (_selected.PublicResourceAmount.HasValue)
                    ObjectSummary += "\n可用资源：" + _selected.PublicResourceAmount.Value.ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(_selected.WorkSummary)) ObjectSummary += "\n" + _selected.WorkSummary;
            }
            Changed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _region.SelectionChanged -= RefreshSelection;
            _region.ObjectsChanged -= RefreshSummary;
            if (_selected != null) _selected.Changed -= OnSelectedChanged;
            _selected = null;
            ObjectSummary = string.Empty;
            _disposed = true;
            Changed = null;
        }
    }
}
