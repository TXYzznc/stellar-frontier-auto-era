# Agent Watchdog

`tools/agent_watchdog.py` is a model-free decision engine. It reads role queues and the window registry, then emits idempotent `wake` decisions only when a role has runnable work and no active task. It never reads task bodies or sends messages.

Use `python tools/agent_watchdog.py --role <role> --append` from an external host scheduler. The scheduler is responsible for consuming `watchdog-decisions.local.jsonl` and invoking the supported Codex window wake API. Explicit pause flags suppress wakeups; `待验收` and `等待协作` are not runnable and do not wake a role.
