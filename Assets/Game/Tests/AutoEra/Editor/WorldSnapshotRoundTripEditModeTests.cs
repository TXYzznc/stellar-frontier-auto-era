using System.Collections.Generic;
using AutoEra.Save;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.World.Identity;
using UnityEngine;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldSnapshotRoundTripEditModeTests
    {
        private static readonly Dictionary<string,int> Required = new Dictionary<string,int> { { "machines",1 }, { "algorithms",1 } };
        private sealed class MachineData { public ulong Id { get; set; } public string Name { get; set; } }
        private sealed class AlgorithmData { public ulong PendingTask { get; set; } }
        private sealed class ValueData
        {
            public Vector3 Position { get; set; }
            public PersistentId Identity { get; set; }
            public CorrelationId Correlation { get; set; }
            public PersistentObjectReference Reference { get; set; }
        }
        private static string Content() => WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1234,ulong.MaxValue,7,"Fixture",
            new[] { new WorldSnapshotSection("machines",1,new { id=42UL, name="原机器" }),new WorldSnapshotSection("algorithms",1,new { pendingTask=42UL }) }));
        [Test] public void VersionedDirectory_RoundTripsTimeIdentityAndSections()
        {
            Assert.That(WorldSnapshotCodec.TryRead(Content(),Required,out var snapshot,out var reason),Is.True,reason);
            Assert.That(snapshot.WorldMilliseconds,Is.EqualTo(1234)); Assert.That(snapshot.AllocatedThrough,Is.EqualTo(ulong.MaxValue));
            Assert.That(snapshot.Revision,Is.EqualTo(7));
            Assert.That(snapshot.TryReadSection<MachineData>("machines",out var machine,out _),Is.True); Assert.That(machine.Id,Is.EqualTo(42));
            Assert.That(snapshot.TryReadSection<AlgorithmData>("algorithms",out var algorithm,out _),Is.True); Assert.That(algorithm.PendingTask,Is.EqualTo(42));
            Assert.That(Content(),Does.Not.Contain("$type"));
        }
        [Test] public void MissingRequiredSection_IsRejectedInsteadOfCreatingEmptyWorld()
        {
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1234,42,7,"Fixture",new[] {new WorldSnapshotSection("machines",1,new { id=42UL })}));
            Assert.That(WorldSnapshotCodec.TryRead(content,Required,out var snapshot,out var reason),Is.False);
            Assert.That(snapshot,Is.Null); Assert.That(reason,Does.Contain("algorithms"));
        }
        [TestCase(2)] [TestCase(0)] public void UnknownWorldVersion_IsRejected(int version)
        { string content=Content().Replace("\"worldSchemaVersion\":1","\"worldSchemaVersion\":"+version); Assert.That(WorldSnapshotCodec.TryRead(content,Required,out _,out _),Is.False); }
        [Test] public void UnknownDomainVersion_IsRejected()
        { string content=Content().Replace("\"name\":\"machines\",\"version\":1","\"name\":\"machines\",\"version\":2"); Assert.That(WorldSnapshotCodec.TryRead(content,Required,out _,out var reason),Is.False); Assert.That(reason,Does.Contain("machines")); }
        [Test] public void DuplicateSections_AreRejected()
        { string content=Content().Replace("\"name\":\"algorithms\"","\"name\":\"machines\""); Assert.That(WorldSnapshotCodec.TryRead(content,Required,out _,out _),Is.False); }
        [Test] public void DuplicateHeaderProperties_AreRejected()
        { Assert.That(WorldSnapshotCodec.TryRead(Content().Insert(1,"\"worldSchemaVersion\":1,"),Required,out _,out _),Is.False); }
        [TestCase("{}")] [TestCase("")] [TestCase("{broken")] public void OldEmptyOrMalformedContent_IsNotAWorld(string json)
        { Assert.That(WorldSnapshotCodec.TryRead(json,Required,out _,out var reason),Is.False); Assert.That(reason,Is.Not.Empty); }
        [Test] public void InvalidTimeAndMalformedSummary_AreRejected()
        {
            Assert.That(WorldSnapshotCodec.TryRead(Content().Replace("\"worldMilliseconds\":1234","\"worldMilliseconds\":-1"),Required,out _,out _),Is.False);
            Assert.That(WorldSnapshotCodec.TryRead(Content().Replace("\"summary\":\"Fixture\"","\"summary\":{}"),Required,out _,out _),Is.False);
        }
        [Test] public void ClosedValues_PreserveFullIdentityRangeAndMissingReferenceWithoutUnityTraversal()
        {
            var original=new ValueData { Position=new Vector3(2,.15f,-2.4f),Identity=new PersistentId(ulong.MaxValue),Correlation=new CorrelationId(ulong.MaxValue),Reference=new PersistentObjectReference(new PersistentId(777),PersistentObjectKind.ResourcePoint) };
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1,ulong.MaxValue,1,"Fixture",new[] {new WorldSnapshotSection("values",1,original)}));
            Assert.That(content,Does.Not.Contain("normalized"));Assert.That(content,Does.Not.Contain("IsValid"));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"values",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<ValueData>("values",out var restored,out reason),Is.True,reason);
            Assert.That(restored.Position,Is.EqualTo(original.Position));Assert.That(restored.Identity,Is.EqualTo(original.Identity));Assert.That(restored.Correlation,Is.EqualTo(original.Correlation));Assert.That(restored.Reference,Is.EqualTo(original.Reference));
            var registry=new PersistentObjectRegistry(new PersistentIdAllocator());
            Assert.That(restored.Reference.TryResolve(registry,out _),Is.EqualTo(PersistentRegistryResult.Missing));
        }
        [Test] public void MissingCoordinate_IsRejectedInsteadOfDefaultPosition()
        {
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1,1,1,"Fixture",new[] {new WorldSnapshotSection("values",1,new ValueData {Position=Vector3.one})}));
            content=content.Replace(",\"z\":1.0",string.Empty);
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"values",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<ValueData>("values",out _,out reason),Is.False);Assert.That(reason,Is.Not.Empty);
        }
        [Test] public void NonFinitePosition_CannotBecomeAFormalSnapshot()
        {
            var snapshot=new WorldSnapshotDocument(1,1,1,"Fixture",new[] {new WorldSnapshotSection("values",1,new ValueData {Position=new Vector3(float.NaN,0,0)})});
            Assert.That(()=>WorldSnapshotCodec.Serialize(snapshot),Throws.Exception);
        }
        [TestCase("Active")] [TestCase("Position")] public void MissingDomainField_IsRejectedInsteadOfImplicitIdleOrOrigin(string field)
        {
            string json=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1,1,1,"Fixture",new[] {new WorldSnapshotSection("navigation",1,new MachineNavigationSnapshot())}));
            string member=field=="Active" ? "\"Active\":false," : "\"Position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},";
            Assert.That(json,Does.Contain(member));json=json.Replace(member,string.Empty);
            Assert.That(WorldSnapshotCodec.TryRead(json,new Dictionary<string,int>{{"navigation",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<MachineNavigationSnapshot>("navigation",out _,out reason),Is.False);Assert.That(reason,Is.Not.Empty);
        }
    }
}
