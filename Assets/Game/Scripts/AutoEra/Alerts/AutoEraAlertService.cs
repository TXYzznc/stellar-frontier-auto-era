using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Alerts
{
    /// <summary>
    /// 警报账本：谁在报、报过几次、什么时候恢复的、玩家读过没有。
    ///
    /// 它刻意只有**三个**会改变状态的动作，因为规格把这条边界划得很清楚：
    /// <list type="bullet">
    /// <item><see cref="Raise"/>：问题出现（或恢复之后再次出现）；</item>
    /// <item><see cref="Resolve"/>：**真实条件**已经不成立，自动转历史；</item>
    /// <item><see cref="MarkRead"/>：只改阅读状态，不解决问题。</item>
    /// </list>
    /// 界面上**没有**「清除故障」或「重置状态」这类按钮（规格原文）——警报是领域事实的投影，
    /// 不是待办清单：玩家要做的是把问题本身解决掉，警报会自己消失。
    ///
    /// **一行一问题**（规格：同类型、来源与目标合并）：账本按（种类，来源）保存条目，
    /// 条件持续存在期间不做任何写入——`次数` 是**发生过的段数**，不是轮询次数。
    /// 若把「每一帧都还在报」也算一次，一条整夜停机就能把次数刷到几万，历史也就没法读了。
    /// 恢复之后再发生＝同一行重新变为活跃、次数 +1、重新变成未读：
    /// 那一行始终回答「这个问题一共发生过几次、上次是什么时候」。
    /// </summary>
    public sealed class AutoEraAlertService
    {
        private readonly List<AlertEntry> _entries = new List<AlertEntry>(32);
        private readonly List<AlertEntry> _scratch = new List<AlertEntry>(32);
        private int _nextId = 1;

        /// <summary>账本内容发生变化。界面按区域刷新，不整页重刷。</summary>
        public event Action Changed;

        public int Count => _entries.Count;

        /// <summary>活跃警报条数（中枢与 HUD 摘要都用它）。</summary>
        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].IsActive) count++;
                }

                return count;
            }
        }

        /// <summary>未读条数（含已恢复的未读历史）。</summary>
        public int UnreadCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (!_entries[i].Read) count++;
                }

                return count;
            }
        }

        /// <summary>活跃警报里的最高等级；没有活跃警报时返回 null——界面据此说「当前没有需要处理的警报」。</summary>
        public AlertSeverity? HighestActiveSeverity
        {
            get
            {
                AlertSeverity? highest = null;
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (!_entries[i].IsActive) continue;
                    if (!highest.HasValue || _entries[i].Severity > highest.Value) highest = _entries[i].Severity;
                }

                return highest;
            }
        }

        /// <summary>
        /// 报告问题存在。返回 true 表示这是一次**真实的跨越**（新建一条，或已恢复的那条重新活跃），
        /// 调用方据此写事件日志；条件持续存在期间返回 false 且账本不变。
        /// </summary>
        public bool Raise(AlertKind kind, PersistentId source, long worldMilliseconds)
        {
            int index = IndexOf(kind, source);
            if (index < 0)
            {
                _entries.Add(new AlertEntry(_nextId++, kind, source, AlertCatalog.Severity(kind), AlertState.Active,
                    1, worldMilliseconds, worldMilliseconds, 0L, false));
                Changed?.Invoke();
                return true;
            }

            AlertEntry current = _entries[index];
            if (current.IsActive)
            {
                // 问题还在：不改次数、不改时间、不通知——它已经是「活跃」了。
                return false;
            }

            _entries[index] = new AlertEntry(current.Id, kind, source, AlertCatalog.Severity(kind), AlertState.Active,
                current.Count + 1, current.FirstMilliseconds, worldMilliseconds, 0L, false);
            Changed?.Invoke();
            return true;
        }

        /// <summary>真实条件已经不成立：活跃的那条转历史。没有活跃条目时什么都不做（返回 false）。</summary>
        public bool Resolve(AlertKind kind, PersistentId source, long worldMilliseconds)
        {
            int index = IndexOf(kind, source);
            if (index < 0 || !_entries[index].IsActive)
            {
                return false;
            }

            AlertEntry current = _entries[index];
            _entries[index] = new AlertEntry(current.Id, current.Kind, current.Source, current.Severity,
                AlertState.Recovered, current.Count, current.FirstMilliseconds, current.LastMilliseconds,
                worldMilliseconds, current.Read);
            Changed?.Invoke();
            return true;
        }

        /// <summary>标记已读：只改阅读状态，不改变警报本身（规格：标记已读不解决问题）。</summary>
        public bool MarkRead(int id)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id != id || _entries[i].Read) continue;

                AlertEntry current = _entries[i];
                _entries[i] = new AlertEntry(current.Id, current.Kind, current.Source, current.Severity,
                    current.State, current.Count, current.FirstMilliseconds, current.LastMilliseconds,
                    current.RecoveredMilliseconds, true);
                Changed?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 按界面需要的顺序拷出一份快照（**不清空目标**）：先活跃、再历史；
        /// 活跃内部按「等级从高到低、最近发生在前」，历史按恢复时间从近到远。
        /// 顺序由账本给，界面不自己排——否则中枢与 HUD 会排出两个不同的「最要紧的那条」。
        /// </summary>
        public void CopyInto(List<AlertEntry> destination)
        {
            if (destination == null) return;

            for (int pass = 0; pass < 2; pass++)
            {
                bool active = pass == 0;
                _scratch.Clear();
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].IsActive == active) _scratch.Add(_entries[i]);
                }

                _scratch.Sort(active ? (Comparison<AlertEntry>)CompareActive : CompareHistory);
                for (int i = 0; i < _scratch.Count; i++) destination.Add(_scratch[i]);
            }
        }

        /// <summary>清空账本；区域重建时调用。</summary>
        public void Clear()
        {
            if (_entries.Count == 0) return;
            _entries.Clear();
            Changed?.Invoke();
        }

        private static int CompareActive(AlertEntry left, AlertEntry right)
        {
            int bySeverity = right.Severity.CompareTo(left.Severity);
            if (bySeverity != 0) return bySeverity;
            return right.LastMilliseconds.CompareTo(left.LastMilliseconds);
        }

        private static int CompareHistory(AlertEntry left, AlertEntry right) =>
            right.RecoveredMilliseconds.CompareTo(left.RecoveredMilliseconds);

        /// <summary>合并键＝（种类，来源）。目标由种类决定，所以不单独参与比较。</summary>
        private int IndexOf(AlertKind kind, PersistentId source)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Kind == kind && _entries[i].Source == source) return i;
            }

            return -1;
        }
    }
}
