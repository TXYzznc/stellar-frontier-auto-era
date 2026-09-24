using System.Collections.Generic;

namespace AutoEra.UI
{
    /// <summary>Routes semantic UI intents to the topmost form that consumes them.</summary>
    public sealed class AutoEraUiIntentRouter
    {
        private readonly List<AutoEraUiFormBase> _forms = new List<AutoEraUiFormBase>();
        public bool BlocksWorldInput
        {
            get
            {
                // 「谁挡输入」由界面自己声明（见 AutoEraUiFormBase.BlocksWorldInput）：
                // 世界放置的机器部署页就是刻意不挡的那一类，它需要在世界里挪预览。
                foreach (AutoEraUiFormBase form in _forms)
                    if (form != null && form.gameObject.activeInHierarchy && form.BlocksWorldInput) return true;
                return false;
            }
        }

        public void Register(AutoEraUiFormBase form)
        {
            if (form == null)
            {
                return;
            }

            _forms.Remove(form);
            _forms.Add(form);
        }

        public void Unregister(AutoEraUiFormBase form)
        {
            if (form != null)
            {
                _forms.Remove(form);
            }
        }

        public bool Dispatch(AutoEraUiIntent intent)
        {
            for (int index = _forms.Count - 1; index >= 0; index--)
            {
                AutoEraUiFormBase form = _forms[index];
                if (form != null && form.TryHandleIntent(intent))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
