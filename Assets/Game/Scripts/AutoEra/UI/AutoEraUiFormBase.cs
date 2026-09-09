using AutoEra.UI.Contracts;

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private GameObject _openTriggerFocus;

        /// <summary>Raised for a state-gated UI action; the authoritative operation owner decides the outcome.</summary>
        public event Action<AutoEraUiOperationActionRequest> OperationActionRequested;

        public long CurrentFormVersion => _requestVersionGate.FormVersion;

        protected override void OnOpen(object userData)
        {
            _openTriggerFocus = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            base.OnOpen(userData);
            _requestVersionGate.Open();
            AutoEraUiRuntime.RegisterForm(this);
            OnAutoEraOpen();
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
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
            AutoEraUiRuntime.UnregisterForm(this);
            OnAutoEraClose(isShutdown);
            base.OnClose(isShutdown, userData);
            RestoreOpenTriggerFocus();
        }

        protected override void OnRecycle()
        {
            _requestVersionGate.Close();
            AutoEraUiRuntime.UnregisterForm(this);
            OnAutoEraRecycle();
            base.OnRecycle();
            RestoreOpenTriggerFocus();
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
            if (OnBeforeFormIntent(intent))
            {
                return true;
            }

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
        protected virtual bool OnBeforeFormIntent(AutoEraUiIntent intent) { return false; }
        protected virtual bool OnAutoEraIntent(AutoEraUiIntent intent) { return false; }
        protected abstract void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation);

        protected void RaiseOperationActionRequest(AutoEraUiOperationActionRequest request)
        {
            OperationActionRequested?.Invoke(request);
        }

        private void RestoreOpenTriggerFocus()
        {
            if (_openTriggerFocus != null && _openTriggerFocus.activeInHierarchy && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_openTriggerFocus);
            }

            _openTriggerFocus = null;
        }
    }
}
