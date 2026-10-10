using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>端口视图仅负责行与尺寸；图编辑仍经读模型命令。</summary>
    public static class AlgorithmGraphPortView
    {
        public const float NodeWidth = 300f;
        public const float PortHeight = 26f;
        public const float PortSpacing = 2f;
        public const float HeaderAndPadding = 58f;
        public static float Height(int inputs, int outputs) => Mathf.Max(100f, HeaderAndPadding + Mathf.Max(inputs, outputs) * (PortHeight + PortSpacing));
        public static void Resize(GameObject instance, UiAlgorithmNodeRow node)
        { ((RectTransform)instance.transform).sizeDelta = new Vector2(NodeWidth, Height(node.InputPorts?.Count ?? 0, node.OutputPorts?.Count ?? 0)); }
        public static void Render(GameObject instance, string contentPath, string templateName, string buttonName, string textName,
            IReadOnlyList<UiAlgorithmPortRow> ports, ulong nodeId, bool input,
            Dictionary<(ulong, string, bool), RectTransform> buttons, ulong pendingNode, string pendingPort,
            Action<ulong, string, bool> clicked)
        {
            var content = instance.transform.Find(contentPath); var template = content?.Find(templateName);
            if (content == null || template == null) return;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i); if (child == template) continue;
                child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject);
            }
            if (ports == null) return;
            var item = instance.GetComponent<AlgorithmNodeItem>();
            foreach (var port in ports)
            {
                var row = UnityEngine.Object.Instantiate(template.gameObject, content);
                row.name = templateName + "(" + port.Key + ")"; row.SetActive(true);
                bool pending = !input && pendingNode == nodeId && pendingPort == port.Key;
                var button = row.transform.Find(buttonName)?.GetComponent<Button>();
                var label = row.transform.Find(buttonName + "/" + textName)?.GetComponent<TMP_Text>();
                if (label != null) label.SetText((pending ? "* " : "") + PortLabel(port.Key));
                item?.ApplyPortVisual(row.transform, port.ValueKind, input, port.Connected, pending);
                if (button == null) continue;
                buttons[(nodeId, port.Key, input)] = (RectTransform)button.transform;
                string key = port.Key; button.onClick.AddListener(() => clicked(nodeId, key, input));
            }
        }
        public static string PortLabel(string key)
        {
            switch (key)
            {
                case "event": return "触发"; case "value": return "数值"; case "valid": return "有效";
                case "sampled": return "采样"; case "condition": return "条件"; case "target": return "目标";
                case "count": return "数量"; case "item": return "物品"; case "accepted": return "已接收";
                case "started": return "开始"; case "completed": return "完成"; case "failed": return "失败";
                case "cancelled": return "取消"; case "rejected": return "拒绝"; case "partial": return "部分完成";
                case "preempted": return "被抢占"; case "targetInvalid": return "目标失效";
                case "seconds": return "秒数"; case "true": return "成立"; case "false": return "不成立";
                default: return key;
            }
        }
    }
}
