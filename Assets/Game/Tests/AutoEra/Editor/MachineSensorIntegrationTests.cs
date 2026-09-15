using System.Collections;
using AutoEra.DataTable;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineSensorIntegrationTests
    {
        [UnityTest]
        public IEnumerator LoadedConfiguration_AndFormalRegionLifecycle()
        {
            const string launchPath = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launchPath).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launchPath, OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath(launchPath).isLoaded, Is.True, "Use saved Launch scene.");
            yield return new EnterPlayMode();
            yield return VerifyRuntime();
            yield return new ExitPlayMode();
        }
        private static IEnumerator VerifyRuntime()
        {
            double deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);
            var sensors = SensorCatalog.FromLoadedGameData();
            Assert.That(sensors.Count, Is.EqualTo(4));
            var components = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DataTableComponent>().GetDataTable<ComponentDefinitions>();
            foreach (int model in new[] { 2101, 2102, 2103, 2104 })
            {
                var first = components.GetDataRow(model * 10 + 1);
                var second = components.GetDataRow(model * 10 + 2);
                Assert.That(second.IdlePower, Is.EqualTo(first.IdlePower));
                Assert.That(second.WorkingPower, Is.EqualTo(first.WorkingPower));
            }
            Assert.That(sensors.TryGet(21031, out _), Is.False, "Do not enable exploration.");
            Assert.That(sensors.TryGet(21041, out _), Is.False, "Do not enable communication.");
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scene/InitialRegion.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            deadline = Time.realtimeSinceStartupAsDouble + 20;
            while (!loading.isDone && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(loading.isDone, Is.True);
            var scene = SceneManager.GetSceneByPath("Assets/Game/Scene/InitialRegion.unity");
            InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineExecutionContext context = null;
                RegionSensorEnvironment environment = null;
                RegionSensorReadProvider provider = null;
                var anchor = new GameObject("B14 test-only explicit sensor anchor");
                try
                {
                    bool ready = false; string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    deadline = Time.realtimeSinceStartupAsDouble + 20;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                    Assert.That(failure, Is.Null); Assert.That(ready, Is.True);
                    var catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out var definition), Is.True);
                    Assert.That(catalog.TryGetComponent(20011, out var coreDefinition), Is.True);
                    Assert.That(catalog.TryGetComponent(21012, out var sensorDefinition), Is.True);
                    var machine = session.Machines.Create(definition);
                    var core = session.Machines.CreateComponent(coreDefinition);
                    var component = session.Machines.CreateComponent(sensorDefinition);
                    session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                    session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0);
                    entry.Region.DeployMachine(machine.Id, new Vector2(20,-25), Vector2.one, out _);
                    machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true);
                    machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                    context = new MachineExecutionContext(machine, session.IdAllocator);
                    var target = entry.Region.Register(PersistentObjectKind.Building,"B14 explicit public fixture",new Vector2(30,30),Vector2.one);
                    provider = new RegionSensorReadProvider(target, p => new Vector3(30,0,30));
                    environment = new RegionSensorEnvironment(entry.Region); environment.Register(provider);
                    anchor.transform.position = new Vector3(29,0,30);
                    Assert.That(sensors.TryGet(21012,out var profile),Is.True);
                    var sensor = context.Sensors.Bind(component,profile,environment,new TransformSensorAnchor(anchor.transform));
                    sensor.Bind(provider.Target); entry.AttachSensors(context.Sensors);
                    entry.Advance(.01);
                    Assert.That(sensor.TryRead(out var sample),Is.True);
                    Assert.That(sample.Soil,Is.Null,"No fabricated agriculture provider.");
                    Assert.That(context.Compute.Used,Is.EqualTo(10));
                    entry.Release();
                    Assert.That(context.Compute.Used,Is.Zero); Assert.That(sensor.TryRead(out _),Is.False);
                }
                finally
                {
                    entry.Release(); context?.Dispose(); provider?.Dispose(); environment?.Dispose();
                    Object.Destroy(anchor);
                }
            }
        }
    }
}
