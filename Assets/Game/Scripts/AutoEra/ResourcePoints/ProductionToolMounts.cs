using UnityEngine;

namespace AutoEra.ResourcePoints
{
    /// <summary>Explicit model installation anchors; no fallback or guessed socket at runtime.</summary>
    public sealed class ProductionToolMounts : MonoBehaviour
    {
        [SerializeField] private Transform[] _mounts;
        public Transform Read(int slot) => _mounts != null && slot >= 0 && slot < _mounts.Length ? _mounts[slot] : null;
        public void ConfigureForEditor(Transform[] mounts) { _mounts = mounts; }
    }
}
