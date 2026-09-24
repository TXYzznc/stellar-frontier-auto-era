using System;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    /// <summary>
    /// 已部署机器的实体逻辑。
    ///
    /// 存在的理由：**机器实体预制体不带 `RegionObjectView`**——那个组件是区域种子（`InitialRegion/*`）
    /// 的配置，用来把场景里摆好的对象**注册**成区域对象。已部署机器不是这种情况：
    /// 它的区域对象已经在 `DeployMachine` 里建好了，视图只能**指向**它。
    /// 所以本类在初始化时补上 `RegionObjectView`，让实体走 `BindDeployed` 而不是 `Initialize`。
    ///
    /// 为什么可以这样补：框架的 `Entity.ShowEntity` 在预制体没有实体逻辑时会 `AddComponent` 补上
    /// 逻辑组件，因此**不需要改动 B08 交付的正式实体预制体**——那批资产有明确的交付负责人，
    /// 为一条新链路去动它们会把所有权搅乱。
    ///
    /// 视图缺失不影响部署：区域对象与花名册状态是领域事实，视图只是呈现
    /// （见变更 design.md 的 D2／D5）。
    /// </summary>
    public sealed class InitialRegionMachineEntity : EntityBase
    {
        private RegionObjectView _view;

        /// <summary>本实体的区域视图。绑定前为 null。</summary>
        public RegionObjectView View => _view;

        /// <summary>是否已经完成视图绑定。</summary>
        public bool IsBound => _view != null && _view.Model != null;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _view = GetComponent<RegionObjectView>();
            if (_view == null)
            {
                _view = gameObject.AddComponent<RegionObjectView>();
            }
        }

        /// <summary>
        /// 绑定到**已经存在**的区域对象。取不到就抛——调用方必须先把机器部署成功，
        /// 这里不做「顺手注册一个」的兜底，那正是要避免的重复注册。
        /// </summary>
        public void Bind(InitialRegion region, PersistentId id)
        {
            if (_view == null) throw new InvalidOperationException("The machine view was not created; OnInit did not run.");
            _view.BindDeployed(region, id);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            // 视图释放**不**删除领域对象：机器是否仍在区域内由领域决定。
            if (_view != null) _view.Release();
            base.OnHide(isShutdown, userData);
        }
    }
}
