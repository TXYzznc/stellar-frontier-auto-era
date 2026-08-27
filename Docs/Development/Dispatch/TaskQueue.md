# 长期窗口持久任务队列

本协议解决“向运行中窗口下发新任务会立刻打断当前工作”的问题。窗口消息不再承担普通任务排队；
正式派发来源仍是派发单、OpenSpec和专业文档，队列只保存本机执行顺序和恢复状态。

## 状态文件与工具

- 本机队列：`.ai/dispatch/task-queue.local.json`，被Git忽略，但在项目重启和窗口上下文恢复后保留。
- 管理工具：`python tools/window_task_queue.py`。
- 每个职能独立维护`active`、`pending`、`suspended`和`completed`。
- 写入采用锁文件与原子替换；禁止人工同时编辑队列JSON。

初始化示例：

```powershell
python tools/window_task_queue.py init --roles producer git-integration art-3d art-concept-3d art-2d client backend design qa
```

## 普通入队不抢占

```powershell
python tools/window_task_queue.py enqueue `
  --role art-concept-3d `
  --task-id b03-g02-axonometric `
  --title "G02轴测效果图" `
  --priority 10 `
  --dispatch-path "openspec/changes/b03-parallel-art-art004-art005-production-rnd/tasks.md"
```

- 一个窗口同时只能有一个`active`任务。
- 新的独立任务永远进入`pending`，即使优先级更高，也不得替换`active`。
- `pending`按数字优先级从小到大排序；同优先级按`enqueuedSeq`执行，未指定优先级默认为50。
- 下发方发现目标窗口正在运行时，只写队列，不调用窗口消息接口发送任务正文。
- 若目标窗口空闲且没有`active`，可`claim`后用一条启动消息唤醒；运行中的窗口不接收普通派发消息。

## 窗口领取与收尾

窗口启动、上下文恢复和每项任务完成收尾后必须读取自己的队列：

```powershell
python tools/window_task_queue.py status --role art-concept-3d
python tools/window_task_queue.py claim --role art-concept-3d
python tools/window_task_queue.py complete --role art-concept-3d --task-id b03-g01-axonometric --claim-next
```

完成当前任务必须先保存产物、自验、记录证据、释放不再需要的共享资源并完成规定汇报。使用
`--claim-next`后，工具优先恢复曾被合法抢占的`suspended`任务，再按“优先级＋FIFO”领取下一项。

## 允许抢占的唯一条件

普通新任务、后续对象、优先级变化、普通知悉和制作人希望加速均不得抢占。只有以下情况允许在
安全检查点调用`preempt`：

1. S0／S1问题或资源／数据安全风险，并经用户或制作人明确批准；
2. 用户明确要求“立即切换”；
3. 新信息证明继续当前产物必然作废。

```powershell
python tools/window_task_queue.py preempt `
  --role art-3d `
  --task-id urgent-fix `
  --reason-kind s1 `
  --checkpoint "源文件已保存，Unity与DCC已释放" `
  --approved-by producer
```

抢占必须记录检查点和批准来源。被抢占任务进入`suspended`，紧急任务完成后优先自动恢复。无法形成
安全检查点时不得强制切换。

## 当前任务修订与独立新任务

- 修改当前产物的合同修订：若继续工作会造成确定性返工，可在安全检查点合并到当前任务；这不是
  开始第二项独立任务。
- 与当前产物无关的新对象、新功能或后续阶段：进入`pending`。
- 仅提供将来会用到的参考信息：写入对应派发单／OpenSpec或队列摘要，不直接消息打断窗口。
- 用户直接向运行中窗口提出新任务时，窗口必须先分类；若不是合法抢占，登记到`pending`后继续当前任务。

## 3D前置生产的专用阶段门

三视图与生产说明任务只能在对应主美技术文档经制作人对照归档需求验收通过后入队。主美刚完成
文档不等于允许3D原画开工。验收未通过时，3D原画队列中不得出现该对象的三视图／生产说明任务。
