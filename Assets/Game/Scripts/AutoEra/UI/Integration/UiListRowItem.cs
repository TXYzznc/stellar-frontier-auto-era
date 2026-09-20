using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 「字段名 + 值」型列表行，配合 GF 的 <c>SpawnItem&lt;T&gt;</c> 对象池使用。
    ///
    /// 契约里所有 <c>Item_*Template</c> 的行结构都是 `Btn_...Row`（Button + Image）下挂
    /// `Txt_...RowLabel` 与 `Txt_...RowValue`，所以这里按名字后缀定位，不依赖具体页面，
    /// 可以被中枢索引／详情、HUD 模块等复用。
    ///
    /// 池化语义：<see cref="UIItemObject.OnInit"/> 只在对象首次创建时调用一次，因此子节点
    /// 查找与按钮监听只做一次；每次复用只更新文本与回调。
    /// </summary>
    public sealed class UiListRowItem : UIItemObject
    {
        private const string LabelSuffix = "RowLabel";
        private const string ValueSuffix = "RowValue";

        private Action<int> _clicked;
        private int _index;

        public TMP_Text Label { get; private set; }
        public TMP_Text Value { get; private set; }
        public Button Button { get; private set; }

        protected override void OnInit()
        {
            foreach (TMP_Text text in gameObject.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name.EndsWith(LabelSuffix, StringComparison.Ordinal))
                {
                    Label = text;
                }
                else if (text.name.EndsWith(ValueSuffix, StringComparison.Ordinal))
                {
                    Value = text;
                }
            }

            Button = gameObject.GetComponentInChildren<Button>(true);
            if (Button != null)
            {
                Button.onClick.AddListener(RaiseClicked);
            }
        }

        /// <summary>绑定一行。传入 <paramref name="clicked"/> 为 null 表示纯展示行，按钮不可交互。</summary>
        public void Bind(int index, string label, string value, Action<int> clicked)
        {
            _index = index;
            _clicked = clicked;

            if (Label != null)
            {
                Label.richText = false;
                Label.SetText(label ?? string.Empty);
            }

            if (Value != null)
            {
                Value.richText = false;
                Value.SetText(value ?? string.Empty);
            }

            if (Button != null)
            {
                Button.interactable = clicked != null;
            }
        }

        private void RaiseClicked() => _clicked?.Invoke(_index);
    }
}
