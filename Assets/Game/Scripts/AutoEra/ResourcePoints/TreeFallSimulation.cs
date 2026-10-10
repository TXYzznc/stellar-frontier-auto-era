using System;
using System.Collections.Generic;
using AutoEra.World.Region;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.ResourcePoints
{
    /// <summary>Isolated native physics containing only the authored ground and fallen upper trees.</summary>
    public sealed class TreeFallSimulation : IDisposable
    {
        private readonly Scene _scene;
        private readonly PhysicsScene _physics;
        private readonly List<Collider> _upperColliders = new List<Collider>();
        private bool _disposed;
        private double _pendingSeconds;
        public TreeFallSimulation(MeshFilter ground)
        {
            if (ground == null || ground.sharedMesh == null) throw new ArgumentException("Explicit ground required.");
            // Scene unloading is asynchronous; a restored candidate may coexist with the retiring world.
            _scene = SceneManager.CreateScene("AutoEra.TreeFalls."+Guid.NewGuid().ToString("N"), new CreateSceneParameters(LocalPhysicsMode.Physics3D)); _physics = _scene.GetPhysicsScene();
            var floor = new GameObject("ProductionGround"); SceneManager.MoveGameObjectToScene(floor, _scene);
            floor.transform.SetPositionAndRotation(ground.transform.position, ground.transform.rotation); floor.transform.localScale = ground.transform.lossyScale;
            floor.AddComponent<MeshCollider>().sharedMesh = ground.sharedMesh;
        }
        public Rigidbody Attach(GameObject upper, Bounds bounds, Vector3 fallDirection)
        {
            if (_disposed || upper == null) throw new ObjectDisposedException(nameof(TreeFallSimulation));
            upper.transform.SetParent(null, true); SceneManager.MoveGameObjectToScene(upper, _scene);
            var collider = upper.AddComponent<BoxCollider>(); collider.center = upper.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size; // upper root has unit world scale and an identity rotation at attachment.
            foreach (var other in _upperColliders) if (other != null) Physics.IgnoreCollision(collider, other, true);
            _upperColliders.Add(collider);
            var body = upper.AddComponent<Rigidbody>(); body.mass = 1; body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.centerOfMass = Vector3.zero; // Mesh/root rotation origin is the actual fracture surface.
            var hinge = upper.AddComponent<ConfigurableJoint>(); hinge.autoConfigureConnectedAnchor = false;
            hinge.anchor = Vector3.zero; hinge.connectedAnchor = upper.transform.position;
            hinge.xMotion = hinge.yMotion = hinge.zMotion = ConfigurableJointMotion.Locked;
            hinge.angularXMotion = hinge.angularYMotion = hinge.angularZMotion = ConfigurableJointMotion.Free;
            body.angularVelocity = Vector3.Cross(Vector3.up, fallDirection).normalized * 1.5f;
            return body;
        }
        public void Advance(double seconds)
        {
            if (_disposed || seconds <= 0) return;
            _pendingSeconds += seconds;
            // Large offline jumps use the domain timeout; never run an unbounded physics catch-up.
            if (_pendingSeconds > 1) _pendingSeconds = 1;
            while (_pendingSeconds >= .02) { _physics.Simulate(.02f); _pendingSeconds -= .02; }
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; _upperColliders.Clear();
            if (_scene.IsValid() && _scene.isLoaded) SceneManager.UnloadSceneAsync(_scene);
        }
    }
}
