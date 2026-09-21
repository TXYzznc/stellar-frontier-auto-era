using System.Collections.Generic;
using AutoEra.Save;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 损坏存档恢复（规格 01-启动与存档/Recovery）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    /// 恢复失败不静默降级，也不自动改用另一个槽位——候选由调用方经请求对象指定。
    ///
    /// 这一页回答「这个槽位坏了没有、备份能不能救它」：
    /// 诊断栏陈述真实文件状态与备份可用性；候选栏列出可读出来的那份内容；
    /// 「恢复」只在**确有备份**时可点，否则禁用——按规格，不可行的写操作要禁用并说明。
    ///
    /// 注意区分两件事：<see cref="SaveSlotService.Read"/> 会在主文件损坏时回退读备份，
    /// 所以「能读出来」不代表存档已经修好；只有 <see cref="SaveSlotService.RestoreFromBackup"/>
    /// 把备份提升为主文件才算真恢复。
    /// </summary>
    public sealed partial class SaveRecoveryForm : AutoEraShellFormBase
    {
        private const string NoTargetSlot = "没有指定要恢复的槽位：请从存档列表里选中损坏的存档再进入本页。";

        private int _slotIndex = -1;
        private SaveSlotService _service;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_returnButton != null) _returnButton.onClick.AddListener(RequestCancel);
            if (_restoreButton != null) _restoreButton.onClick.AddListener(RestoreBackup);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            // 首焦点优先落在安全返回，而不是可能造成数据覆盖的「恢复」。
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _restoreButton != null ? _restoreButton.gameObject : null);

            _slotIndex = TryGetRequest(out AutoEraUiSaveSlotRequest request) ? request.SlotIndex : -1;
            _service = TryGetSession(out AutoEraUiSession session) ? session.SaveSlots : null;
            Render();
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            _service = null;
            _slotIndex = -1;
        }

        /// <summary>本页当前处理的槽位索引；-1 表示没有指定。</summary>
        public int TargetSlotIndex => _slotIndex;

        /// <summary>「恢复」当前是否可点：只有确实存在**可用**备份时才可点。</summary>
        public bool CanRestore => _service != null
            && SaveSlotService.IsValidSlotIndex(_slotIndex)
            && _service.HasUsableBackup(_slotIndex);

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void RestoreBackup()
        {
            if (!CanRestore)
            {
                return;
            }

            bool restored = _service.RestoreFromBackup(_slotIndex);
            SetState(_recoveryLoadingState, false);
            SetState(_recoverySuccessState, restored);
            SetState(_recoveryErrorState, !restored);
            SetState(_recoveryEmptyState, false);
            SetState(_recoveryDisabledState, false);

            if (_recoveryCandidateBody != null)
            {
                _recoveryCandidateBody.SetText(restored
                    ? "已从备份恢复：该槽位现在可以正常读取。"
                    : "恢复失败：备份不可读，主文件保持原样。");
            }

            RenderDiagnosis();
            if (_restoreButton != null)
            {
                _restoreButton.interactable = CanRestore;
            }
        }

        private void Render()
        {
            if (_service == null || !SaveSlotService.IsValidSlotIndex(_slotIndex))
            {
                ShowUnavailable(NoTargetSlot);
                return;
            }

            SaveSlotReadResult result = _service.Read(_slotIndex);
            bool hasBackup = _service.HasUsableBackup(_slotIndex);

            SetState(_recoveryLoadingState, false);
            SetState(_recoveryErrorState, result.Status == SaveSlotReadStatus.Corrupt && !hasBackup);
            SetState(_recoverySuccessState, result.IsSuccess || hasBackup);
            SetState(_recoveryEmptyState, result.Status == SaveSlotReadStatus.Empty && !hasBackup);
            SetState(_recoveryDisabledState, false);

            RenderDiagnosis();
            RenderCandidate(result, hasBackup);

            if (_restoreButton != null)
            {
                _restoreButton.interactable = hasBackup;
            }
        }

        private void RenderDiagnosis()
        {
            if (_recoveryDiagnosisTemplate == null || _recoveryDiagnosisContent == null)
            {
                return;
            }

            SaveSlotReadResult result = _service.Read(_slotIndex);
            var fields = new List<UiDetailField>
            {
                new UiDetailField("槽位", "槽位 " + (_slotIndex + 1)),
                new UiDetailField("文件状态", SaveSlotNarrative.DescribeStatus(result.Status)),
                new UiDetailField("本次读取来源", SaveSlotNarrative.DescribeOrigin(result)),
            };

            // 三份备份逐份列出：规格要求恢复界面说明「有效备份时间」，
            // 只写一句「备份可用」会让玩家不知道会退回到哪个时间点。
            IReadOnlyList<SaveSlotBackupInfo> backups = _service.ListBackups(_slotIndex);
            for (int i = 0; i < backups.Count; i++)
            {
                fields.Add(new UiDetailField("备份 " + backups[i].Index, SaveSlotNarrative.DescribeBackup(backups[i])));
            }

            RenderDetailRows(_recoveryDiagnosisTemplate, _recoveryDiagnosisContent, fields);
            if (_recoveryDiagnosisBody != null)
            {
                _recoveryDiagnosisBody.SetText(string.Empty);
            }
        }

        private void RenderCandidate(SaveSlotReadResult result, bool hasBackup)
        {
            if (_recoveryCandidateTemplate != null && _recoveryCandidateContent != null && result.IsSuccess)
            {
                var fields = new List<UiDetailField>
                {
                    new UiDetailField("摘要", result.Record.Summary),
                    new UiDetailField("世界时间", AutoEraUiFormat.WorldTime(result.Record.WorldTimeMilliseconds)),
                    new UiDetailField("保存时间", SaveSlotNarrative.DescribeBackupTime(result.Record)),
                    new UiDetailField("来源", SaveSlotNarrative.DescribeOrigin(result)),
                };

                RenderDetailRows(_recoveryCandidateTemplate, _recoveryCandidateContent, fields);
            }

            if (_recoveryCandidateBody != null)
            {
                _recoveryCandidateBody.SetText(SaveSlotNarrative.DescribeRecovery(
                    result, _service.ListBackups(_slotIndex)));
            }
        }

        private void ShowUnavailable(string reason)
        {
            SetState(_recoveryLoadingState, false);
            SetState(_recoverySuccessState, false);
            SetState(_recoveryErrorState, false);
            SetState(_recoveryEmptyState, true);
            SetState(_recoveryDisabledState, true);

            if (_recoveryDiagnosisBody != null)
            {
                _recoveryDiagnosisBody.SetText(reason);
            }

            if (_recoveryCandidateBody != null)
            {
                _recoveryCandidateBody.SetText(string.Empty);
            }

            if (_restoreButton != null)
            {
                _restoreButton.interactable = false;
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
