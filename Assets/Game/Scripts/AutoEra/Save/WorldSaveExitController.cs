using System;

namespace AutoEra.Save
{
    public enum WorldSaveExitState { Idle,WaitingBoundary,Writing,Failed,Completed,Forced }
    /// <summary>Blocks new player commands immediately; freezes simulation only after a complete snapshot. Returning to play keeps any atomic IO running.</summary>
    public sealed class WorldSaveExitController : IDisposable
    {
        private readonly WorldSaveCoordinator _saving;
        private bool _disposed,_captured;
        public WorldSaveExitController(WorldSaveCoordinator saving)
        { _saving=saving ?? throw new ArgumentNullException(nameof(saving));_saving.SnapshotCaptured+=OnCaptured; }
        public WorldSaveExitState State { get; private set; }
        public string Reason { get; private set; }
        public bool BlocksNewCommands=>State!=WorldSaveExitState.Idle;
        public bool FreezesSimulation=>_captured && BlocksNewCommands;
        public event Action Changed;
        public bool Begin()
        {
            if(_disposed || BlocksNewCommands || State==WorldSaveExitState.Completed || State==WorldSaveExitState.Forced)return false;
            _captured=false;SetState(WorldSaveExitState.WaitingBoundary,null);_saving.RequestSave(true);return true;
        }
        public bool Retry()
        {
            if(_disposed || State!=WorldSaveExitState.Failed)return false;
            // Keep a captured world frozen while recapturing/retrying IO.
            SetState(WorldSaveExitState.WaitingBoundary,null);_saving.RequestSave(true);return true;
        }
        public void ReturnToGame()
        { if(_disposed || State==WorldSaveExitState.Completed || State==WorldSaveExitState.Forced)return;_captured=false;SetState(WorldSaveExitState.Idle,null); }
        public bool ConfirmForceExit()
        {
            if(_disposed || State!=WorldSaveExitState.Failed || _saving.IsWriting)return false;
            SetState(WorldSaveExitState.Forced,"已确认丢弃尚未保存的进度");return true;
        }
        public void Pump(double elapsed)
        {
            if(_disposed)return;
            _saving.Pump(elapsed);
            if(!BlocksNewCommands || State==WorldSaveExitState.Completed || State==WorldSaveExitState.Forced)return;
            if(_saving.State==WorldSaveState.Failed) {SetState(WorldSaveExitState.Failed,_saving.Reason);return;}
            if(!_captured)return;
            if(_saving.IsWriting) {SetState(WorldSaveExitState.Writing,null);return;}
            if(_saving.HasUnsavedChanges || _saving.HasPendingRequest) { _saving.RequestSave(true);return; }
            if(_saving.State==WorldSaveState.Saved)SetState(WorldSaveExitState.Completed,null);
        }
        private void OnCaptured()
        { if(!BlocksNewCommands)return;_captured=true;SetState(WorldSaveExitState.Writing,null); }
        private void SetState(WorldSaveExitState state,string reason)
        { if(State==state && Reason==reason)return;State=state;Reason=reason;Changed?.Invoke(); }
        public void Dispose()
        { if(_disposed)return;_disposed=true;_saving.SnapshotCaptured-=OnCaptured;Changed=null; }
    }
}
