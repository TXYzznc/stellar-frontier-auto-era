using System;

namespace AutoEra.Algorithms
{
    /// <summary>
    /// 五套第一版系统模板的算法图定义与录入入口。模板以 template=true 编译入库：
    /// 绑定在实例化时由玩家补全，对象默认值在 <see cref="AlgorithmTemplateLibrary.Save"/> 时被零化。
    /// 效应器行为节点只声明动作与绑定键，目标从效应器绑定的 TargetId 推导。
    /// </summary>
    public static class InitialAlgorithmTemplates
    {
        /// <summary>世界创建时向模板库录入当前已可构造的系统模板。</summary>
        public static void Seed(AlgorithmTemplateLibrary library)
        {
            if (library == null) throw new ArgumentNullException(nameof(library));
            library.Save("基础灌溉", Irrigation(), system: true);
            library.Save("储量资源开采", Drilling(), system: true);
            library.Save("生长资源采集", Harvesting(), system: true);
            library.Save("农田基础作业", Farming(), system: true);
            library.Save("固定路线运输", Transport(), system: true);
        }

        /// <summary>
        /// 基础灌溉（DEC-107 / DEC-121，正式逻辑成本 9，持续算力 12）：
        /// 土壤传感器湿度 → 迟滞（开启 50 / 停止 65，锁存）→ 水枪锁存式喷射（开关=迟滞锁存状态，流量=标准流量）。
        /// </summary>
        public static AlgorithmDocument Irrigation()
        {
            var g = new AlgorithmDocument { DocumentId = 1, Revision = 1 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "soil", ValueType = AlgorithmType.Of(AlgorithmValueKind.Number) });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number), Default = AlgorithmValue.Numeric(50) });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number), Default = AlgorithmValue.Numeric(65) });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number), Default = AlgorithmValue.Numeric(1) });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Hysteresis, StateKey = "spray" });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Spray, BindingKey = "gun" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Output = "sampled", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 7, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 7, Input = "value" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 7, Input = "on" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 7, Input = "off" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 8, Output = "value", Input = "on" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 8, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 8, Input = "flow" });
            return g;
        }

        /// <summary>
        /// 储量资源开采（DEC-108 / DEC-122）：剩余储量与缓存空间均大于 0 时提交钻探，
        /// 计划批量取可采数量、缓存空间、默认上限 10 的最小值。
        /// </summary>
        public static AlgorithmDocument Drilling()
        {
            var g = new AlgorithmDocument { DocumentId = 1, Revision = 1 };
            var num = AlgorithmType.Of(AlgorithmValueKind.Number);
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "ore", Field = "resource", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Input, BindingKey = "ore_cap", Field = "capacity", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Input, BindingKey = "ore_cached", Field = "cached", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Subtract, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Minimum, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Minimum, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.Parameter, ValueType = num, Default = AlgorithmValue.Numeric(10) });
            g.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Constant, ValueType = num, Default = AlgorithmValue.Numeric(0) });
            g.Nodes.Add(new AlgorithmNode { Id = 11, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.Greater, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 12, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.Greater, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 13, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.And });
            g.Nodes.Add(new AlgorithmNode { Id = 14, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 15, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Drill, BindingKey = "drill" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 5, Output = "event", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 5, Output = "sampled", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 14, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 6, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 6, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 7, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 7, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 8, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 9, To = 8, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 11, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 10, To = 11, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 12, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 10, To = 12, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 13, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 12, To = 13, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 13, To = 14, Input = "condition" });
            g.Edges.Add(new AlgorithmEdge { From = 14, To = 15, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 15, Input = "count" });
            return g;
        }

        /// <summary>
        /// 生长资源采集（DEC-108 / DEC-122）：资源点处于可采集阶段、可采数量与缓存空间均大于 0 时提交切割，
        /// 计划批量取可采数量、缓存空间、默认上限 10 的最小值。
        /// </summary>
        public static AlgorithmDocument Harvesting()
        {
            var g = new AlgorithmDocument { DocumentId = 1, Revision = 1 };
            var num = AlgorithmType.Of(AlgorithmValueKind.Number);
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "tree", Field = "resource", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Input, BindingKey = "tree_cap", Field = "capacity", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Input, BindingKey = "tree_cached", Field = "cached", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Input, BindingKey = "tree_ready", Field = "harvestable", ValueType = AlgorithmType.Of(AlgorithmValueKind.Boolean) });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Subtract, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Minimum, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Minimum, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Parameter, ValueType = num, Default = AlgorithmValue.Numeric(10) });
            g.Nodes.Add(new AlgorithmNode { Id = 11, Kind = AlgorithmNodeKind.Constant, ValueType = num, Default = AlgorithmValue.Numeric(0) });
            g.Nodes.Add(new AlgorithmNode { Id = 12, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.Greater, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 13, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.Greater, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 14, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.And });
            g.Nodes.Add(new AlgorithmNode { Id = 15, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.And });
            g.Nodes.Add(new AlgorithmNode { Id = 16, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 17, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Cut, BindingKey = "cutter" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 6, Output = "event", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 6, Output = "sampled", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 16, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 7, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 7, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 8, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 8, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 9, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 10, To = 9, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 12, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 12, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 13, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 13, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 12, To = 14, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 13, To = 14, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 14, To = 15, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 15, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 15, To = 16, Input = "condition" });
            g.Edges.Add(new AlgorithmEdge { From = 16, To = 17, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 9, To = 17, Input = "count" });
            return g;
        }

        /// <summary>
        /// 农田基础作业（DEC-107 / DEC-121）：监听启动/恢复与阶段变化，按阶段分派——
        /// 空置→播种（作物银穗麦、数量默认 10）、成熟→收获剩余数量、待清理→整批次清理。
        /// </summary>
        public static AlgorithmDocument Farming()
        {
            var g = new AlgorithmDocument { DocumentId = 1, Revision = 1 };
            var num = AlgorithmType.Of(AlgorithmValueKind.Number);
            var bln = AlgorithmType.Of(AlgorithmValueKind.Boolean);
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "field_vacant", Field = "vacant", ValueType = bln });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Input, BindingKey = "field_mature", Field = "mature", ValueType = bln });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Input, BindingKey = "field_cleanup", Field = "cleanup", ValueType = bln });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Input, BindingKey = "field_crop", Field = "resource", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Enumeration), Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Enumeration), EnumValue = 0 } });
            g.Nodes.Add(new AlgorithmNode { Id = 11, Kind = AlgorithmNodeKind.Parameter, ValueType = num, Default = AlgorithmValue.Numeric(10) });
            g.Nodes.Add(new AlgorithmNode { Id = 12, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Sow, BindingKey = "arm" });
            g.Nodes.Add(new AlgorithmNode { Id = 13, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Harvest, BindingKey = "arm" });
            g.Nodes.Add(new AlgorithmNode { Id = 14, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Clean, BindingKey = "arm" });
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 6, Output = "event", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 6, Output = "sampled", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 7, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 8, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 9, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 7, Input = "condition" });
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 8, Input = "condition" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 9, Input = "condition" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 12, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 13, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 9, To = 14, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 12, Input = "count" });
            g.Edges.Add(new AlgorithmEdge { From = 10, To = 12, Input = "crop" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 13, Input = "count" });
            return g;
        }

        /// <summary>
        /// 固定路线运输（DEC-111 / DEC-123）：本地监测来源缓存，达量或货舱已有目标物品时优先交付，
        /// 否则前往来源装载、送往固定目的地卸载并返回；任务查询节点显式防重，容量不足经 30 秒延迟重试。
        /// 第一版简化：来源与目的地用位置参数（对象引用→停靠位置解析延后）；物品类型用枚举参数，
        /// 货舱查询的 Field 与其约定同源。正式逻辑成本 34（输入/参数 8 + 任务查询 4 + 出发判断 7 + 条件分支 1 +
        /// 任务提交 4 + 取货/交付分支 1 + 装载链 2 + 交付合流与移动 2 + 卸载合流与行为 2 + 容量重试 3 + 返回与完成 2）。
        /// </summary>
        public static AlgorithmDocument Transport()
        {
            var g = new AlgorithmDocument { DocumentId = 1, Revision = 1 };
            var num = AlgorithmType.Of(AlgorithmValueKind.Number);
            var bln = AlgorithmType.Of(AlgorithmValueKind.Boolean);
            var origin = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position) };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Input, BindingKey = "source_cached", Field = "cached", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Input, BindingKey = "source_amount", Field = "resource", ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Cargo, Field = "target_item" });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Position), Default = origin.Copy() });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Position), Default = origin.Copy() });
            g.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Parameter, ValueType = num, Default = AlgorithmValue.Numeric(10) });
            g.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Parameter, ValueType = AlgorithmType.Of(AlgorithmValueKind.Enumeration), Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Enumeration), EnumValue = 0 } });
            g.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.QueryTask, Field = "transport" });
            g.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 11, Kind = AlgorithmNodeKind.Arithmetic, Operator = AlgorithmOperator.Minimum, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 12, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.GreaterOrEqual, ValueType = num });
            g.Nodes.Add(new AlgorithmNode { Id = 13, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.Or });
            g.Nodes.Add(new AlgorithmNode { Id = 14, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.Not });
            g.Nodes.Add(new AlgorithmNode { Id = 15, Kind = AlgorithmNodeKind.Boolean, Operator = AlgorithmOperator.And });
            g.Nodes.Add(new AlgorithmNode { Id = 16, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 17, Kind = AlgorithmNodeKind.SubmitTask, Field = "transport" });
            g.Nodes.Add(new AlgorithmNode { Id = 18, Kind = AlgorithmNodeKind.Branch });
            g.Nodes.Add(new AlgorithmNode { Id = 19, Kind = AlgorithmNodeKind.Navigate });
            g.Nodes.Add(new AlgorithmNode { Id = 20, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Transfer, BindingKey = "arm" });
            g.Nodes.Add(new AlgorithmNode { Id = 21, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 22, Kind = AlgorithmNodeKind.Navigate });
            g.Nodes.Add(new AlgorithmNode { Id = 23, Kind = AlgorithmNodeKind.Merge });
            g.Nodes.Add(new AlgorithmNode { Id = 24, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Transfer, BindingKey = "arm" });
            g.Nodes.Add(new AlgorithmNode { Id = 25, Kind = AlgorithmNodeKind.Constant, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number, "s"), Default = AlgorithmValue.Numeric(30, "s") });
            g.Nodes.Add(new AlgorithmNode { Id = 26, Kind = AlgorithmNodeKind.Delay });
            g.Nodes.Add(new AlgorithmNode { Id = 27, Kind = AlgorithmNodeKind.Navigate });
            g.Nodes.Add(new AlgorithmNode { Id = 28, Kind = AlgorithmNodeKind.Log });
            // 根触发：启动/恢复 + 来源缓存采样。
            g.Edges.Add(new AlgorithmEdge { From = 1, To = 10, Output = "event", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 2, To = 10, Output = "sampled", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 10, To = 16, Output = "event", Input = "event" });
            // 出发判断：装载量 = min(来源可用, 货舱剩余)；出发 = 已有目标 或 达量；提交条件 = 防重 且 出发。
            g.Edges.Add(new AlgorithmEdge { From = 3, To = 11, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 11, Output = "remaining", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 12, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 7, To = 12, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 13, Output = "has_item", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 12, To = 13, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 9, To = 14, Output = "found", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 13, To = 15, Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 14, To = 15, Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 15, To = 16, Input = "condition" });
            // 提交运输任务，再按“已有目标”分流。
            g.Edges.Add(new AlgorithmEdge { From = 16, To = 17, Output = "true", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 17, To = 18, Output = "accepted", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 18, Output = "has_item", Input = "condition" });
            // 已有目标 → 直接交付；否则 → 前往来源装载。
            g.Edges.Add(new AlgorithmEdge { From = 18, To = 21, Output = "true", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 18, To = 19, Output = "false", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 19, Input = "target" });
            g.Edges.Add(new AlgorithmEdge { From = 19, To = 20, Output = "completed", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 11, To = 20, Input = "count" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 20, Input = "item" });
            g.Edges.Add(new AlgorithmEdge { From = 20, To = 21, Output = "completed", Input = "b" });
            // 交付合流 → 前往目的地 → 卸载合流（含重试）→ 卸载。
            g.Edges.Add(new AlgorithmEdge { From = 21, To = 22, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 6, To = 22, Input = "target" });
            g.Edges.Add(new AlgorithmEdge { From = 22, To = 23, Output = "completed", Input = "a" });
            g.Edges.Add(new AlgorithmEdge { From = 26, To = 23, Output = "event", Input = "b" });
            g.Edges.Add(new AlgorithmEdge { From = 23, To = 24, Output = "event", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 8, To = 24, Input = "item" });
            g.Edges.Add(new AlgorithmEdge { From = 4, To = 24, Output = "amount", Input = "count" });
            // 容量不足 → 30 秒延迟重试；完成后 → 返回来源并结束。
            g.Edges.Add(new AlgorithmEdge { From = 24, To = 26, Output = "partial", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 25, To = 26, Input = "seconds" });
            g.Edges.Add(new AlgorithmEdge { From = 24, To = 27, Output = "completed", Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 5, To = 27, Input = "target" });
            g.Edges.Add(new AlgorithmEdge { From = 27, To = 28, Output = "completed", Input = "event" });
            return g;
        }
    }
}
