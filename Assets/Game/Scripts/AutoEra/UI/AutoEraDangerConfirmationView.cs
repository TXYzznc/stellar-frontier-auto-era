using System;
using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Concrete fixed-layout confirmation layer for dangerous actions and invalid description bindings.</summary>
    public sealed class AutoEraDangerConfirmationView : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayRoot;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private GameObject _errorBox;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _dangerButton;

        private AutoEraDangerConfirmationPresentation _presentation;
        private bool _isVisible;
        private GameObject _triggerFocus;

        public event Action Confirmed;
        public bool IsVisible => _isVisible;

        private void Awake()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(HandleCancelClick);
            }

            if (_dangerButton != null)
            {
                _dangerButton.onClick.AddListener(HandleConfirmClick);
            }
        }

        private void OnDestroy()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(HandleCancelClick);
            }

            if (_dangerButton != null)
            {
                _dangerButton.onClick.RemoveListener(HandleConfirmClick);
            }
        }

        public void Show(AutoEraConfirmationDescriptionResolution description)
        {
            _triggerFocus = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            _presentation = new AutoEraDangerConfirmationPresentation(description);
            _isVisible = true;
            SetActive(_overlayRoot, true);

            bool valid = _presentation.Description.IsValid;
            SetActive(_errorBox, !valid);
            if (_bodyText != null)
            {
                _bodyText.gameObject.SetActive(valid);
                _bodyText.text = valid ? _presentation.Description.Body : string.Empty;
            }

            if (_errorText != null)
            {
                _errorText.text = valid ? string.Empty : _presentation.Description.ErrorDetail;
            }

            if (_dangerButton != null)
            {
                _dangerButton.interactable = _presentation.IsDangerActionEnabled;
            }

            if (!valid)
            {
                Debug.LogError(_presentation.Description.BuildConsoleDiagnostic());
            }

            SelectCancelByDefault();
        }

        public void Hide()
        {
            _isVisible = false;
            SetActive(_overlayRoot, false);

            if (_triggerFocus != null && _triggerFocus.activeInHierarchy && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_triggerFocus);
            }

            _triggerFocus = null;
        }

        public bool TryHandleIntent(AutoEraUiIntent intent)
        {
            if (!_isVisible)
            {
                return false;
            }

            if (intent == AutoEraUiIntent.Cancel)
            {
                Hide();
                return true;
            }

            if (intent == AutoEraUiIntent.Confirm && _presentation.IsDangerActionEnabled)
            {
                Confirmed?.Invoke();
                Hide();
                return true;
            }

            return false;
        }

        private void HandleCancelClick()
        {
            TryHandleIntent(AutoEraUiIntent.Cancel);
        }

        private void HandleConfirmClick()
        {
            TryHandleIntent(AutoEraUiIntent.Confirm);
        }

        private void SelectCancelByDefault()
        {
            if (_cancelButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_cancelButton.gameObject);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
