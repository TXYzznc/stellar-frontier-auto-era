using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>Small lifecycle-safe timer host for terminal UI feedback; never changes operation authority.</summary>
    internal sealed class AutoEraUiVisualTimer : MonoBehaviour
    {
        private static AutoEraUiVisualTimer s_instance;
        private readonly Dictionary<GameObject, int> _tokens = new Dictionary<GameObject, int>();

        public static void ShowFor(GameObject target, float seconds)
        {
            if (target == null)
            {
                return;
            }

            AutoEraUiVisualTimer host = EnsureInstance();
            int token = host._tokens.TryGetValue(target, out int previous) ? previous + 1 : 1;
            host._tokens[target] = token;
            target.SetActive(true);
            host.StartCoroutine(host.HideAfter(target, token, seconds));
        }

        private static AutoEraUiVisualTimer EnsureInstance()
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            var root = new GameObject(nameof(AutoEraUiVisualTimer));
            DontDestroyOnLoad(root);
            s_instance = root.AddComponent<AutoEraUiVisualTimer>();
            return s_instance;
        }

        private IEnumerator HideAfter(GameObject target, int token, float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (target != null && _tokens.TryGetValue(target, out int current) && current == token)
            {
                target.SetActive(false);
                _tokens.Remove(target);
            }
        }
    }
}
