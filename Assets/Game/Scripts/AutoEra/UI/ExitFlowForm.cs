using AutoEra.Save;
using AutoEra.UI.Contracts;

namespace AutoEra.UI
{
    public sealed partial class ExitFlowForm : AutoEraShellFormBase
    {
        private WorldSaveExitController _exit;
        private bool _forceConfirmation;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if(_backButton!=null)_backButton.onClick.AddListener(Resume);
            if(_closeButton!=null)_closeButton.onClick.AddListener(Resume);
            if(_resumeButton!=null)_resumeButton.onClick.AddListener(Resume);
            if(_retryButton!=null)_retryButton.onClick.AddListener(Retry);
            if(_forceButton!=null)_forceButton.onClick.AddListener(Force);
        }
        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots,0);_forceConfirmation=false;
            ApplyDefaultFocus(_resumeButton!=null ? _resumeButton.gameObject : null,null);
            _exit=TryGetSession(out AutoEraUiSession session) ? session.Application?.SaveExit : null;
            if(_exit!=null) { _exit.Changed+=Render;_exit.Begin(); }
            Render();
        }
        public bool ShowFormPage(int page)=>ShowPage(_pageRoots,page);
        public bool CanSaveAndExit=>_exit!=null;
        private void Resume() { _exit?.ReturnToGame();CloseSelf(); }
        private void Retry() { _forceConfirmation=false;_exit?.Retry();Render(); }
        private void Force()
        {
            if(_exit?.State!=WorldSaveExitState.Failed)return;
            if(!_forceConfirmation)
            {
                _forceConfirmation=true;
                _exitFlowFailureBody?.SetText("强制退出会丢失最近一次成功保存之后的进度。再次点击强制退出确认；也可以返回游戏。");return;
            }
            if(_exit.ConfirmForceExit())CloseSelf();
        }
        private void Render()
        {
            var state=_exit?.State ?? WorldSaveExitState.Idle;
            bool failed=state==WorldSaveExitState.Failed;
            if(_retryButton!=null)_retryButton.interactable=failed;
            if(_forceButton!=null)_forceButton.interactable=failed;
            SetState(_exitFlowLoadingState,state==WorldSaveExitState.WaitingBoundary || state==WorldSaveExitState.Writing);
            SetState(_exitFlowSuccessState,state==WorldSaveExitState.Completed);
            SetState(_exitFlowErrorState,failed);SetState(_exitFlowEmptyState,false);SetState(_exitFlowDisabledState,_exit==null);
            string stage=_exit==null ? "当前区域尚未接入完整世界存档，保存退出不可用。" :
                state==WorldSaveExitState.WaitingBoundary ? "正在完成当前操作，暂时不能进行新操作。" :
                state==WorldSaveExitState.Writing ? "正在保存，请稍候。" :
                state==WorldSaveExitState.Completed ? "保存完成，正在返回主菜单。" : failed ? "保存失败，可重试或返回游戏。" : "已返回游戏。";
            _exitFlowStageBody?.SetText(stage);
            WriteStateCard(_exitFlowLoadingState,stage);
            WriteStateCard(_exitFlowSuccessState,"保存完成");
            WriteStateCard(_exitFlowErrorState,"保存失败，请重试或返回游戏。");
            WriteStateCard(_exitFlowDisabledState,stage);
            if(!_forceConfirmation)_exitFlowFailureBody?.SetText(_exit?.Reason ?? "暂无保存失败记录。");
        }
        private void Release()
        {
            if(_exit!=null) { _exit.Changed-=Render;if(_exit.BlocksNewCommands)_exit.ReturnToGame(); }
            _exit=null;_forceConfirmation=false;
        }
        protected override void OnAutoEraClose(bool isShutdown)=>Release();
        protected override void OnAutoEraRecycle()=>Release();
        protected override bool OnBeforeFormIntent(AutoEraUiIntent intent)
        { if(intent!=AutoEraUiIntent.Cancel)return false;Resume();return true; }
        protected override void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot,AutoEraUiOperationPresentation presentation) { }
    }
}
