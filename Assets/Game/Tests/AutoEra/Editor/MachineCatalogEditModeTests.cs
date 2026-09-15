using System;
using System.IO;
using System.Linq;
using AutoEra.DataTable;
using AutoEra.Machines;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AutoEra.World.Identity;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineCatalogEditModeTests
    {
        [TestCase(1)]
        [TestCase(2)]
        public void ApprovedFixedCarrierAndCargo_InstantiateAndPreserveSingleContainer(int level)
        {
            var catalog = new MachineCatalog(Machines(), Components(), key => true, key => key);
            Assert.That(catalog.TryGetMachine(10020 + level, out var carrier), Is.True);
            Assert.That(catalog.TryGetComponent(22060 + level, out var cargo), Is.True);
            Assert.That(carrier.MaximumIntegrity, Is.EqualTo(level == 1 ? 100 : 120));
            Assert.That(carrier.BaseCapacity, Is.EqualTo(level == 1 ? 10 : 12));
            Assert.That(carrier.CanMove, Is.False);
            Assert.That(carrier.SensorSlots, Is.EqualTo(2));
            Assert.That(carrier.CoreSlots, Is.EqualTo(1));
            Assert.That(carrier.EffectorSlots, Is.EqualTo(1));
            Assert.That(cargo.AddedCapacity, Is.EqualTo(level == 1 ? 30 : 36));
            Assert.That(cargo.HasBehavior, Is.False);
            var carrierRow = Machines().Single(row => row.Id == 10020 + level);
            var cargoRow = Components().Single(row => row.Id == 22060 + level);
            Assert.That(carrierRow.IdlePower, Is.EqualTo(0.1));
            Assert.That(carrierRow.WorkingPower, Is.EqualTo(level == 1 ? 1 : 1.2));
            Assert.That(carrierRow.PurchasePrice, Is.EqualTo(350));
            Assert.That(carrierRow.RecyclePrice, Is.EqualTo(280));
            Assert.That(cargoRow.IdlePower, Is.Zero);
            Assert.That(cargoRow.WorkingPower, Is.Zero);
            Assert.That(cargoRow.PurchasePrice, Is.EqualTo(400));
            Assert.That(cargoRow.RecyclePrice, Is.EqualTo(320));

            var ids = new PersistentIdAllocator();
            var registry = new PersistentObjectRegistry(ids);
            using (var roster = new MachineRoster(ids, registry))
            {
                var machine = roster.Create(carrier);
                var item = roster.CreateComponent(cargo);
                Assert.That(roster.Install(machine.Id, ManagementOrigin.Library, item.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(machine.TotalCapacity, Is.EqualTo(level == 1 ? 40 : 48));
                Assert.That(item.OwnerId, Is.EqualTo(machine.Id));
                machine.UpdateContainerUsage(carrier.BaseCapacity + 1);
                Assert.That(roster.Remove(machine.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.CapacityInUse));
                machine.UpdateContainerUsage(0);
                Assert.That(roster.Remove(machine.Id, ManagementOrigin.Library, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(machine.TotalCapacity, Is.EqualTo(carrier.BaseCapacity));
            }

            var scene = EditorSceneManager.NewPreviewScene();
            try
            {
                foreach (int rowId in new[] { 10020 + level, 22060 + level })
                {
                    Assert.That(catalog.TryGetReadyPrefab(rowId, out string path), Is.True);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/" + path + ".prefab");
                    Assert.That(prefab, Is.Not.Null, path);
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    Assert.That(instance, Is.Not.Null, path);
                    Assert.That(instance.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), path);
                    foreach (var component in instance.GetComponentsInChildren<Component>(true))
                    {
                        Assert.That(component, Is.Not.Null, "Missing script: " + path);
                        using (var serialized = new SerializedObject(component))
                        {
                            var property = serialized.GetIterator();
                            while (property.Next(true))
                                if (property.propertyType == SerializedPropertyType.ObjectReference)
                                    Assert.That(property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0,
                                        Is.True, path + ":" + property.propertyPath);
                        }
                    }
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static MachineDefinitions[] Machines() => File.ReadAllLines("Assets/Game/DataTable/Machines/MachineDefinitions.txt")
            .Skip(4).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => { var row = new MachineDefinitions(); row.ParseDataRow(s, null); return row; }).ToArray();
        private static ComponentDefinitions[] Components() => File.ReadAllLines("Assets/Game/DataTable/Machines/ComponentDefinitions.txt")
            .Skip(4).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => { var row = new ComponentDefinitions(); row.ParseDataRow(s, null); return row; }).ToArray();

        [Test]
        public void ResourceCatalog_ValidatesAllGeneratedMetadataAndRejectsDuplicateIdentity()
        {
            var machines = new MachineCatalog(Machines(), Components(), key => true, key => key);
            var rows = File.ReadAllLines("Assets/Game/DataTable/Catalog/FirstVersionObjects.txt").Skip(4)
                .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => { var row = new FirstVersionObjects(); row.ParseDataRow(s, null); return row; }).ToArray();
            var catalog = new FirstVersionCatalog(rows, machines);
            Assert.That(catalog.Count, Is.EqualTo(40));
            Assert.Throws<FormatException>(() => new FirstVersionCatalog(rows.Concat(new[] { rows[0] }), machines));
            foreach (var row in rows)
                if (!string.IsNullOrEmpty(row.Prefab)) Assert.That(AssetDatabase.LoadMainAssetAtPath("Assets/Game/Prefabs/Entity/" + row.Prefab + ".prefab"), Is.Not.Null, row.Prefab);
        }

        [Test]
        public void GeneratedRows_LoadAndOnlyReadyAssetsResolve()
        {
            var catalog = new MachineCatalog(Machines(), Components(), key => true, key => key);
            Assert.That(catalog.TryGetMachine(10011, out var machine), Is.True);
            Assert.That(machine.BaseCapacity, Is.EqualTo(30));
            Assert.That(catalog.TryGetComponent(20011, out var core), Is.True);
            Assert.That(core.ComputeCapacity, Is.EqualTo(50));
            foreach (var row in catalog.Availability)
            {
                if (!catalog.TryGetReadyPrefab(row.Key, out var path)) continue;
                Assert.That(AssetDatabase.LoadMainAssetAtPath("Assets/Game/Prefabs/Entity/" + path + ".prefab"), Is.Not.Null, path);
            }
        }

        [Test]
        public void Configuration_RejectsDuplicateRowsAndMissingLocalizedName()
        {
            var machines = Machines(); var components = Components();
            Assert.Throws<FormatException>(() => new MachineCatalog(machines.Concat(new[] { machines[0] }), components, key => true, key => key));
            Assert.Throws<FormatException>(() => new MachineCatalog(machines, components, key => false, key => key));
        }

        [TestCase("Id", "10010")]
        [TestCase("ModelId", "1002")]
        [TestCase("Level", "3")]
        [TestCase("Availability", "InvalidAvailability")]
        [TestCase("IdlePower", "NaN")]
        [TestCase("WorkingPower", "-1")]
        [TestCase("PurchasePrice", "-1")]
        [TestCase("Prefab", "../Outside")]
        public void Configuration_RejectsInvalidMachineFieldsByHeaderName(string fieldName, string value)
        {
            var invalid = ParseMachine(MutateRow("Assets/Game/DataTable/Machines/MachineDefinitions.txt", fieldName, value));
            Assert.Throws<FormatException>(() => new MachineCatalog(new[] { invalid }, Components(), key => true, key => key));
        }

        [Test]
        public void Configuration_RejectsInvalidComponentKind()
        {
            var invalid = ParseComponent(MutateRow("Assets/Game/DataTable/Machines/ComponentDefinitions.txt", "Kind", "InvalidKind"));
            Assert.Throws<FormatException>(() => new MachineCatalog(Machines(), new[] { invalid }, key => true, key => key));
        }

        [Test]
        public void ResourceCatalog_RejectsReadyWithoutPrefabOrUnsafePrefabPath()
        {
            var rows = File.ReadAllLines("Assets/Game/DataTable/Catalog/FirstVersionObjects.txt").Skip(4)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var machines = new MachineCatalog(Machines(), Components(), key => true, key => key);

            var header = File.ReadAllLines("Assets/Game/DataTable/Catalog/FirstVersionObjects.txt")[1];
            var missingPrefab = ParseResource(MutateRow(rows[0], header, "ResourceState", "Ready"));
            Assert.Throws<FormatException>(() => new FirstVersionCatalog(new[] { missingPrefab }, machines));

            var unsafePrefab = ParseResource(MutateRow(MutateRow(rows[0], header, "ResourceState", "Ready"), header, "Prefab", "../Outside"));
            Assert.Throws<FormatException>(() => new FirstVersionCatalog(new[] { unsafePrefab }, machines));
        }

        [Test]
        public void Configuration_RejectsReadyMachineWithoutPrefab()
        {
            var path = "Assets/Game/DataTable/Machines/MachineDefinitions.txt";
            var lines = File.ReadAllLines(path);
            var source = lines.Skip(4).First(s => !string.IsNullOrWhiteSpace(s));
            Assert.That(MutateRow(source, lines[1], "Availability", "__probe__"), Does.Contain("__probe__"));
            Assert.That(ParseMachine(source).Availability, Is.EqualTo("Ready"));
            var row = ParseMachine(MutateRow(path, "Prefab", ""));
            Assert.Throws<FormatException>(() => new MachineCatalog(new[] { row }, Components(), key => true, key => key));
        }

        private static MachineDefinitions ParseMachine(string row)
        {
            var result = new MachineDefinitions();
            result.ParseDataRow(row, null);
            return result;
        }

        private static ComponentDefinitions ParseComponent(string row)
        {
            var result = new ComponentDefinitions();
            result.ParseDataRow(row, null);
            return result;
        }

        private static FirstVersionObjects ParseResource(string row)
        {
            var result = new FirstVersionObjects();
            result.ParseDataRow(row, null);
            return result;
        }

        private static string MutateRow(string path, string fieldName, string value)
        {
            var lines = File.ReadAllLines(path);
            return MutateRow(lines.Skip(4).First(s => !string.IsNullOrWhiteSpace(s)), lines[1], fieldName, value);
        }

        private static string MutateRow(string row, string header, string fieldName, string value)
        {
            var headers = header.Split('\t');
            var values = row.Split('\t');
            var fieldIndex = Array.IndexOf(headers, fieldName);
            Assert.That(fieldIndex, Is.GreaterThanOrEqualTo(0), fieldName);
            values[fieldIndex] = value;
            return string.Join("\t", values);
        }
    }
}
