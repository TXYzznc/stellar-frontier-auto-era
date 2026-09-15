using System;
using AutoEra.UI;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Explicit world-procedure scene entry; does not create a global world or use scene searches.</summary>
    public sealed class InitialRegionScene : MonoBehaviour
    {
        [SerializeField] private Rect _bounds = new Rect(-40, -40, 80, 80);
        [SerializeField] private RegionObjectView[] _objects;
        [SerializeField] private string[] _entityPrefabs;
        [SerializeField] private string _entityGroup = "Default";
        [SerializeField] private MeshFilter _navigationGround;
        [SerializeField] private float _navigationRadius = 1.6f;
        [SerializeField] private float _navigationHeight = 2f;
        private readonly List<int> _entityIds = new List<int>();
        private readonly Dictionary<int, EntityParams> _pendingEntities = new Dictionary<int, EntityParams>();
        private int _entityVersion;
        private bool _entityEvents;
        private Action<string> _failure;
        private AutoEraWorldSession _session;
        private FieldHudForm _hud;
        private long _lastDisplayedSecond = -1;
        public InitialRegion Region { get; private set; }
        public RegionNavigation Navigation { get; private set; }
        public long WorldMilliseconds => _session != null && _session.IsActive ? _session.Clock.WorldMilliseconds : 0;
        private readonly System.Collections.Generic.List<AutoEra.Machines.Sensors.MachineSensorSet> _sensorSets = new System.Collections.Generic.List<AutoEra.Machines.Sensors.MachineSensorSet>();
        public void AttachSensors(AutoEra.Machines.Sensors.MachineSensorSet sensors)
        {
            if (Region == null || !Region.IsActive || sensors == null || _sensorSets.Contains(sensors))
                throw new ArgumentException("An active region and unique explicit sensor set are required.");
            _sensorSets.Add(sensors);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private double _developmentTimeMultiplier = 1d;
        public bool TrySetDevelopmentTimeMultiplier(double multiplier)
        {
            if (double.IsNaN(multiplier) || double.IsInfinity(multiplier) || multiplier < 0d) return false;
            _developmentTimeMultiplier = multiplier;
            return true;
        }
#endif

        public void InitializeRuntime(AutoEraWorldSession session, Action ready, Action<string> failed)
        {
            if (Region != null || session == null || !session.IsActive) throw new InvalidOperationException("Invalid region initialization.");
            if (_objects == null || _entityPrefabs == null || _objects.Length != _entityPrefabs.Length || _objects.Length == 0)
                throw new InvalidOperationException("Region entity configuration missing.");
            _session = session;
            Region = new InitialRegion(session, _bounds);
            int version = ++_entityVersion;
            _failure = failed;
            GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnEntityFailure);
            _entityEvents = true;
            int remaining = _objects.Length;
            try
            {
                for (int i = 0; i < _objects.Length; i++)
                {
                    RegionObjectView seed = _objects[i];
                    if (seed == null || string.IsNullOrWhiteSpace(_entityPrefabs[i])) throw new InvalidOperationException("Missing region seed.");
                    seed.gameObject.SetActive(false);
                    var parameters = EntityParams.Create(seed.transform.position, seed.transform.eulerAngles);
                    int id = parameters.Id;
                    _entityIds.Add(id);
                    _pendingEntities.Add(id, parameters);
                    parameters.OnShowCallback = logic =>
                    {
                        _pendingEntities.Remove(id);
                        if (version != _entityVersion || Region == null) { GF.Entity.HideEntitySafe(id); return; }
                        try
                        {
                            ((InitialRegionEntity)logic).Bind(Region);
                            if (--remaining == 0) { InitializeNavigation(); ready?.Invoke(); }
                        }
                        catch (Exception error) { _failure?.Invoke(error.Message); }
                    };
                    GF.Entity.ShowEntity<InitialRegionEntity>(_entityPrefabs[i], _entityGroup, id, parameters);
                }
            }
            catch { Release(); throw; }
        }

        private void OnEntityFailure(object sender, GameEventArgs args)
        {
            var data = (ShowEntityFailureEventArgs)args;
            if (!_pendingEntities.TryGetValue(data.EntityId, out EntityParams parameters)) return;
            _pendingEntities.Remove(data.EntityId);
            ReferencePool.Release(parameters);
            _failure?.Invoke(data.ErrorMessage);
        }

        public void Initialize(AutoEraWorldSession session)
        {
            if (Region != null) throw new InvalidOperationException("Region scene is already initialized.");
            if (session == null || !session.IsActive) throw new ArgumentException("An active world session is required.");
            _session = session;
            Region = new InitialRegion(session, _bounds);
            try
            {
                if (_objects == null || _objects.Length == 0) throw new InvalidOperationException("Region content is not configured.");
                foreach (RegionObjectView view in _objects)
                {
                    if (view == null) throw new InvalidOperationException("Region content has a missing view.");
                    view.Initialize(Region);
                }
                InitializeNavigation();
            }
            catch { Release(); throw; }
        }

        public void BindHud(FieldHudForm hud)
        {
            if (Region == null) throw new InvalidOperationException("Initialize region before binding HUD.");
            if (_hud != null) _hud.BindRegion(null);
            _hud = hud;
            _lastDisplayedSecond = -1;
            if (_hud != null) { _hud.BindRegion(Region); _hud.BindMachines(_session.Machines); }
        }

        private void InitializeNavigation()
        {
            if (_navigationGround == null) return; // Legacy/test fixtures have no navigation configuration.
            Navigation = new RegionNavigation(Region, _navigationGround, _navigationRadius, _navigationHeight);
            if (!Navigation.RebuildIfNeeded()) throw new InvalidOperationException(Navigation.Error);
        }

        public bool Select(RegionObjectView view, bool inputBlocked)
            => Region != null && Region.Select(view != null && view.Model != null ? view.Model.Id : PersistentId.Invalid, inputBlocked);
        public void ShowFieldAccess(bool accessible, bool managementOpen) => _hud?.SetFieldAccess(accessible, managementOpen);

        public void Advance(double realSeconds)
        {
            if (_session == null || !_session.IsActive) { Release(); return; }
            Navigation?.Tick(UnityEngine.Time.realtimeSinceStartupAsDouble, (float)realSeconds);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool advanced = _session.Clock.TryAdvanceDevelopmentRealtimeSeconds(realSeconds, _developmentTimeMultiplier);
#else
            bool advanced = _session.Clock.TryAdvanceRealtimeSeconds(realSeconds);
#endif
            if (!advanced)
                throw new ArgumentOutOfRangeException(nameof(realSeconds));
            long second = _session.Clock.WorldMilliseconds / 1000;
            long sensorTime = _session.Clock.WorldMilliseconds;
            int sensorCount = _sensorSets.Count;
            for (int i = 0; i < sensorCount && i < _sensorSets.Count; i++) _sensorSets[i].Tick(sensorTime);
            if (_hud != null && second != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = second;
                _hud.ShowWorldTime(_session.Clock.WorldMilliseconds);
            }
        }

        public void Release()
        {
            foreach (var sensors in _sensorSets) sensors.Dispose();
            _sensorSets.Clear();
            Navigation?.Dispose(); Navigation = null;
            _entityVersion++;
            _failure = null;
            if (_entityEvents) { GF.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnEntityFailure); _entityEvents = false; }
            foreach (int id in _entityIds) GF.Entity.HideEntitySafe(id);
            _entityIds.Clear();
            foreach (EntityParams parameters in _pendingEntities.Values) ReferencePool.Release(parameters);
            _pendingEntities.Clear();
            if (_hud != null) _hud.BindRegion(null);
            _hud = null;
            if (_objects != null) foreach (RegionObjectView view in _objects) if (view != null) view.Release();
            Region?.Dispose();
            Region = null;
            _session = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _developmentTimeMultiplier = 1d;
#endif
        }

        private void OnDestroy() => Release();
    }
}
