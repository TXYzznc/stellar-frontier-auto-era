using System;
using AutoEra.Application;
using AutoEra.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace AutoEra.Procedures
{
    [Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName)]
    public sealed class AutoEraMainMenuProcedure : ProcedureBase
    {
        private AutoEraApplicationContext _context;
        private MainMenuForm _form;
        private int _uiId = -1;
        private bool _active, _enter;
        private int _version;
        private string _message;
        private bool _loading;
        protected override void OnEnter(IFsm<IProcedureManager> owner)
        {
            base.OnEnter(owner);
            if (!AutoEraProcedureContextSlot.TryGet(owner, out _context)) throw new InvalidOperationException("Application context missing.");
            _context.ReleaseActiveWorldSession();
            _active = true; _enter = false;
            int version = ++_version;
            AutoEraRuntimeSettings settings = AutoEraRuntimeSettings.Load(key => GF.Config.GetString(key));
            _message = _context.WorldEntryError ?? "进入初始区域";
            _loading = true;
            var parameters = UIParams.Create(false);
            parameters.OpenCallback = logic =>
            {
                if (!_active || version != _version) return;
                _form = (MainMenuForm)logic;
                _form.EnterRequested += RequestEnterWorld;
            };
            _uiId = GF.UI.OpenUIForm(UIViews.MainMenuForm, parameters);
            _context.SceneFlow.Load(settings.MainMenuScene, scene =>
            {
                if (_active && version == _version) _loading = false;
            }, error =>
            {
                if (!_active || version != _version) return;
                _loading = false;
                _message = "菜单场景加载失败：" + error + "；仍可重试进入区域";
            });
        }
        public void RequestEnterWorld() { if (_active && !_loading && !_enter) { _context.WorldEntryError = null; _enter = true; _form?.SetStatus(true, "正在进入区域"); } }
        protected override void OnUpdate(IFsm<IProcedureManager> owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            _form?.SetStatus(_loading, _loading ? "正在加载菜单" : _message);
            if (_enter) ChangeState<AutoEraWorldProcedure>(owner);
        }
        protected override void OnLeave(IFsm<IProcedureManager> owner, bool shutdown)
        {
            _active = false; _version++;
            _context?.SceneFlow.Cancel();
            if (_form != null) _form.EnterRequested -= RequestEnterWorld;
            _form = null;
            if (_uiId >= 0) GF.UI.CloseUIForm(_uiId);
            _uiId = -1;
            base.OnLeave(owner, shutdown);
        }
    }
}
