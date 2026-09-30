using System;
using AutoEra.World.Identity;
using UnityEngine;

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

            MoveCollidersToSelectionLayer();
        }

        /// <summary>
        /// 把实体上的碰撞体切到现场点选层（RegionSelection，层 8）并**启用**。
        ///
        /// 机器实体预制体（B08 美术交付）的碰撞体在 Default 层（0）且**默认禁用**
        /// （<c>m_Enabled: 0</c>），而 <see cref="RegionInputModule"/> 的现场点选只对
        /// RegionSelection 层做射线检测（<c>_selectionLayers</c>）。只切层不启用，射线照样
        /// 穿透、机器点不中——这正是「部署成功却点不中、也没日志」的根因（层名存在时本方法静默）。
        /// </summary>
        private void MoveCollidersToSelectionLayer()
        {
            int selectionLayer = LayerMask.NameToLayer("RegionSelection");
            if (selectionLayer < 0)
            {
                // 项目里没有这个层名时静默跳过：至少不抛异常，让机器照常部署，只是不可点选。
                Debug.LogWarning("[AutoEra][Region] 未找到 RegionSelection 层，机器实体将不可点选。");
                return;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
                colliders[i].gameObject.layer = selectionLayer;
            }

            Debug.Log($"[AutoEra][Region] 已启用 {colliders.Length} 个碰撞体并切到 RegionSelection 层（{name}）。");
        }

        /// <summary>
        /// 绑定到**已经存在**的区域对象。取不到就抛——调用方必须先把机器部署成功，
        /// 这里不做「顺手注册一个」的兜底，那正是要避免的重复注册。
        /// </summary>
        public void Bind(InitialRegion region, PersistentId id)
        {
            if (_view == null) throw new InvalidOperationException("The machine view was not created; OnInit did not run.");
            _view.BindDeployed(region, id);
            SizeSelectionCollider(region, id);
        }

        /// <summary>
        /// 把选择碰撞体对齐到机器的真实占地：预制体里碰撞体是 1×1×1 的占位，而机器占地来自
        /// 区域对象（如轮式载体 1.8×2.4）。不对齐会让可点选区域只有中间一小块。
        /// </summary>
        private void SizeSelectionCollider(InitialRegion region, PersistentId id)
        {
            if (region == null || !region.TryGet(id, out RegionObject body)) return;
            int selectionLayer = LayerMask.NameToLayer("RegionSelection");
            if (selectionLayer < 0) return;

            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject.layer != selectionLayer) continue;
                if (colliders[i] is BoxCollider box)
                {
                    box.size = new Vector3(body.Size.x, 2f, body.Size.y);
                }
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            // 视图释放**不**删除领域对象：机器是否仍在区域内由领域决定。
            if (_view != null) _view.Release();
            base.OnHide(isShutdown, userData);
        }
    }
}
