using System;
using TMPro;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 主菜单（规格 01-启动与存档/MainMenu）。
    ///
    /// 结构由 Docs/Development/UI-PrefabLayouts/MainMenuForm.contract.json 生成，
    /// 本脚本只声明契约里的绑定并实现意图，不自行搭建层级。
    /// 公共接口（EnterRequested / StatusText / SetStatus）由 AutoEraMainMenuProcedure 使用，
    /// 属于流程接入点，重建结构时必须保留。
    /// </summary>
    public sealed partial class MainMenuForm : AutoEraShellFormBase
    {
        // 绑定字段由 MainMenuForm.Fields.cs 依契约生成，与本文件同属一个 partial 类；
        // 新增节点引用请改契约后重新生成，不要在此手写字段。

        /// <summary>玩家请求进入初始区域；由流程订阅。</summary>
        public event Action EnterRequested;

        public string StatusText => _status != null ? _status.text : string.Empty;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 启动链入口（来源：00-页面关系与复用 的主要入口表）。「继续」走流程而不是界面，
            // 由 AutoEraMainMenuProcedure 订阅 EnterRequested；其余入口交给导航服务，
            // 它会把这页拿到的会话透传给目标界面。
            if (_enterButton != null) _enterButton.onClick.AddListener(RequestEnter);
            if (_newButton != null) _newButton.onClick.AddListener(OpenNewProgress);
            if (_slotsButton != null) _slotsButton.onClick.AddListener(OpenSaveSlots);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OpenSettings);
            if (_exitButton != null) _exitButton.onClick.AddListener(OpenExitFlow);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        private void OpenSaveSlots() => AutoEraUiNavigator.Open(this, UIViews.SaveSlotsForm);

        /// <summary>「新游戏」直达新建进度页；页索引由 SaveSlotsForm 自己公开，不在这里写死。</summary>
        private void OpenNewProgress() =>
            AutoEraUiNavigator.Open(this, UIViews.SaveSlotsForm, new AutoEraUiPageRequest(SaveSlotsForm.PageNewProgress));

        private void OpenSettings() => AutoEraUiNavigator.Open(this, UIViews.SettingsForm);

        private void OpenExitFlow() => AutoEraUiNavigator.Open(this, UIViews.ExitFlowForm);

        protected override void OnAutoEraOpen()
        {
            // 所属流程会在场景就绪后才允许点击；这里先进入加载态，避免一帧内的可点击竞态。
            ShowPage(_pageRoots, 0);
            SetStatus(true, "正在加载菜单");
            ApplyDefaultFocus(null, _enterButton != null ? _enterButton.gameObject : null);
        }

        public void SetStatus(bool loading, string text)
        {
            if (_enterButton != null) _enterButton.interactable = !loading;
            if (_status != null)
            {
                _status.richText = false;
                _status.SetText(text ?? string.Empty);
            }
        }

        private void RequestEnter()
        {
            if (_enterButton != null && !_enterButton.interactable)
            {
                return;
            }

            EnterRequested?.Invoke();
        }

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        protected override void OnAutoEraClose(bool isShutdown) => EnterRequested = null;

        protected override void OnAutoEraRecycle()
        {
            EnterRequested = null;
            base.OnAutoEraRecycle();
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
