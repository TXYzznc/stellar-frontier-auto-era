using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoEra.UI
{
    /// <summary>Forwards EventSystem cancel events into the semantic UI router without polling a physical device.</summary>
    public sealed class AutoEraUiCancelIntentProxy : MonoBehaviour, ICancelHandler
    {
        public void OnCancel(BaseEventData eventData)
        {
            if (AutoEraUiRuntime.DispatchIntent(AutoEraUiIntent.Cancel))
            {
                eventData.Use();
            }
        }
    }
}
