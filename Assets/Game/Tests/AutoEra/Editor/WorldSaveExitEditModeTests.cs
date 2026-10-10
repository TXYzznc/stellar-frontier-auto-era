using System.Threading;
using System.Threading.Tasks;
using AutoEra.Save;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldSaveExitEditModeTests
    {
        private sealed class Source : IWorldSnapshotSource
        {
            internal bool Safe=true;
            public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
            { snapshot=Safe ? new WorldSnapshotDocument(10,1,revision,"Fixture",new[] {new WorldSnapshotSection("fixture",1,new {value=1})}) : null;reason=Safe ? null : "FixtureUnsafe";return Safe; }
        }
        private sealed class Writer : IWorldSnapshotWriter
        {
            internal TaskCompletionSource<WorldSaveWriteResult> Pending;
            internal int Count;
            public Task<WorldSaveWriteResult> WriteAsync(int slot,WorldSnapshotDocument data,CancellationToken token)
            { Count++;Pending=new TaskCompletionSource<WorldSaveWriteResult>();return Pending.Task; }
            internal void Finish(bool succeeded=true)=>Pending.SetResult(new WorldSaveWriteResult(succeeded,succeeded ? null : "FixtureIoFailure"));
        }
        [Test] public void ExitWaitsForBoundaryThenFreezesUntilWriteAcknowledged()
        {
            var source=new Source {Safe=false};var writer=new Writer();
            using(var saving=new WorldSaveCoordinator(0,source,writer))using(var exit=new WorldSaveExitController(saving))
            {
                Assert.That(exit.Begin(),Is.True);exit.Pump(0);
                Assert.That(exit.BlocksNewCommands,Is.True);Assert.That(exit.FreezesSimulation,Is.False);Assert.That(writer.Count,Is.Zero);
                source.Safe=true;exit.Pump(0);Assert.That(exit.FreezesSimulation,Is.True);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Writing));
                writer.Finish();exit.Pump(0);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Completed));
                exit.ReturnToGame();Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Completed));Assert.That(exit.FreezesSimulation,Is.True);
            }
        }
        [Test] public void FailedWriteKeepsWorldFrozen_RetryPreservesOriginalDirtyRevision()
        {
            var writer=new Writer();using(var saving=new WorldSaveCoordinator(0,new Source(),writer))using(var exit=new WorldSaveExitController(saving))
            {
                saving.MarkDirty();exit.Begin();exit.Pump(0);writer.Finish(false);exit.Pump(0);
                Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Failed));Assert.That(exit.FreezesSimulation,Is.True);Assert.That(saving.HasUnsavedChanges,Is.True);
                Assert.That(exit.Retry(),Is.True);exit.Pump(0);Assert.That(writer.Count,Is.EqualTo(2));Assert.That(exit.FreezesSimulation,Is.True);
                writer.Finish();exit.Pump(0);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Completed));Assert.That(saving.SavedRevision,Is.EqualTo(1));
            }
        }
        [Test] public void ReturnToPlayDuringWriteUnfreezesWithoutCancellingAtomicIo()
        {
            var writer=new Writer();using(var saving=new WorldSaveCoordinator(0,new Source(),writer))using(var exit=new WorldSaveExitController(saving))
            {
                exit.Begin();exit.Pump(0);exit.ReturnToGame();saving.MarkDirty();
                Assert.That(exit.BlocksNewCommands,Is.False);Assert.That(exit.FreezesSimulation,Is.False);Assert.That(saving.IsWriting,Is.True);
                writer.Finish();exit.Pump(0);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Idle));Assert.That(saving.HasUnsavedChanges,Is.True);
            }
        }
        [Test] public void ForceExitRequiresFailureAndNoWriteInFlight()
        {
            var writer=new Writer();using(var saving=new WorldSaveCoordinator(0,new Source(),writer))using(var exit=new WorldSaveExitController(saving))
            {
                Assert.That(exit.ConfirmForceExit(),Is.False);exit.Begin();exit.Pump(0);Assert.That(exit.ConfirmForceExit(),Is.False);
                writer.Finish(false);exit.Pump(0);Assert.That(exit.ConfirmForceExit(),Is.True);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Forced));
                exit.ReturnToGame();Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Forced));Assert.That(exit.FreezesSimulation,Is.True);
            }
        }
        [Test] public void ExitDuringOlderAutosaveWaitsForItsOwnFreshSnapshot()
        {
            var writer=new Writer();using(var saving=new WorldSaveCoordinator(0,new Source(),writer))using(var exit=new WorldSaveExitController(saving))
            {
                saving.MarkDirty(true);saving.Pump(0);exit.Begin();exit.Pump(0);
                Assert.That(exit.FreezesSimulation,Is.False);writer.Finish();exit.Pump(0);
                Assert.That(writer.Count,Is.EqualTo(2));Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Writing));Assert.That(exit.FreezesSimulation,Is.True);
                writer.Finish();exit.Pump(0);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Completed));
            }
        }
        [Test] public void MutationAfterCaptureCannotCompleteExitBeforeNewestRevisionSaved()
        {
            var writer=new Writer();using(var saving=new WorldSaveCoordinator(0,new Source(),writer))using(var exit=new WorldSaveExitController(saving))
            {
                exit.Begin();exit.Pump(0);saving.MarkDirty();writer.Finish();exit.Pump(0);
                Assert.That(exit.State,Is.Not.EqualTo(WorldSaveExitState.Completed));exit.Pump(0);Assert.That(writer.Count,Is.EqualTo(2));
                writer.Finish();exit.Pump(0);Assert.That(exit.State,Is.EqualTo(WorldSaveExitState.Completed));Assert.That(saving.SavedRevision,Is.EqualTo(1));
            }
        }
    }
}
