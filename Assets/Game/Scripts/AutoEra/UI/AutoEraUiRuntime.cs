using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>Composition root for the UI intent router and replaceable physical-input adapters.</summary>
    public sealed class AutoEraUiRuntime : MonoBehaviour
    {
        private static AutoEraUiRuntime s_instance;

        private readonly AutoEraUiIntentRouter _router = new AutoEraUiIntentRouter();
        private AutoEraUiIntentInputAdapter _inputAdapter;

        public AutoEraUiIntentInputAdapter InputAdapter => _inputAdapter;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            _inputAdapter = new AutoEraUiIntentInputAdapter(intent => Dispatch(intent));
        }

        private void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        public bool Dispatch(AutoEraUiIntent intent)
        {
            return _router.Dispatch(intent);
        }

        /// <summary>Physical-input adapters call this semantic boundary after their own device normalization.</summary>
        public static bool DispatchIntent(AutoEraUiIntent intent)
        {
            return EnsureInstance().Dispatch(intent);
        }

        public static void RegisterForm(AutoEraUiFormBase form)
        {
            EnsureInstance()._router.Register(form);
        }

        public static void UnregisterForm(AutoEraUiFormBase form)
        {
            if (s_instance != null)
            {
                s_instance._router.Unregister(form);
            }
        }

        private static AutoEraUiRuntime EnsureInstance()
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            var root = new GameObject(nameof(AutoEraUiRuntime));
            DontDestroyOnLoad(root);
            return root.AddComponent<AutoEraUiRuntime>();
        }
    }
}
