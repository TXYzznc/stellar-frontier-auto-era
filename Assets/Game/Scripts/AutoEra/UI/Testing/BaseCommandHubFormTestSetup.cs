using System;

using AutoEra.Machines;
using AutoEra.World.Identity;

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AutoEra.UI.Testing
{
    /// <summary>
    /// 基地中枢测试登记：打开前注入一套纯内存的演示机器花名册。
    /// 正常流程中该数据由世界会话创建；测试场景下由本钩子伪造，随 Play 结束自然丢弃，不落盘、不进存档。
    /// </summary>
    [UiPanelTestSetup(UIViews.BaseCommandHubForm, "注入演示机器花名册（纯内存伪造：3 台机器，1 台已部署；随 Play 结束丢弃）")]
    public sealed class BaseCommandHubFormTestSetup : IUiPanelTestSetup
    {
        private static MachineRoster _currentRoster;

        public UniTask PrepareAsync(UiPanelTestSetupContext context)
        {
            // 重复准备时先丢弃上一套，避免残留事件订阅；Disposed 会通知仍开着的旧界面回退到未绑定文案。
            _currentRoster?.Dispose();
            _currentRoster = null;

            _currentRoster = CreateDemoRoster(out int created);
            AutoEraUiRuntime.BindMachineRoster(_currentRoster);
            if (created == 0)
            {
                Debug.LogWarning("[AutoEra][UiPanelTest] 机器数据表或本地化尚未就绪，基地中枢将以空花名册打开。");
            }

            return UniTask.CompletedTask;
        }

        private static MachineRoster CreateDemoRoster(out int createdCount)
        {
            createdCount = 0;
            var allocator = new PersistentIdAllocator();
            var roster = new MachineRoster(allocator, new PersistentObjectRegistry(allocator));
            if (!MachineCatalog.IsGameDataLoaded)
            {
                return roster;
            }

            var catalog = MachineCatalog.FromLoadedGameData();
            int[] demoRowIds = { 10011, 10021, 10022 };
            for (int i = 0; i < demoRowIds.Length; i++)
            {
                if (!catalog.TryGetMachine(demoRowIds[i], out var definition))
                {
                    continue;
                }

                MachineInstance machine = roster.Create(definition);
                createdCount++;
                if (createdCount == 1)
                {
                    roster.Deploy(machine.Id);
                }
            }

            return roster;
        }
    }
}
