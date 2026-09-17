using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Input
{
    /// <summary>Semantic actions a binding set must describe. Pointer and zoom are unbound device channels.</summary>
    public enum RegionInputAction
    {
        PanForward = 0,
        PanBack = 1,
        PanLeft = 2,
        PanRight = 3,
        Orbit = 4,
        Select = 5,
        Focus = 6,
        Rotate = 7,
        Cancel = 8,
    }

    /// <summary>Hold reads continuously; Press reads only the down edge.</summary>
    public enum RegionInputTrigger
    {
        Hold = 0,
        Press = 1,
    }

    /// <summary>One immutable action binding. Mouse buttons are expressed through KeyCode.Mouse0..2.</summary>
    public readonly struct RegionInputBinding : IEquatable<RegionInputBinding>
    {
        public RegionInputBinding(RegionInputAction action, RegionInputTrigger trigger, KeyCode key)
        {
            Action = action;
            Trigger = trigger;
            Key = key;
        }

        public RegionInputAction Action { get; }
        public RegionInputTrigger Trigger { get; }
        public KeyCode Key { get; }

        public bool Equals(RegionInputBinding other)
        {
            return Action == other.Action && Trigger == other.Trigger && Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return obj is RegionInputBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Action * 397) ^ ((int)Trigger * 31) ^ (int)Key;
            }
        }

        public static bool operator ==(RegionInputBinding left, RegionInputBinding right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RegionInputBinding left, RegionInputBinding right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Immutable manifest of action bindings; the single source of truth for key display and
    /// device reads. Created through factories, never mutated after construction.
    /// </summary>
    public sealed class RegionInputBindingSet
    {
        private readonly RegionInputBinding[] _bindings;

        private RegionInputBindingSet(RegionInputBinding[] bindings)
        {
            _bindings = bindings;
        }

        public static RegionInputBindingSet CreateDefault()
        {
            return Create(new[]
            {
                new RegionInputBinding(RegionInputAction.PanForward, RegionInputTrigger.Hold, KeyCode.W),
                new RegionInputBinding(RegionInputAction.PanBack, RegionInputTrigger.Hold, KeyCode.S),
                new RegionInputBinding(RegionInputAction.PanLeft, RegionInputTrigger.Hold, KeyCode.A),
                new RegionInputBinding(RegionInputAction.PanRight, RegionInputTrigger.Hold, KeyCode.D),
                new RegionInputBinding(RegionInputAction.Orbit, RegionInputTrigger.Hold, KeyCode.Mouse1),
                new RegionInputBinding(RegionInputAction.Select, RegionInputTrigger.Press, KeyCode.Mouse0),
                new RegionInputBinding(RegionInputAction.Focus, RegionInputTrigger.Press, KeyCode.F),
                new RegionInputBinding(RegionInputAction.Rotate, RegionInputTrigger.Press, KeyCode.R),
                new RegionInputBinding(RegionInputAction.Cancel, RegionInputTrigger.Press, KeyCode.Escape),
            });
        }

        /// <summary>Validates and orders a custom manifest; gaps are allowed, duplicates are not.</summary>

        public static RegionInputBindingSet Create(IEnumerable<RegionInputBinding> bindings)
        {
            if (bindings == null) throw new ArgumentNullException(nameof(bindings));
            var unique = new Dictionary<RegionInputAction, RegionInputBinding>();
            var keys = new HashSet<KeyCode>();
            foreach (RegionInputBinding binding in bindings)
            {
                if (!Enum.IsDefined(typeof(RegionInputAction), binding.Action))
                    throw new ArgumentException("Binding action is undefined.", nameof(bindings));
                if (!Enum.IsDefined(typeof(RegionInputTrigger), binding.Trigger))
                    throw new ArgumentException("Binding trigger is undefined.", nameof(bindings));
                if ((int)binding.Key < 0)
                    throw new ArgumentException("Binding key is undefined.", nameof(bindings));
                if (unique.ContainsKey(binding.Action))
                    throw new ArgumentException($"Action {binding.Action} is bound more than once.", nameof(bindings));
                if (!keys.Add(binding.Key))
                    throw new ArgumentException($"Key {binding.Key} is bound to more than one action.", nameof(bindings));
                unique.Add(binding.Action, binding);
            }
            var ordered = new List<RegionInputBinding>(unique.Values);
            ordered.Sort((left, right) => ((int)left.Action).CompareTo((int)right.Action));
            return new RegionInputBindingSet(ordered.ToArray());
        }

        public IReadOnlyList<RegionInputBinding> Bindings => _bindings;

        public bool TryGet(RegionInputAction action, out RegionInputBinding binding)
        {
            for (int index = 0; index < _bindings.Length; index++)
            {
                if (_bindings[index].Action != action) continue;
                binding = _bindings[index];
                return true;
            }

            binding = default;
            return false;
        }
    }
}