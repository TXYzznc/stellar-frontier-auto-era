using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>
    /// Serialized identity for objects generated from a FunctionalRigContract.
    /// It lets an Editor rebuild reuse the same object instead of creating a duplicate.
    /// </summary>
    public sealed class FunctionalRigPrototypeStableId : MonoBehaviour
    {
        [SerializeField] private string _stableId;

        public string StableId => _stableId;

        public void Configure(string stableId)
        {
            _stableId = stableId;
        }
    }
}
