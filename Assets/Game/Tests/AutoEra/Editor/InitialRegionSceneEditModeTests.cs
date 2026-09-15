using System.Collections.Generic;
using System.Linq;
using AutoEra.Input;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace AutoEra.Tests.Editor
{
    public sealed class InitialRegionSceneEditModeTests
    {
        [Test]
        public void SavedRegion_RegistersAndReleasesActualViews_AndSelectionIsIsolated()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene("Assets/Game/Scene/InitialRegion.unity", OpenSceneMode.Additive);
            try
            {
                InitialRegionScene entry = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<InitialRegionScene>(true)).Single();
                using (var session = new AutoEraWorldSessionFactory().Create(0))
                {
                    entry.Initialize(session);
                    Assert.That(entry.Region.Count, Is.EqualTo(7));
                    Assert.That(session.ObjectRegistry.Count, Is.EqualTo(7));
                    RegionInputModule input = entry.GetComponent<RegionInputModule>();
                    Camera camera = entry.GetComponent<RegionCameraController>().ViewCamera;
                    var source = new FixedSource();
                    input.SetSource(source);
                    var first = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<RegionObjectView>(true)).First(v => v.Model != null);
                    Assert.That(input.SelectTarget(first), Is.True);
                    Assert.That(first.IsHighlighted, Is.True);
                    input.SelectTarget(null);
                    Assert.That(first.IsHighlighted, Is.False);
                    Vector3 before = camera.transform.position;
                    input.Tick(1, true);
                    Assert.That(camera.transform.position, Is.EqualTo(before), "UI must block camera movement.");
                    input.Tick(1, false);
                    Assert.That(camera.transform.position, Is.Not.EqualTo(before));
                    Assert.That(source.Reads, Is.EqualTo(2));
                    entry.Advance(.5);
                    Assert.That(session.Clock.WorldMilliseconds, Is.EqualTo(500));
                    entry.Release();
                    Assert.That(session.ObjectRegistry.Count, Is.Zero);
                    entry.Initialize(session);
                    Assert.That(entry.Region.Objects.All(o => o.Id.Value > 7), Is.True);
                    entry.Release();
                }
                int layer = LayerMask.NameToLayer("RegionSelection");
                Assert.That(layer, Is.GreaterThanOrEqualTo(8));
                for (int other = 0; other < 32; other++) Assert.That(Physics.GetIgnoreLayerCollision(layer, other), Is.True);
                var sources = new List<NavMeshBuildSource>();
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Collider c in root.GetComponentsInChildren<Collider>(true).Where(c => c.gameObject.layer == layer))
                        Assert.That(c.isTrigger, Is.True);
                    NavMeshBuilder.CollectSources(root.transform, ~(1 << layer), NavMeshCollectGeometry.PhysicsColliders, 0,
                        new List<NavMeshBuildMarkup>(), sources);
                    Assert.That(sources.All(s => s.component == null || s.component.gameObject.layer != layer), Is.True);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private sealed class FixedSource : IRegionInputSource
        {
            public int Reads;
            public RegionInputFrame Read() { Reads++; return new RegionInputFrame { Pan = Vector2.right }; }
        }
    }
}
