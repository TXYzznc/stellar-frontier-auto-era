using System;
using AutoEra.Machines;
using System.Collections.Generic;
using AutoEra.Save;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineTaskSnapshotEditModeTests
    {
        [Test] public void PartialTask_RestoresSameIdAndOnlyFinishesOutstandingActivity()
        {
            var source=new MachineTaskQueue(new PersistentIdAllocator());
            source.Submit("transport",WorkPriority.Normal,out var task);source.TryStart(task.Id);
            source.AddActivity(task.Id);source.AddActivity(task.Id);source.CloseChain(task.Id);source.EndActivity(task.Id,false);
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(100,task.Id.Value,1,"Fixture",new[] {new WorldSnapshotSection("tasks",1,source.Capture())}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"tasks",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<MachineTaskQueueSnapshot>("tasks",out var snapshot,out reason),Is.True,reason);
            var ids=new PersistentIdAllocator();var restored=new MachineTaskQueue(ids);int ended=0;restored.Ended+=value=>ended++;
            restored.Restore(snapshot);Assert.That(ended,Is.Zero);Assert.That(restored.TryGet(task.Id,out var same),Is.True);Assert.That(same.State,Is.EqualTo(MachineTaskState.Running));
            restored.EndActivity(task.Id,false);Assert.That(ended,Is.EqualTo(1));Assert.That(restored.TryGet(task.Id,out _),Is.False);
            Assert.That(ids.TryAllocate(out var next),Is.True);Assert.That(next.Value,Is.GreaterThan(task.Id.Value));
        }
        [Test] public void PausedQueue_RestoresWaitReasonsAndPriorityFifo()
        {
            var source=new MachineTaskQueue(new PersistentIdAllocator());source.SetPaused(true,MachineWaitReason.Power);
            source.Submit("first",WorkPriority.Normal,out var first);source.Submit("urgent",WorkPriority.Urgent,out var urgent);source.Submit("second",WorkPriority.Normal,out var second);
            var restored=new MachineTaskQueue(new PersistentIdAllocator());restored.Restore(source.Capture());Assert.That(restored.StartNext(),Is.Null);
            Assert.That(restored.TryGet(first.Id,out var paused),Is.True);Assert.That(paused.WaitReasons,Is.EqualTo(MachineWaitReason.Power));
            restored.SetPaused(false,MachineWaitReason.Power);Assert.That(restored.StartNext().Id,Is.EqualTo(urgent.Id));Assert.That(restored.StartNext().Id,Is.EqualTo(first.Id));Assert.That(restored.StartNext().Id,Is.EqualTo(second.Id));
        }
        [Test] public void InvalidReference_IsRejectedBeforeMutatingCandidateOrAllocator()
        {
            var source=new MachineTaskQueue(new PersistentIdAllocator());source.Submit("queued",WorkPriority.Normal,out _);var snapshot=source.Capture();snapshot.WaitingOrder[0]=999;
            var ids=new PersistentIdAllocator();var restored=new MachineTaskQueue(ids);Assert.Throws<ArgumentException>(()=>restored.Restore(snapshot));
            Assert.That(restored.WaitingCount,Is.Zero);Assert.That(ids.NextId.Value,Is.EqualTo(1));
            restored.Submit("still usable",WorkPriority.Normal,out var task);Assert.That(task.Id.Value,Is.EqualTo(1));
        }
        [Test] public void DuplicateIdentityAndImpossiblePhase_AreRejected()
        {
            var source=new MachineTaskQueue(new PersistentIdAllocator());source.Submit("first",WorkPriority.Normal,out _);source.Submit("second",WorkPriority.Normal,out _);
            var duplicate=source.Capture();duplicate.Live[1].Id=duplicate.Live[0].Id;
            Assert.Throws<ArgumentException>(()=>new MachineTaskQueue(new PersistentIdAllocator()).Restore(duplicate));
            var impossible=source.Capture();impossible.Live[0].ChainClosed=true;
            Assert.Throws<ArgumentException>(()=>new MachineTaskQueue(new PersistentIdAllocator()).Restore(impossible));
        }
        [Test] public void CompletedHistory_IsDetachedAndNotRepublishedOnRestore()
        {
            var source=new MachineTaskQueue(new PersistentIdAllocator());source.Submit("done",WorkPriority.Normal,out var task);source.TryStart(task.Id);source.CloseChain(task.Id);
            var snapshot=source.Capture();snapshot.History[0].Name="Saved history";
            foreach(var original in source.History)Assert.That(original.Name,Is.EqualTo("done"));
            var restored=new MachineTaskQueue(new PersistentIdAllocator());int ended=0;restored.Ended+=value=>ended++;restored.Restore(snapshot);Assert.That(ended,Is.Zero);
            foreach(var value in restored.History) { Assert.That(value.Id,Is.EqualTo(task.Id));Assert.That(value.Name,Is.EqualTo("Saved history")); }
        }
    }
}
