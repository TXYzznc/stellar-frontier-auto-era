using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法图画布内容区的布局占位（规格 13-算法工作台）。
    ///
    /// 规格把 <c>Content_AlgorithmGraph</c> 的组件声明为 `GraphLayoutGroup : LayoutGroup`
    /// 并注明「待实现……根据模型画布坐标布局，单一布局所有权，不与手写 Transform 争夺」。
    /// 所以这里提供一个**不做任何排列**的 LayoutGroup 原型：
    ///
    /// ① 满足「Content_ 必须挂 LayoutGroup」的结构契约；
    /// ② 把子节点的位置完全留给图快照与未来的完整布局算法——通用布局组会按自己的规则
    ///    覆写子节点坐标，而画布子节点（节点/连线）的位置必须由模型坐标决定。
    ///
    /// 完整实现（按模型坐标布局、单一所有权、连线几何）在算法工作台接入时替换本类方法体。
    /// </summary>
    public sealed class AutoEraGraphLayoutGroup : LayoutGroup
    {
        public override void CalculateLayoutInputHorizontal() { }

        public override void CalculateLayoutInputVertical() { }

        public override void SetLayoutHorizontal() { }

        public override void SetLayoutVertical() { }
    }
}
