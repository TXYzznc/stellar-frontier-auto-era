using System;
using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>
    /// Explicit structural boundary between product logic, functional rig visuals and
    /// gameplay-authoritative collision. It contains references only and owns no motion tick.
    /// </summary>
    public sealed class FunctionalRigPrototypeHierarchy : MonoBehaviour
    {
        [SerializeField] private Transform _logicRoot;
        [SerializeField] private Transform _rigRoot;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _authorityCollisionRoot;

        public Transform LogicRoot => _logicRoot;

        public Transform RigRoot => _rigRoot;

        public Transform VisualRoot => _visualRoot;

        public Transform AuthorityCollisionRoot => _authorityCollisionRoot;

        public void Configure(Transform logicRoot, Transform rigRoot, Transform visualRoot, Transform authorityCollisionRoot)
        {
            _logicRoot = logicRoot;
            _rigRoot = rigRoot;
            _visualRoot = visualRoot;
            _authorityCollisionRoot = authorityCollisionRoot;
        }

        public bool TryValidate(out string error)
        {
            if (_logicRoot == null || _rigRoot == null || _visualRoot == null || _authorityCollisionRoot == null)
            {
                error = "Prototype hierarchy requires logic, rig, visual and authority-collision roots.";
                return false;
            }

            if (_logicRoot.parent != transform || _rigRoot.parent != transform || _authorityCollisionRoot.parent != transform)
            {
                error = "Logic, rig and authority-collision roots must be direct siblings under the prototype root.";
                return false;
            }

            if (_visualRoot.parent != _rigRoot)
            {
                error = "Visual root must be a direct child of the rig root.";
                return false;
            }

            if (_authorityCollisionRoot.GetComponent<Collider>() == null)
            {
                error = "Authority-collision root requires a Collider.";
                return false;
            }

            error = null;
            return true;
        }

        private void OnValidate()
        {
            if (!TryValidate(out string error))
            {
                Debug.LogWarning("[AutoEra.Motion] PrototypeHierarchy Validation Failed: " + error, this);
            }
        }
    }
}
