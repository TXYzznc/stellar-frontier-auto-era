using System;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.Motion.Adapter;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionWorkBridgeEditModeTests
    {
        [Test]
        public void BridgeRequiresExplicitAuthorityInputs()
        {
            Assert.Throws<System.ArgumentNullException>(() => new MotionWorkBridge(null, null, null, "x", null));
        }

        [Test]
        public void WorkRequestPreservesAuthoritativeQueueResult()
        {
            MethodInfo method = typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.RequestWork));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(WorkRequestResult)));
        }

        [Test]
        public void WorkReleaseIsExplicitAndBoolean()
        {
            MethodInfo method = typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.ReleaseWork));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void BridgeExposesAuthorityLifecycleAndSamplingBoundaries()
        {
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Begin)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Sample)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Dispose)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetProperty(nameof(MotionWorkBridge.TaskId)), Is.Not.Null);
        }

        [Test]
        public void WorkState_TracksQueuePromotionWithoutManualReRequest()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-50, -50, 100, 100)))
            {
                RegionObject target = region.Register(PersistentObjectKind.ResourcePoint, "mine", Vector2.zero, Vector2.one);
                MachineInstance machine = session.Machines.Create(new MachineDefinition(1, "Fixture", 1, 1, 1, 1, 30, true, true, 100));
                var core = session.Machines.CreateComponent(new ComponentDefinition(2, HardwareKind.Core, 1, 0, 20, 100, false));
                session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                region.DeployMachine(machine.Id, new Vector2(10, 0), Vector2.one, out _);
                machine.Activate(ManagementOrigin.Field);
                machine.UpdateEnvironment(true, true);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                var context = new MachineExecutionContext(machine, session.IdAllocator);
                var navigation = new MachineNavigation(context, new Driver(), new MachineNavigationSettings());
                RegionObject other = region.Register(PersistentObjectKind.Machine, "owner", new Vector2(-8, 0), Vector2.one);

                GameObject rigGO = new GameObject("BridgeTestRig");
                MotionRig rig = rigGO.AddComponent<MotionRig>();
                var bindings = new System.Collections.Generic.List<MotionJointBinding>();
                string[] prefixes = { "front_left", "front_right", "rear_left", "rear_right" };
                foreach (string p in prefixes)
                {
                    foreach (string kind in new[] { "roll", "steer" })
                    {
                        var jt = new GameObject(p + "_" + kind).transform;
                        jt.SetParent(rigGO.transform, false);
                        bindings.Add(new MotionJointBinding(p + "_" + kind, jt, MotionJointChannel.Rotation,
                            Vector3.up, -90, 90, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero));
                    }
                }
                rig.Configure(bindings.ToArray());

                GameObject execGO = new GameObject("BridgeTestExecutor");
                MotionExecutor executor = execGO.AddComponent<MotionExecutor>();
                executor.Configure(rig);
                var adapter = new MachineNavigationMotionAdapter(rig, Vector3.zero, 0, 0.32f, 1.8f);
                var bridge = new MotionWorkBridge(navigation, executor, adapter, "bridge-test-joint", Array.Empty<string>());

                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1, -1, 2, 2)))
                {
                    queue.Request(other.Id, queue.WorkArea.center); // 他机成为持有者
                    Assert.That(bridge.RequestWork(queue, machine.Id, queue.WorkArea.center), Is.EqualTo(WorkRequestResult.Waiting));
                    Assert.That(bridge.WorkState, Is.EqualTo(WorkRequestState.Waiting));

                    queue.Release(other.Id); // 持有者释放 → 本机被队列唤醒
                    Assert.That(bridge.WorkState, Is.EqualTo(WorkRequestState.Granted),
                        "被唤醒后 WorkState 必须跟随权威队列变成 Granted，而不是停留等待快照。");

                    bridge.ReleaseWork(queue, machine.Id);
                    Assert.That(bridge.WorkState, Is.EqualTo(WorkRequestState.None));
                    Assert.That(queue.Owner.IsValid, Is.False);
                }

                bridge.Dispose();
                navigation.Dispose();
                context.Dispose();
                region.Dispose();
                UnityEngine.Object.DestroyImmediate(execGO);
                UnityEngine.Object.DestroyImmediate(rigGO);
            }
        }

        private sealed class Driver : IMachineNavigationDriver
        {
            public Vector3 Position { get; set; }
            public float Yaw { get; set; }
            public float Speed { get; set; }
            public bool PathValid { get; set; } = true;
            public bool TryPlan(MachineNavigationTarget target, MachineInstance machine, out Vector3 destination)
            {
                destination = target.Candidates[0];
                return target.Allows(machine, destination);
            }
            public void Resume() { }
            public void Stop() { Speed = 0; }
            public void Align(float yaw, float degreesPerSecond, float deltaSeconds) { }
        }
    }
}
