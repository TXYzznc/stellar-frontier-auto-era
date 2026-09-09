using AutoEra.UI.Contracts;

namespace AutoEra.UI
{
    public enum AutoEraUiIntent
    {
        Cancel,
        Confirm,
        NavigatePrevious,
        NavigateNext
    }

    /// <summary>
    /// Thin UIForm bridge: lifecycle and stale-callback protection live here;
    /// domain state remains in the owner that creates operation snapshots.
    /// </summary>
    public abstract class AutoEraUiFormBase : UIFormBase
    {
        private readonly AutoEraUiRequestVersionGate _requestVersionGate = new AutoEraUiRequestVersionGate();

        protected long CurrentFormVersion => _requestVersionGate.FormVersion;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _requestVersionGate.Open();
            OnAutoEraOpen();
        }

        protected override void OnCover()
        {
            base.OnCover();
            OnAutoEraCover();
        }

        protected override void OnResume()
        {
            base.OnResume();
            OnAutoEraResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            OnAutoEraPause();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            _requestVersionGate.Close();
            OnAutoEraClose(isShutdown);
            base.OnClose(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            _requestVersionGate.Close();
            OnAutoEraRecycle();
            base.OnRecycle();
        }

        public long BeginOperationRequest()
        {
            return _requestVersionGate.BeginRequest();
        }

        public bool TryApplyOperationSnapshot(long formVersion, long requestVersion, AutoEraUiOperationSnapshot snapshot, bool longWaitHint)
        {
            if (!_requestVersionGate.Accepts(formVersion, requestVersion) || snapshot == null)
            {
                return false;
            }

            OnOperationPresentationChanged(snapshot, AutoEraUiOperationPresentation.Create(snapshot, longWaitHint));
            return true;
        }

        public bool TryHandleIntent(AutoEraUiIntent intent)
        {
            if (intent == AutoEraUiIntent.Cancel)
            {
                return TryCloseFromInputModule();
            }

            return OnAutoEraIntent(intent);
        }

        protected virtual void OnAutoEraOpen() { }
        protected virtual void OnAutoEraCover() { }
        protected virtual void OnAutoEraResume() { }
        protected virtual void OnAutoEraPause() { }
        protected virtual void OnAutoEraClose(bool isShutdown) { }
        protected virtual void OnAutoEraRecycle() { }
        protected virtual bool OnAutoEraIntent(AutoEraUiIntent intent) { return false; }
        protected abstract void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation);
    }
}
