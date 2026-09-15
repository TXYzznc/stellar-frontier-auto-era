using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.Motion.Adapter;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineNavigationIntegrationTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab";
        private const string EvidencePath = "openspec/changes/b13-wheeled-navigation-and-avoidance/evidence";
        private static readonly Vector3 Origin = new Vector3(1000, 0, 1000);

        private sealed class World : IDisposable
        {
            public readonly AutoEraWorldSession Session = new AutoEraWorldSessionFactory().Create(0);
            public readonly InitialRegion Region;
            public readonly GameObject Root = new GameObject("B13 isolated navigation fixture");
            private readonly NavMeshData _data;
            private readonly NavMeshDataInstance _instance;
            private readonly Material _material;
            public World()
            {
                Region = new InitialRegion(Session, new Rect(980, 980, 40, 40));
                _material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _material.SetColor("_BaseColor", new Color(.22f, .28f, .3f));
                Box("Ground", Origin + new Vector3(0, -.25f, 0), new Vector3(40, .5f, 40));
                Box("Static obstacle", Origin + new Vector3(0, 1, 0), new Vector3(4, 2, 8));
                Region.Register(PersistentObjectKind.Building, "Obstacle", new Vector2(1000, 1000), new Vector2(4, 8));
                var sources = new List<NavMeshBuildSource>
                {
                    Source(Origin + new Vector3(0, -.25f, 0), new Vector3(40, .5f, 40), 0),
                    Source(Origin + new Vector3(0, 1, 0), new Vector3(4, 2, 8), 1)
                };
                var settings = NavMesh.GetSettingsByID(0);
                settings.agentRadius = 1.6f; settings.agentHeight = 2; settings.agentClimb = .3f;
                _data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Origin, new Vector3(44, 8, 44)), Vector3.zero, Quaternion.identity);
                Assert.That(_data, Is.Not.Null); _instance = NavMesh.AddNavMeshData(_data);
                Assert.That(_instance.valid, Is.True);
            }
            private static NavMeshBuildSource Source(Vector3 position, Vector3 size, int area)
                => new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, transform = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one), size = size, area = area };
            private void Box(string name, Vector3 position, Vector3 size)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name; box.transform.SetParent(Root.transform);
                box.transform.position = position; box.transform.localScale = size; box.GetComponent<Renderer>().sharedMaterial = _material;
                if (name == "Static obstacle")
                {
                    var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", new Color(.72f, .36f, .12f));
                    box.GetComponent<Renderer>().SetPropertyBlock(block);
                }
            }
            public void Capture(string filename)
            {
                var node = new GameObject("EvidenceCamera"); node.transform.SetParent(Root.transform);
                var camera = node.AddComponent<Camera>(); camera.transform.position = Origin + new Vector3(24, 30, -27);
                camera.transform.LookAt(Origin); camera.orthographic = true; camera.orthographicSize = 18;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.08f, .1f, .13f);
                var rt = new RenderTexture(1280, 720, 24); var previous = RenderTexture.active;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                try
                {
                    camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                    image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                    Directory.CreateDirectory(EvidencePath); File.WriteAllBytes(Path.Combine(EvidencePath, filename), image.EncodeToPNG());
                }
                finally { RenderTexture.active = previous; camera.targetTexture = null; Object.DestroyImmediate(image); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(node); }
            }
            public void Dispose()
            { _instance.Remove(); Object.DestroyImmediate(_data); Object.DestroyImmediate(Root); Object.DestroyImmediate(_material); Region.Dispose(); Session.Dispose(); }
        }
        private sealed class Carrier : IDisposable
        {
            public readonly GameObject View;
            public readonly MachineNavigation Navigation;
            public readonly MachineExecutionContext Context;
            public readonly MachineNavigationMotionAdapter Motion;
            public readonly NavMeshAgent Agent;
            public readonly MachineTaskRecord Task;
            private readonly World _world;
            public Carrier(World world, Vector3 local, int priority)
            {
                _world = world;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath); Assert.That(prefab, Is.Not.Null);
                View = Object.Instantiate(prefab, Origin + local, Quaternion.Euler(0, 90, 0), world.Root.transform);
                View.name = "Formal wheeled carrier " + priority;
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(View), Is.Zero);
                var machine = world.Session.Machines.Create(new MachineDefinition(1001, "Wheel", 1, 2, 2, 1, 30, true, true, 100));
                var core = world.Session.Machines.CreateComponent(new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));
                world.Session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                Assert.That(world.Region.DeployMachine(machine.Id, new Vector2(View.transform.position.x, View.transform.position.z), new Vector2(1.8f, 2.4f), out _, 90),
                    Is.EqualTo(RegionMachineDeploymentResult.Bound));
                machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Context = new MachineExecutionContext(machine, world.Session.IdAllocator);
                Context.Tasks.Submit("Navigation fixture", WorkPriority.Normal, out Task); Context.Tasks.StartNext();
                Agent = View.AddComponent<NavMeshAgent>();
                var settings = new MachineNavigationSettings();
                var driver = new UnityMachineNavigationDriver(Agent, settings, 1.6f, 2, priority);
                Assert.That(Agent.Warp(Origin + local), Is.True);
                Navigation = new MachineNavigation(Context, driver, settings);
                var rig = View.GetComponentInChildren<MotionRig>(); Assert.That(rig, Is.Not.Null); Assert.That(rig.TryValidate(out var error), Is.True, error);
                Motion = new MachineNavigationMotionAdapter(rig, View.transform.position, 90, .32f, 1.8f);
            }
            public void Start(Vector3 local, float yaw = 90)
                => Assert.That(Navigation.Start(Task.Id, new MachineNavigationTarget(_world.Region, Origin + local, yaw), Time.realtimeSinceStartupAsDouble), Is.EqualTo(NavigationAdmission.Accepted));
            public void Tick()
            { Navigation.Tick(Time.realtimeSinceStartupAsDouble, Time.unscaledDeltaTime); Motion.Sample(View.transform.position, View.transform.eulerAngles.y); }
            public void Dispose()
            { Navigation.Dispose(); Context.Tasks.CloseChain(Task.Id); Context.Dispose(); _world.Region.Remove(Context.Machine.Id); Object.DestroyImmediate(View); }
        }

        [UnityTest]
        public IEnumerator FormalCarrier_NavigatesAvoidsAlignsAndCancels_WithoutMutatingAssets()
        {
            yield return new EnterPlayMode();
            yield return VerifyRuntime();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyRuntime()
        {
            using (var world = new World())
            {
                float maxDetour = 0; long ticks = 0, allocated = 0; int samples = 0;
                using (var carrier = new Carrier(world, new Vector3(-10, 0, 0), 30))
                {
                    carrier.Start(new Vector3(10, 0, 0), 180); world.Capture("navigation-bound.png");
                    double until = Time.realtimeSinceStartupAsDouble + 45;
                    bool captured = false;
                    while (carrier.Navigation.IsActive && Time.realtimeSinceStartupAsDouble < until)
                    {
                        long memory = GC.GetAllocatedBytesForCurrentThread(), start = System.Diagnostics.Stopwatch.GetTimestamp();
                        carrier.Tick(); ticks += System.Diagnostics.Stopwatch.GetTimestamp() - start;
                        allocated += GC.GetAllocatedBytesForCurrentThread() - memory; samples++;
                        Vector3 p = carrier.View.transform.position - Origin;
                        maxDetour = Mathf.Max(maxDetour, Mathf.Abs(p.z));
                        Assert.That(Mathf.Abs(p.x) > 2 || Mathf.Abs(p.z) > 4, Is.True, "Actual carrier entered static obstacle.");
                        if (!captured && Mathf.Abs(p.z) > 4) { world.Capture("navigation-detour.png"); captured = true; }
                        yield return null;
                    }
                    Assert.That(carrier.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed), "Route did not complete: " + carrier.Navigation.State +
                        " position=" + carrier.View.transform.position + " remaining=" + carrier.Agent.remainingDistance +
                        " speed=" + carrier.Agent.velocity + " scale=" + Time.timeScale + " travel=" + carrier.Motion.TravelDistance + " plans=" + carrier.Navigation.PlanCount);
                    Assert.That(maxDetour, Is.GreaterThan(4)); Assert.That(carrier.Agent.velocity.magnitude, Is.LessThan(.03f));
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(carrier.View.transform.eulerAngles.y, 180)), Is.LessThanOrEqualTo(1));
                    Assert.That(Mathf.Abs(carrier.Motion.TravelDistance), Is.GreaterThan(20));
                    Assert.That(carrier.Motion.RollDegrees, Is.EqualTo(carrier.Motion.TravelDistance / (.32f * 2 * Mathf.PI) * 360).Within(.05f));
                    world.Capture("navigation-arrived.png");
                }
                float minimumSeparation = float.PositiveInfinity;
                using (var left = new Carrier(world, new Vector3(-8, 0, -10), 20))
                using (var right = new Carrier(world, new Vector3(8, 0, -9), 60))
                {
                    left.Start(new Vector3(12, 0, -10)); right.Start(new Vector3(-12, 0, -9), 270);
                    double until = Time.realtimeSinceStartupAsDouble + 25;
                    while ((left.Navigation.IsActive || right.Navigation.IsActive) && Time.realtimeSinceStartupAsDouble < until)
                    {
                        left.Tick(); right.Tick();
                        minimumSeparation = Mathf.Min(minimumSeparation, Vector3.Distance(left.View.transform.position, right.View.transform.position));
                        yield return null;
                    }
                    Assert.That(left.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                    Assert.That(right.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                    Assert.That(minimumSeparation, Is.GreaterThan(1.8f)); world.Capture("navigation-two-carriers.png");
                }
                using (var cancelled = new Carrier(world, new Vector3(-10, 0, 0), 30))
                {
                    cancelled.Start(new Vector3(10, 0, 0));
                    double until = Time.realtimeSinceStartupAsDouble + 1;
                    while (Time.realtimeSinceStartupAsDouble < until) { cancelled.Tick(); yield return null; }
                    Vector3 before = cancelled.View.transform.position; cancelled.Navigation.Cancel();
                    until = Time.realtimeSinceStartupAsDouble + .5;
                    while (Time.realtimeSinceStartupAsDouble < until) { cancelled.Tick(); yield return null; }
                    Assert.That(Vector3.Distance(before, cancelled.View.transform.position), Is.LessThan(.02f));
                    Assert.That(cancelled.Context.Compute.Used, Is.Zero); world.Capture("navigation-cancelled.png");
                }
                Directory.CreateDirectory(EvidencePath);
                File.WriteAllText(Path.Combine(EvidencePath, "playmode-metrics.json"),
                    JsonUtility.ToJson(new Metrics { Samples = samples, ManagedBytes = allocated,
                        TickMilliseconds = ticks * 1000d / System.Diagnostics.Stopwatch.Frequency,
                        MaxDetourMeters = maxDetour, MinimumTwoCarrierSeparation = minimumSeparation }, true));
            }
            yield return null;
            // Test Runner's default gate still fails on Error/Exception/Assert.
            // Do not fail on the editor service's ordinary domain-reload self-test Log messages.
        }
        [Serializable] private sealed class Metrics
        { public int Samples; public long ManagedBytes; public double TickMilliseconds; public float MaxDetourMeters; public float MinimumTwoCarrierSeparation; }
        [UnityTearDown] public IEnumerator Cleanup() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
    }
}
