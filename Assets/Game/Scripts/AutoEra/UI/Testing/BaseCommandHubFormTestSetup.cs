using System;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AutoEra.UI.Testing
{
    /// <summary>
    /// 基地中枢测试登记：把一套纯内存的世界会话写进打开参数。
    ///
    /// 关键在于**走的是生产同一条路**：界面数据来源在生产与测试中完全一致
    /// （UIParams 里的 <see cref="AutoEraUiSession"/> → 机器域读模型），这里伪造的只是
    /// 一个随 Play 结束丢弃的应用上下文与世界会话，不落盘、不进存档。
    /// 历史：这里曾用 AutoEraUiRuntime.BindMachineRoster 旁路直灌花名册，已随该旁路一并删除。
    /// </summary>
    [UiPanelTestSetup(UIViews.BaseCommandHubForm, "注入内存世界会话（3 台机器，1 台已部署；随 Play 结束丢弃）")]
    public sealed class BaseCommandHubFormTestSetup : IUiPanelTestSetup
    {
        private static AutoEraApplicationContext _currentContext;

        public UniTask PrepareAsync(UiPanelTestSetupContext context)
        {
            // 重复准备时先丢弃上一套，避免残留事件订阅；Dispose 会通知仍开着的旧界面回退到不可用态。
            _currentContext?.Dispose();
            _currentContext = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            if (!_currentContext.TryCreateWorldSession(0L, out AutoEraWorldSession world))
            {
                throw new InvalidOperationException("测试世界会话创建失败。");
            }

            int created = PopulateDemoMachines(world);
            AutoEraUiSession.ForWorld(_currentContext, world).WriteTo(context.Parameters);
            if (created == 0)
            {
                Debug.LogWarning("[AutoEra][UiPanelTest] 机器数据表或本地化尚未就绪，基地中枢将以空花名册打开。");
            }

            return UniTask.CompletedTask;
        }

        private static int PopulateDemoMachines(AutoEraWorldSession world)
        {
            int created = 0;
            if (!MachineCatalog.IsGameDataLoaded)
            {
                return created;
            }

            MachineRoster roster = world.Machines;
            var catalog = MachineCatalog.FromLoadedGameData();
            int[] demoRowIds = { 10011, 10021, 10022 };
            for (int i = 0; i < demoRowIds.Length; i++)
            {
                if (!catalog.TryGetMachine(demoRowIds[i], out var definition))
                {
                    continue;
                }

                MachineInstance machine = roster.Create(definition);
                created++;
                if (created == 1)
                {
                    roster.Deploy(machine.Id);
                }
            }

            return created;
        }
    }
}
