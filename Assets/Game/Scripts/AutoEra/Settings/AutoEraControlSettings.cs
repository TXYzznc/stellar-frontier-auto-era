using System;
using AutoEra.World.Region;

namespace AutoEra.Settings
{
    /// <summary>
    /// 本机操作设置：镜头平移／旋转／缩放速度与水平／垂直反转。
    ///
    /// 分工与前两页一致：持久化走注入的 <see cref="ISettingsStore"/>，
    /// 应用走 <see cref="IRegionCameraTarget"/>（现场输入模块把正在使用的镜头交出来）。
    ///
    /// **本页刻意不含改键**：设计原文是「第一版不开放改键，只读显示 InputModule 当前绑定」
    /// （<c>03-玩家体验/01-视角交互与信息呈现.md</c>）。所以「当前按键」那一栏是**只读**的，
    /// 不存在可持久化的绑定表——曾经把「缺少绑定表」写成这一页的缺口，那是把设计明确排除的东西
    /// 当成了待办。
    ///
    /// 区间与默认值取自 <see cref="RegionCameraParameters"/>（单一来源）：滑条区间、这里的夹取
    /// 与镜头自身的取值必须是同一套，否则会出现「滑条能拖到 40、镜头却在 15 处封顶」。
    /// </summary>
    public sealed class AutoEraControlSettings
    {
        public const string KeyPanSpeed = "AutoEra.Control.PanSpeed";
        public const string KeyRotationSpeed = "AutoEra.Control.RotationSpeed";
        public const string KeyZoomSpeed = "AutoEra.Control.ZoomSpeed";
        public const string KeyInvertHorizontal = "AutoEra.Control.InvertHorizontal";
        public const string KeyInvertVertical = "AutoEra.Control.InvertVertical";

        private readonly ISettingsStore _store;

        public AutoEraControlSettings(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>本机设置存储；为 null 表示设置服务不可用（界面据此说明原因）。</summary>
        public ISettingsStore Store => _store;

        // ------------------------------------------------------------ 读取

        public float PanSpeed =>
            RegionCameraParameters.ClampPan(_store.GetFloat(KeyPanSpeed, RegionCameraParameters.DefaultPanSpeed));

        public float RotationSpeed =>
            RegionCameraParameters.ClampRotation(_store.GetFloat(KeyRotationSpeed, RegionCameraParameters.DefaultRotationSpeed));

        public float ZoomSpeed =>
            RegionCameraParameters.ClampZoom(_store.GetFloat(KeyZoomSpeed, RegionCameraParameters.DefaultZoomSpeed));

        public bool InvertHorizontal => _store.GetBool(KeyInvertHorizontal, false);

        public bool InvertVertical => _store.GetBool(KeyInvertVertical, false);

        /// <summary>与默认值的差异条数，供「本机配置状态」栏展示。</summary>
        public int DifferenceFromDefaults()
        {
            int count = 0;
            if (!UnityEngine.Mathf.Approximately(PanSpeed, RegionCameraParameters.DefaultPanSpeed)) count++;
            if (!UnityEngine.Mathf.Approximately(RotationSpeed, RegionCameraParameters.DefaultRotationSpeed)) count++;
            if (!UnityEngine.Mathf.Approximately(ZoomSpeed, RegionCameraParameters.DefaultZoomSpeed)) count++;
            if (InvertHorizontal) count++;
            if (InvertVertical) count++;
            return count;
        }

        // ------------------------------------------------------------ 写入

        public bool SetPanSpeed(float value, out string reason)
        {
            reason = null;
            if (!Valid(RegionCameraParameters.MinPanSpeed, RegionCameraParameters.MaxPanSpeed, value))
            {
                reason = "平移速度超出可用区间。";
                return false;
            }

            _store.SetFloat(KeyPanSpeed, RegionCameraParameters.ClampPan(value));
            return Settle(out reason);
        }

        public bool SetRotationSpeed(float value, out string reason)
        {
            reason = null;
            if (!Valid(RegionCameraParameters.MinRotationSpeed, RegionCameraParameters.MaxRotationSpeed, value))
            {
                reason = "旋转速度超出可用区间。";
                return false;
            }

            _store.SetFloat(KeyRotationSpeed, RegionCameraParameters.ClampRotation(value));
            return Settle(out reason);
        }

        public bool SetZoomSpeed(float value, out string reason)
        {
            reason = null;
            if (!Valid(RegionCameraParameters.MinZoomSpeed, RegionCameraParameters.MaxZoomSpeed, value))
            {
                reason = "缩放速度超出可用区间。";
                return false;
            }

            _store.SetFloat(KeyZoomSpeed, RegionCameraParameters.ClampZoom(value));
            return Settle(out reason);
        }

        public bool SetInvertHorizontal(bool value, out string reason)
        {
            reason = null;
            _store.SetBool(KeyInvertHorizontal, value);
            return Settle(out reason);
        }

        public bool SetInvertVertical(bool value, out string reason)
        {
            reason = null;
            _store.SetBool(KeyInvertVertical, value);
            return Settle(out reason);
        }

        /// <summary>恢复本页默认：只动操作参数，不碰声音与显示设置。</summary>
        public bool ResetToDefaults(out string reason)
        {
            reason = null;
            _store.SetFloat(KeyPanSpeed, RegionCameraParameters.DefaultPanSpeed);
            _store.SetFloat(KeyRotationSpeed, RegionCameraParameters.DefaultRotationSpeed);
            _store.SetFloat(KeyZoomSpeed, RegionCameraParameters.DefaultZoomSpeed);
            _store.SetBool(KeyInvertHorizontal, false);
            _store.SetBool(KeyInvertVertical, false);
            return Settle(out reason);
        }

        /// <summary>只落盘，不改任何值。</summary>
        public bool Save(out string reason) => Settle(out reason);

        // ------------------------------------------------------------ 应用

        /// <summary>
        /// 把本机参数落到正在使用的镜头上。
        ///
        /// <paramref name="target"/> 为 null 是**正常情况**（世界外打开设置时没有镜头）：
        /// 参数已经保存在本机设置里，进入区域后由现场建立镜头时再取用，所以这里静默返回。
        /// </summary>
        public void ApplyTo(IRegionCameraTarget target)
        {
            if (target == null)
            {
                return;
            }

            target.PanSpeed = PanSpeed;
            target.RotationSpeed = RotationSpeed;
            target.ZoomSpeed = ZoomSpeed;
            target.InvertHorizontal = InvertHorizontal;
            target.InvertVertical = InvertVertical;
        }

        private static bool Valid(float min, float max, float value) =>
            !float.IsNaN(value) && value >= min && value <= max;

        private bool Settle(out string reason) => _store.Save(out reason);
    }
}
