using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Machines.Sensors
{
    public sealed class MachineSensorSet : IDisposable
    {
        private readonly MachineExecutionContext _context;
        private readonly SortedDictionary<PersistentId, MachineSensor> _sensors = new SortedDictionary<PersistentId, MachineSensor>();
        private readonly List<MachineSensor> _iteration = new List<MachineSensor>();
        private readonly List<MachineSensor> _ordered = new List<MachineSensor>();
        private bool _ticking;
        private bool _disposed;
        internal MachineSensorSet(MachineExecutionContext context) { _context = context; }
        public int Count => _sensors.Count;
        public bool TryGet(PersistentId component, out MachineSensor sensor) => _sensors.TryGetValue(component, out sensor);
        public bool Remove(PersistentId component)
        {
            if (!_sensors.TryGetValue(component, out var sensor)) return false;
            _sensors.Remove(component); sensor.Dispose(); _ordered.Remove(sensor);
            return true;
        }
        public MachineSensor Bind(ComponentInstance component, SensorProfile profile, ISensorEnvironment environment, ISensorAnchor anchor)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MachineSensorSet));
            if (component == null || _sensors.ContainsKey(component.Id)) throw new ArgumentException("Sensor already bound.");
            var sensor = new MachineSensor(_context, component, profile, environment, anchor);
            _sensors.Add(component.Id, sensor);
            _ordered.Clear();
            foreach (var item in _sensors.Values) _ordered.Add(item);
            return sensor;
        }
        public void Tick(long worldMilliseconds)
        {
            if (_disposed) return;
            if (_ticking) throw new InvalidOperationException("Sensor tick cannot be reentered.");
            _ticking = true;
            try
            {
                _iteration.Clear();
                _iteration.AddRange(_ordered);
                for (int i = 0; i < _iteration.Count && !_disposed; i++) _iteration[i].Tick(worldMilliseconds);
            }
            finally { _iteration.Clear(); _ticking = false; }
        }
        public void Dispose()
        { if (_disposed) return; _disposed = true; foreach (var sensor in _sensors.Values) sensor.Dispose(); _sensors.Clear(); _ordered.Clear(); }
    }
}
