# 任务

- [ ] 1. 扩展队列任务生命周期：ResultReady、AwaitingProducerAcceptance、Accepted、Rework、Blocked。
- [ ] 2. 实现结果包校验与制作人验收/退回命令。
- [ ] 3. 扩展事件投递状态、退避重试、制作人确认和幂等处理。
- [ ] 4. 增加步骤级阻塞、依赖重试和全部不可执行才整体阻塞的调度规则。
- [ ] 5. 增加自动验证：无回传不得 Accepted、证据缺失不得 Accepted、回传失败会重试、阻塞不冻结安全步骤。
- [ ] 6. 更新窗口初始化与调度契约，禁止“写 completed 后直接空闲”。
