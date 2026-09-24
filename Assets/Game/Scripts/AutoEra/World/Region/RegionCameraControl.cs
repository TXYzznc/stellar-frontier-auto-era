using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>
    /// 镜头参数的可应用目标。
    ///
    /// 存在的理由是把「设置域」与「MonoBehaviour」解耦：设置页要把本机参数落到**正在使用的那台镜头**上，
    /// 但它既不该认识 `MonoBehaviour`，也不该自己去场景里搜组件（那会变成一条全局查找）。
    /// 会话里已经带着现场输入模块（<c>AutoEraUiSession.RegionInput</c>），由它交出这个目标即可；
    /// 测试也能用一个普通对象实现它，不必造场景。
    /// </summary>
    public interface IRegionCameraTarget
    {
        /// <summary>键盘平移速度（米／秒）。</summary>
        float PanSpeed { get; set; }

        /// <summary>轨道旋转速度／灵敏度（度／像素）。</summary>
        float RotationSpeed { get; set; }

        /// <summary>滚轮缩放速度（米／格）。</summary>
        float ZoomSpeed { get; set; }

        /// <summary>水平反转：轨道旋转的水平方向与平移的水平方向取反。</summary>
        bool InvertHorizontal { get; set; }

        /// <summary>垂直反转：轨道旋转的垂直方向与滚轮缩放方向取反。</summary>
        bool InvertVertical { get; set; }
    }

    /// <summary>
    /// 镜头参数的区间与默认值——**单一来源**。
    ///
    /// 设置页的滑条区间、本机设置的夹取、镜头自己的默认值都从这里取：三处各写一份迟早会走样，
    /// 而走样的表现是「滑条能拖到 100，镜头却在 40 处封顶」这种界面在撒谎的情况。
    ///
    /// 数值出处：设计把镜头速度的具体数值留给灰盒原型调优
    /// （<c>03-玩家体验/01-视角交互与信息呈现.md</c>：「镜头移动速度、旋转速度、缩放上下限……在灰盒原型中
    /// 以可读性和误触率为依据调优，不改变本文交互规则」），因此这里给出的是一个**可用的调优区间**，
    /// 默认值取镜头组件现有的序列化值（15／3／5），保证不改变现有手感。
    /// </summary>
    public static class RegionCameraParameters
    {
        public const float MinPanSpeed = 4f;
        public const float MaxPanSpeed = 40f;
        public const float DefaultPanSpeed = 15f;

        public const float MinRotationSpeed = 1f;
        public const float MaxRotationSpeed = 8f;
        public const float DefaultRotationSpeed = 3f;

        public const float MinZoomSpeed = 1f;
        public const float MaxZoomSpeed = 15f;
        public const float DefaultZoomSpeed = 5f;

        /// <summary>把任意输入夹到区间内；NaN 视为默认值。</summary>
        public static float ClampPan(float value) => Clamp(value, MinPanSpeed, MaxPanSpeed, DefaultPanSpeed);

        public static float ClampRotation(float value) => Clamp(value, MinRotationSpeed, MaxRotationSpeed, DefaultRotationSpeed);

        public static float ClampZoom(float value) => Clamp(value, MinZoomSpeed, MaxZoomSpeed, DefaultZoomSpeed);

        private static float Clamp(float value, float min, float max, float fallback) =>
            float.IsNaN(value) ? fallback : Mathf.Clamp(value, min, max);
    }
}
