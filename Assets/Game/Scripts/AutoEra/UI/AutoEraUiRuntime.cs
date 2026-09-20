using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>
    /// 组合根：界面意图路由与可替换的物理输入适配器。
    ///
    /// 它只管「意图怎么走」这一件事，**不持有任何领域数据**：界面要的数据只有一条来源，
    /// 就是打开参数里的 <see cref="AutoEraUiSession"/>（见 <see cref="AutoEraUiParamKeys.Session"/>）。
    /// 历史：这里曾有一条 BindMachineRoster 直通花名册的旁路，只有测试后门在写它，
    /// 与读模型形成第二条数据路，已删除。
    /// </summary>
    public sealed class AutoEraUiRuntime : MonoBehaviour
    {
        private static AutoEraUiRuntime s_instance;

        private readonly AutoEraUiIntentRouter _router = new AutoEraUiIntentRouter();
        private AutoEraUiIntentInputAdapter _inputAdapter;

        public AutoEraUiIntentInputAdapter InputAdapter => _inputAdapter;

        public static bool BlocksWorldInput => s_instance != null && s_instance._router.BlocksWorldInput;

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
            s_instance?._router.Unregister(form);
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
