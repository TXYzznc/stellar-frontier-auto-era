using System;
using TMPro;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    public sealed class MainMenuForm : AutoEraUiFormBase
    {
        [SerializeField] private Button _enterButton;
        [SerializeField] private TMP_Text _status;
        public event Action EnterRequested;
        public string StatusText => _status != null ? _status.text : string.Empty;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _enterButton.onClick.AddListener(RequestEnter);
        }
        protected override void OnAutoEraOpen()
        {
            // The owning procedure enables this only after scene readiness; avoid a one-frame clickable race.
            SetStatus(true, "正在加载菜单");
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_enterButton.gameObject);
        }
        public void SetStatus(bool loading, string text)
        {
            _enterButton.interactable = !loading;
            _status.richText = false;
            _status.SetText(text ?? string.Empty);
        }
        private void RequestEnter() { if (_enterButton.interactable) EnterRequested?.Invoke(); }
        protected override void OnAutoEraClose(bool isShutdown) { EnterRequested = null; }
        protected override void OnAutoEraRecycle() { EnterRequested = null; }
        protected override void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
