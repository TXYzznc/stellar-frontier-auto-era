using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmInstanceEditModeTests
    {
        private sealed class Sink : IAlgorithmCommandSink
        { public bool Safe=true; public bool IsSafe=>Safe; public ulong Submit(AlgorithmTrigger t,AlgorithmIntent i)=>t.TaskId; public void EndBatch(AlgorithmTrigger t){} public void Cancel(){} public bool TryReadCargo(string f,string i2,out AlgorithmValue v){v=null;return false;} public bool TryQueryTask(string n,out AlgorithmValue t){t=null;return false;} }
        private static AlgorithmRuntime Runtime(ulong id,MachineComputePool pool,Sink sink)
        {
            var g=AlgorithmExecutionEditModeTests.Graph(); g.DocumentId=id; g.Nodes[1].Kind=AlgorithmNodeKind.Parameter;
            Assert.That(AlgorithmValidator.TryCompile(g,100,out var p,out _),Is.True);
            return new AlgorithmRuntime(new PersistentId(id),p,pool,sink);
        }
        [Test]
        public void DraftAndPendingAreIsolated_StaleAndHardwareRevisionReject()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);var sink=new Sink();ulong hardware=1;
            using(var service=new AlgorithmInstanceService(ids,pool,()=>hardware,d=>true))
            {
                var r=Runtime(100,pool,sink);Assert.That(service.Add(r),Is.True);
                var draft=service.ReadDraft(100);draft.Nodes[1].Default.Number=20;
                Assert.That(r.CopyApplied().Nodes[1].Default.Number,Is.EqualTo(12));
                Assert.That(service.Edit(100,1,draft),Is.True);sink.Safe=false;
                Assert.That(service.Apply(100,2,1,1,out var request),Is.True);service.Pump(0);
                Assert.That(service.ReadRequest(100).State,Is.EqualTo(AlgorithmApplyState.WaitingSafePoint));
                Assert.That(service.Edit(100,2,service.ReadDraft(100)),Is.True);sink.Safe=true;service.Pump(1);
                Assert.That(service.ReadRequest(100).Reason,Is.EqualTo("StaleRevision"));Assert.That(r.Revision,Is.EqualTo(1));
                Assert.That(service.Apply(100,3,1,1,out request),Is.True);hardware=2;service.Pump(2);
                Assert.That(service.ReadRequest(100).Reason,Is.EqualTo("HardwareOrBindingChanged"));
            }
            Assert.That(pool.AppliedLogicCost,Is.Zero);
        }
        [Test]
        public void ParametersPreserveState_StructurePausesPeersAndResetsOnlyTarget()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var a=Runtime(100,pool,new Sink());var b=Runtime(200,pool,new Sink());service.Add(a);service.Add(b);
                a.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });b.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });service.Pump(0);
                var d=service.ReadDraft(100);d.Nodes[1].Default.Number=20;service.Edit(100,1,d);service.Apply(100,2,1,1,out _);
                service.Pump(1);Assert.That(b.Paused,Is.False);service.Pump(2);
                Assert.That(a.CopyState()["counter"].Number,Is.EqualTo(12));Assert.That(a.Revision,Is.EqualTo(2));
                // Consume the deliberate Startup event before requesting the next change.
                service.Pump(3);
                d=service.ReadDraft(100);d.Nodes[2].StateKey="newCounter";service.Edit(100,2,d);service.Apply(100,3,2,1,out _);
                service.Pump(4);Assert.That(a.Paused&&b.Paused,Is.True);service.Pump(5);
                Assert.That(a.CopyState(),Is.Empty);Assert.That(b.CopyState()["counter"].Number,Is.EqualTo(12));Assert.That(b.Paused,Is.False);
            }
        }
        [Test]
        public void CheckpointPreservesPendingRequestAndState_WithoutReplayingCompletedRoot()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 });service.Pump(0);
                service.Edit(100,1,service.ReadDraft(100));service.SaveDraft(100,2);service.Apply(100,2,1,1,out var request);
                Assert.That(service.Capture(100,0,out var checkpoint),Is.True);
                service.CancelApply(100,request.RequestId);Assert.That(service.Restore(100,checkpoint,1000),Is.True);
                Assert.That(r.History().Length,Is.EqualTo(1));Assert.That(r.WaitingCount,Is.Zero);
                Assert.That(service.ReadRequest(100).RequestId,Is.EqualTo(request.RequestId));Assert.That(service.SavedDraftRevision(100),Is.EqualTo(2));
                Assert.That(r.Enqueue(new AlgorithmTrigger { NodeId=1,Revision=1,Generation=1 }),Is.False);
                service.Pump(1000);service.Pump(1001);Assert.That(r.Revision,Is.EqualTo(2));
            }
        }
        [Test]
        public void TemplatesStripBindings_AreIndependent_AndSystemTemplatesCannotBeDeleted()
        {
            var library=new AlgorithmTemplateLibrary(new PersistentIdAllocator());var g=AlgorithmExecutionEditModeTests.Graph();
            g.Bindings.Add(new AlgorithmBinding { Key="sensor",ComponentId=90,TargetId=91,Generation=2,Type=AlgorithmType.Of(AlgorithmValueKind.Number) });
            ulong id=library.Save("System",g,true);Assert.That(id,Is.Not.Zero);Assert.That(library.Delete(id,1),Is.False);Assert.That(library.Rename(id,1,"new"),Is.False);
            var a=library.Instantiate(id);var b=library.Instantiate(id);Assert.That(a.DocumentId,Is.Not.EqualTo(b.DocumentId));
            Assert.That(a.Nodes[0].Id,Is.Not.EqualTo(b.Nodes[0].Id));Assert.That(a.Bindings,Is.Empty);
            a.Nodes[1].Default.Number=999;Assert.That(b.Nodes[1].Default.Number,Is.EqualTo(12));
            ulong player=library.CopyAsPlayer(id,"Player");Assert.That(player,Is.Not.Zero);Assert.That(library.List().Length,Is.EqualTo(2));Assert.That(library.Delete(player,1),Is.True);
        }
        [Test]
        public void WarningsRequireExplicitConfirmation_AndDoNotInvalidateOldPlan()
        {
            var ids=new PersistentIdAllocator();var pool=new MachineComputePool(ids,100,100);
            using(var service=new AlgorithmInstanceService(ids,pool,()=>1,d=>true))
            {
                var r=Runtime(100,pool,new Sink());service.Add(r);var d=service.ReadDraft(100);
                d.Nodes.Add(new AlgorithmNode { Id=9,Kind=AlgorithmNodeKind.Input,BindingKey="sensor" });
                d.Bindings.Add(new AlgorithmBinding { Key="sensor",ComponentId=80,TargetId=81,Generation=1,Available=false,Type=AlgorithmType.Of(AlgorithmValueKind.Number) });
                service.Edit(100,1,d);Assert.That(service.Apply(100,2,1,1,out var request),Is.True);
                Assert.That(request.State,Is.EqualTo(AlgorithmApplyState.AwaitingWarningConfirmation));service.Pump(0);Assert.That(r.Revision,Is.EqualTo(1));
                Assert.That(service.ConfirmWarnings(100,request.RequestId),Is.True);service.Pump(1);service.Pump(2);Assert.That(r.Revision,Is.EqualTo(2));
            }
        }
    }
}
