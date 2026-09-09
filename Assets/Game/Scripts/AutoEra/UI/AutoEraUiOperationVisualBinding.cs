using AutoEra.UI.Contracts;
using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>Optional visual references for one operation source. Null references intentionally permit staged prefab wiring.</summary>
    [System.Serializable]
    public sealed class AutoEraUiOperationVisualBinding
    {
        [SerializeField] private string _sourceId = string.Empty;
        [SerializeField] private GameObject _spinner;
        [SerializeField] private GameObject _progress;
        [SerializeField] private GameObject _longWaitHint;
        [SerializeField] private GameObject _cancelAction;
        [SerializeField] private GameObject _retryAction;
        [SerializeField] private GameObject _detailsAction;

        public string SourceId => _sourceId;

        public void Apply(AutoEraUiOperationPresentation presentation)
        {
            SetActive(_spinner, presentation.ShowSpinner);
            SetActive(_progress, presentation.ShowProgress);
            SetActive(_longWaitHint, presentation.ShowLongWaitHint);
            SetActive(_cancelAction, presentation.ShowCancel);
            SetActive(_retryAction, presentation.ShowRetry);
            SetActive(_detailsAction, presentation.ShowDetails);
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
