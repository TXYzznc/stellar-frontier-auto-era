---
name: grill-with-docs
description: 对照设计文档、术语和实现核对方案，定位真实冲突，并在获授权后增量归档已确认决定。
disable-model-invocation: true
---

# 文档驱动的决策检查

调用策略：保留 Claude 的 disable-model-invocation；Codex 使用 agents/openai.yaml 的
allow_implicit_invocation=false 表达同一显式调用策略。通用校验器可能不识别前者。

从项目入口定位权威文档、决策记录，再核对实现。优先引用通用规则，不按对象复制规则，
不把设计要求误称为代码已有能力。

给出冲突来源、实际差异、影响与建议，只询问确需用户决定的差异。
已确认的目标、边界、验收和约束直接复用，不设最低轮数或重复确认。

使用当前宿主实际提供且模式允许的提问接口；无适用接口时用简短自然语言。
不强制 AskUserQuestion，不虚构 preview 或 multiSelect 参数。
非阻塞问题提出后继续安全工作；仅暂停缺少关键决策的受影响部分。

## 归档

本次任务允许修改文档时才更新既有权威文档；纯分析只报告。
不因缺少 CONTEXT.md 就另建第二套来源。沿用已有 GameDesign、OpenSpec 或 ADR 约定。
确需新术语表时参考 [CONTEXT-FORMAT.md](./CONTEXT-FORMAT.md)。
仅为难逆转、有实际替代方案且需保留理由的决定建立 ADR，参考 [ADR-FORMAT.md](./ADR-FORMAT.md)。
保留用户最终裁决，不把建议写成已批准。
