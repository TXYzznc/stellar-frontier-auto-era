using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>Serializable semantic text for the project accessibility adapter and focused-control narration.</summary>
    public sealed class AutoEraUiAccessibilityDescription : MonoBehaviour
    {
        [SerializeField] [TextArea] private string _description = string.Empty;

        public string Description => _description;
    }
}
