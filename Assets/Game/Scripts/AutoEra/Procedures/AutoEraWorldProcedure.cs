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
        private bool _worldReady;
        private AutoEra.Save.WorldSlotEntryRequest _slotRequest;
        protected override void OnEnter(IFsm<IProcedureManager> owner)
        {
            base.OnEnter(owner);
            if (!AutoEraProcedureContextSlot.TryGet(owner, out _context)) throw new InvalidOperationException("Application context missing.");
            AutoEraRuntimeSettings settings = AutoEraRuntimeSettings.Load(key => GF.Config.GetString(key));
            _active = true; _return = false;_worldReady=false;
            _context.Slots.TryConsume(out _slotRequest);
            AutoEra.World.AutoEraWorldSession session;
            if(_slotRequest?.Candidate!=null)session=_slotRequest.Candidate.World;
            else
            {
                if (!_context.TryCreateWorldSession(settings.InitialMilliseconds, out session)) throw new InvalidOperationException("World session already exists.");
                if(_slotRequest!=null && !_slotRequest.Setup.TryInitializeNew(session,out var reason)) { Fail(reason);return; }
            }
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
                    var regionInput = _entry.GetComponent<AutoEra.Input.RegionInputModule>();
                    Action ready = () =>
                    {
                    if (!_active || version != _version) return;
                    if(_slotRequest!=null)
                    {
                        _entry.BindPersistence(_slotRequest.Persistence);
                        if(_slotRequest.IsNew && !_slotRequest.Persistence.TryBindScene(_entry,null,out var domainReason)) {Fail(domainReason);return;}
                        bool attached=_slotRequest.Candidate!=null ? _context.TryCommitRestoredWorld(_slotRequest.Candidate,_slotRequest.SlotIndex,_entry) :
                            _context.TryAttachWorldSaving(_slotRequest.SlotIndex,_entry);
                        if(!attached) { Fail("完整世界保存会话接入失败");return; }
                        _entry.BindSaveRequests(_context.SaveCoordinator);
                        if(_slotRequest.IsNew)_context.SaveCoordinator.MarkDirty(true);
                    }
                    _worldReady=true;
                    // 本机保存的镜头参数要在**镜头刚建好**时推上去：玩家可能在主菜单里就调过，
                    // 那时现场还没有镜头。漏掉这一步的表现是「设置保存了、进区域却还是默认手感」，
                    // 而且只有打开一次设置页才会生效——那等于让界面替保存撒谎。
                    SettingsReadModels.CreateControlSettings()?.ApplyTo(regionInput?.CameraTarget);
                    // 会话随参数进入界面：页面只经 UIParams 拿数据来源，不持有全局单例。
                    // 区域也随会话一起给出去——现场界面（放置、世界对象选择、资源页）都要用它，
                    // 而区域是场景级对象，不属于世界会话。
                    // 世界输入模块同样随会话传递：放置预览的指针换算与提交都在它手里，
                    // 界面自己再实现一份就会造出第二个互相竞争的预览。
                    // 不传 allowEscape：由 UITable 的 EscapeClose 兜底。
                    var parameters = AutoEraUiSession.ForWorld(
                        _context, session, _entry.Region, regionInput,
                        _entry.MachineRuntimes, _entry.Energy, _entry.Alerts)
                        .WriteTo(UIParams.Create());
                    parameters.OpenCallback = logic =>
                    {
                        if (_active && version == _version && _entry != null) _entry.BindHud((FieldHudForm)logic);
                    };
                    _uiId = GF.UI.OpenUIForm(UIViews.FieldHudForm, parameters);
                    };
                    if(_slotRequest?.Candidate!=null)
                        _entry.InitializePersistent(_slotRequest.Candidate,_slotRequest.Persistence,()=>
                        {
                            if(!_active || version!=_version)return;
                            if(_slotRequest.RequiresOffline)
                                _slotRequest.Setup.BeginOfflineContinuation(_slotRequest.Candidate,_entry,_slotRequest,ready,Fail);
                            else ready();
                        },Fail);
                    else _entry.InitializeRuntime(session,ready,Fail);
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
            if(_entry!=null)_entry.BlocksNewPlayerCommands=_context?.BlocksNewWorldCommands==true;
            if(_worldReady && _context?.FreezesWorldSimulation!=true)
            {
                _entry?.Advance(realElapsed);
                if(_entry!=null && realElapsed>0)_context?.SaveCoordinator?.MarkDirty();
            }
            _context?.SaveExit?.Pump(realElapsed);
            if(_context?.SaveExit?.State==AutoEra.Save.WorldSaveExitState.Completed || _context?.SaveExit?.State==AutoEra.Save.WorldSaveExitState.Forced)
                _return=true;
        }
        protected override void OnLeave(IFsm<IProcedureManager> owner, bool shutdown)
        {
            _active = false; _version++;
            _worldReady=false;
            _context?.SceneFlow.Cancel();
            var leavingWorld=_context?.ActiveWorldSession ?? _slotRequest?.Candidate?.World;
            if(leavingWorld?.IsActive==true)leavingWorld.Events.SuspendPublisher();
            if(leavingWorld!=null && GF.UI!=null)
                foreach(var form in GF.UI.GetAllLoadedUIForms())
                    if(form.Logic is AutoEraUiFormBase productForm && ReferenceEquals(productForm.SessionOrNull?.World,leavingWorld))
                        GF.UI.CloseUIForm(form.SerialId);
            // 同 MainMenuProcedure：UI 组件可能已先关闭，直接 CloseUIForm 会抛异常。
            if (_uiId >= 0 && GF.UI != null && GF.UI.HasUIForm(_uiId)) GF.UI.CloseUIForm(_uiId);
            _uiId = -1;
            _entry?.Release(); _entry = null;
            _slotRequest?.Dispose();_slotRequest=null;
            _context?.ReleaseActiveWorldSession();
            base.OnLeave(owner, shutdown);
        }
    }
}
