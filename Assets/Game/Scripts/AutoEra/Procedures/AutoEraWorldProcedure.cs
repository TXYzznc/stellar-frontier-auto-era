using System;
using AutoEra.Application;
using AutoEra.UI;
using AutoEra.World.Region;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace AutoEra.Procedures
{
    [Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName)]
    public sealed class AutoEraWorldProcedure : ProcedureBase
    {
        private AutoEraApplicationContext _context;
        private InitialRegionScene _entry;
        private int _uiId = -1, _version;
        private bool _active, _return;
        protected override void OnEnter(IFsm<IProcedureManager> owner)
        {
            base.OnEnter(owner);
            if (!AutoEraProcedureContextSlot.TryGet(owner, out _context)) throw new InvalidOperationException("Application context missing.");
            AutoEraRuntimeSettings settings = AutoEraRuntimeSettings.Load(key => GF.Config.GetString(key));
            if (!_context.TryCreateWorldSession(settings.InitialMilliseconds, out var session)) throw new InvalidOperationException("World session already exists.");
            _active = true; _return = false;
            int version = ++_version;
            _context.SceneFlow.Load(settings.WorldScene, scene =>
            {
                if (!_active || version != _version) return;
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var candidate = root.GetComponent<InitialRegionScene>();
                        if (candidate == null) continue;
                        if (_entry != null) throw new InvalidOperationException("Ambiguous InitialRegion scene entry.");
                        _entry = candidate;
                    }
                    if (_entry == null) throw new InvalidOperationException("InitialRegion entry missing.");
                    _entry.InitializeRuntime(session, () =>
                    {
                    if (!_active || version != _version) return;
                    // 会话随参数进入界面：页面只经 UIParams 拿数据来源，不持有全局单例。
                    // 区域也随会话一起给出去——现场界面（放置、世界对象选择、资源页）都要用它，
                    // 而区域是场景级对象，不属于世界会话。
                    // 不传 allowEscape：由 UITable 的 EscapeClose 兜底。
                    var parameters = AutoEraUiSession.ForWorld(_context, session, _entry.Region).WriteTo(UIParams.Create());
                    parameters.OpenCallback = logic =>
                    {
                        if (_active && version == _version && _entry != null) _entry.BindHud((FieldHudForm)logic);
                    };
                    _uiId = GF.UI.OpenUIForm(UIViews.FieldHudForm, parameters);
                    }, Fail);
                }
                catch (Exception exception) { Fail(exception.Message); }
            }, Fail);
        }
        private void Fail(string error)
        {
            _context.WorldEntryError = "进入区域失败：" + error;
            Log.Warning("[AutoEra][World] Enter failed; returning to menu: {0}", error);
            _return = true;
        }
        public void RequestReturnToMenu() => _return = true;
        protected override void OnUpdate(IFsm<IProcedureManager> owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            if (_return || (_context != null && _context.ConsumeReturnToMenuRequest())) { ChangeState<AutoEraMainMenuProcedure>(owner); return; }
            _entry?.Advance(realElapsed);
        }
        protected override void OnLeave(IFsm<IProcedureManager> owner, bool shutdown)
        {
            _active = false; _version++;
            _context?.SceneFlow.Cancel();
            // 同 MainMenuProcedure：UI 组件可能已先关闭，直接 CloseUIForm 会抛异常。
            if (_uiId >= 0 && GF.UI != null && GF.UI.HasUIForm(_uiId)) GF.UI.CloseUIForm(_uiId);
            _uiId = -1;
            _entry?.Release(); _entry = null;
            _context?.ReleaseActiveWorldSession();
            base.OnLeave(owner, shutdown);
        }
    }
}
