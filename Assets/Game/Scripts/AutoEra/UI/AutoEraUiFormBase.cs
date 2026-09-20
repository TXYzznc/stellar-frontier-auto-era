using AutoEra.UI.Contracts;

using System;
using UnityGameFramework.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

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
            // 参考分辨率与缩放策略由根 Canvas 统一提供：GFBuiltin.UpdateCanvasScaler() 用
            // AppSettings.DesignResolution（1920×1080）下发到场景根 Canvas。Form 预制体根节点
            // 刻意不带 Canvas（动态实例化到场景 Canvas 下），因此这里不再自行添加 CanvasScaler
            // —— 在自身 GameObject 上没有 Canvas 时它不生效，只是无法兑现的死配置。
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

        /// <summary>
        /// 读取打开时注入的服务会话句柄。
        ///
        /// 缺参数时返回 false —— 调用方应呈现「本页不可用」并说明原因，而不是抛异常：
        /// 规格要求不存在／无权限时禁用写操作并说明，而非中断。必须用 <c>TryGet</c>，
        /// 因为 <c>RefParams.Get(string)</c> 在缺 key 时会对 null 取 <c>.Value</c> 而抛
        /// <c>NullReferenceException</c>。
        /// </summary>
        protected bool TryGetSession(out AutoEraUiSession session)
        {
            session = null;
            return Params != null
                && Params.TryGet(AutoEraUiParamKeys.Session, out VarObject boxed)
                && (session = boxed.Value as AutoEraUiSession) != null;
        }

        /// <summary>读取打开请求（pageKey 与稳定身份）。缺参数时返回 false。</summary>
        protected bool TryGetRequest<TRequest>(out TRequest request) where TRequest : class
        {
            request = null;
            return Params != null
                && Params.TryGet(AutoEraUiParamKeys.Request, out VarObject boxed)
                && (request = boxed.Value as TRequest) != null;
        }

        /// <summary>
        /// 供导航服务透传：本页打开时拿到的会话（可能为空）。
        ///
        /// 让子界面的数据来源仍然只有一条——打开参数里的会话，而不是自己去某个全局对象里解析服务。
        /// </summary>
        internal AutoEraUiSession SessionOrNull =>
            TryGetSession(out AutoEraUiSession session) ? session : null;

        /// <summary>
        /// 关闭本界面（返回上一层）。用于「继续游戏」「返回」这类语义明确的按钮，
        /// 它们不该依赖 UITable 的 EscapeClose 登记值。
        /// </summary>
        protected void CloseSelf() => GF.UI.CloseUIForm(UIForm.SerialId);

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
