#!/usr/bin/env python3
"""Persistent, non-interrupting task queue for long-lived Codex role windows."""

from __future__ import annotations

import argparse
import json
import os
import sys
import time
from contextlib import contextmanager
from datetime import datetime, timezone
from pathlib import Path


SCHEMA_VERSION = 1
ALLOWED_PREEMPT_REASONS = {"s0", "s1", "explicit-user", "invalidated"}


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")


def default_queue_path() -> Path:
    return Path(__file__).resolve().parents[1] / ".ai" / "dispatch" / "task-queue.local.json"


@contextmanager
def queue_lock(queue_path: Path, timeout_seconds: float = 5.0):
    lock_path = queue_path.with_suffix(queue_path.suffix + ".lock")
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    deadline = time.monotonic() + timeout_seconds
    fd = None
    while fd is None:
        try:
            fd = os.open(str(lock_path), os.O_CREAT | os.O_EXCL | os.O_WRONLY)
            os.write(fd, f"{os.getpid()}\n".encode("utf-8"))
        except FileExistsError:
            if time.monotonic() >= deadline:
                raise RuntimeError(f"queue lock timeout: {lock_path}")
            time.sleep(0.05)
    try:
        yield
    finally:
        if fd is not None:
            os.close(fd)
        try:
            lock_path.unlink()
        except FileNotFoundError:
            pass


def empty_queue(project: str) -> dict:
    return {
        "schemaVersion": SCHEMA_VERSION,
        "project": project,
        "sequence": 0,
        "roles": {},
    }


def empty_role() -> dict:
    return {"active": None, "pending": [], "suspended": [], "completed": []}


def load_queue(path: Path, project: str) -> dict:
    if not path.exists():
        return empty_queue(project)
    data = json.loads(path.read_text(encoding="utf-8"))
    if data.get("schemaVersion") != SCHEMA_VERSION:
        raise RuntimeError(f"unsupported queue schema: {data.get('schemaVersion')}")
    return data


def save_queue(path: Path, data: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    temp_path = path.with_suffix(path.suffix + f".{os.getpid()}.tmp")
    temp_path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    os.replace(temp_path, path)


def role_state(data: dict, role: str) -> dict:
    return data["roles"].setdefault(role, empty_role())


def all_tasks(state: dict):
    if state["active"]:
        yield state["active"]
    yield from state["pending"]
    yield from state["suspended"]
    yield from state["completed"]


def find_task(state: dict, task_id: str):
    return next((task for task in all_tasks(state) if task["taskId"] == task_id), None)


def select_next(state: dict):
    if state["suspended"]:
        task = state["suspended"].pop()
        task["state"] = "active"
        task["resumedAt"] = utc_now()
        return task
    if not state["pending"]:
        return None
    state["pending"].sort(key=lambda task: (task["priority"], task["enqueuedSeq"]))
    task = state["pending"].pop(0)
    task["state"] = "active"
    task["startedAt"] = task.get("startedAt") or utc_now()
    return task


def output(payload: dict) -> None:
    print(json.dumps(payload, ensure_ascii=False, indent=2))


def mutate(args, action):
    queue_path = Path(args.queue_file).resolve()
    project = args.project or queue_path.parents[2].name
    with queue_lock(queue_path):
        data = load_queue(queue_path, project)
        result = action(data)
        save_queue(queue_path, data)
    output(result)


def command_init(args):
    def action(data):
        for role in args.roles:
            role_state(data, role)
        return {"ok": True, "queueFile": str(Path(args.queue_file).resolve()), "roles": sorted(data["roles"])}

    mutate(args, action)


def command_enqueue(args):
    def action(data):
        state = role_state(data, args.role)
        if find_task(state, args.task_id):
            raise RuntimeError(f"duplicate task id for role {args.role}: {args.task_id}")
        data["sequence"] += 1
        task = {
            "taskId": args.task_id,
            "title": args.title,
            "priority": args.priority,
            "enqueuedSeq": data["sequence"],
            "enqueuedAt": utc_now(),
            "source": args.source,
            "dispatchPath": args.dispatch_path,
            "summary": args.summary,
            "state": "pending",
        }
        state["pending"].append(task)
        return {
            "ok": True,
            "role": args.role,
            "task": task,
            "activeUnchanged": state["active"] is not None,
            "mustMessageRunningWindow": False,
        }

    mutate(args, action)


def command_claim(args):
    def action(data):
        state = role_state(data, args.role)
        if state["active"]:
            return {"ok": True, "claimed": False, "reason": "active-exists", "active": state["active"]}
        state["active"] = select_next(state)
        return {"ok": True, "claimed": state["active"] is not None, "active": state["active"]}

    mutate(args, action)


def command_complete(args):
    def action(data):
        state = role_state(data, args.role)
        active = state["active"]
        if not active or active["taskId"] != args.task_id:
            raise RuntimeError(f"task is not active for role {args.role}: {args.task_id}")
        active["state"] = "completed"
        active["completedAt"] = utc_now()
        active["result"] = args.result
        state["completed"].append(active)
        state["active"] = None
        next_task = None
        if args.claim_next:
            next_task = select_next(state)
            state["active"] = next_task
        return {"ok": True, "completed": active, "nextActive": next_task}

    mutate(args, action)


def command_preempt(args):
    if args.reason_kind not in ALLOWED_PREEMPT_REASONS:
        raise RuntimeError(f"preemption reason not allowed: {args.reason_kind}")
    if not args.checkpoint.strip() or not args.approved_by.strip():
        raise RuntimeError("preemption requires checkpoint and approved-by")

    def action(data):
        state = role_state(data, args.role)
        target_index = next(
            (index for index, task in enumerate(state["pending"]) if task["taskId"] == args.task_id),
            None,
        )
        if target_index is None:
            raise RuntimeError(f"preemption target is not pending: {args.task_id}")
        current = state["active"]
        if current:
            current["state"] = "suspended"
            current["suspendedAt"] = utc_now()
            current["checkpoint"] = args.checkpoint
            current["preemptReason"] = args.reason_kind
            current["preemptApprovedBy"] = args.approved_by
            state["suspended"].append(current)
        target = state["pending"].pop(target_index)
        target["state"] = "active"
        target["startedAt"] = target.get("startedAt") or utc_now()
        target["preemptedPreviousTask"] = current["taskId"] if current else None
        state["active"] = target
        return {"ok": True, "preempted": current, "active": target}

    mutate(args, action)


def command_status(args):
    queue_path = Path(args.queue_file).resolve()
    project = args.project or queue_path.parents[2].name
    data = load_queue(queue_path, project)
    if args.role:
        output({"schemaVersion": data["schemaVersion"], "project": data["project"], "role": args.role, "state": role_state(data, args.role)})
    else:
        output(data)


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--queue-file", default=str(default_queue_path()))
    parser.add_argument("--project", default=None)
    subparsers = parser.add_subparsers(dest="command", required=True)

    init_parser = subparsers.add_parser("init")
    init_parser.add_argument("--roles", nargs="+", required=True)
    init_parser.set_defaults(func=command_init)

    enqueue_parser = subparsers.add_parser("enqueue")
    enqueue_parser.add_argument("--role", required=True)
    enqueue_parser.add_argument("--task-id", required=True)
    enqueue_parser.add_argument("--title", required=True)
    enqueue_parser.add_argument("--priority", type=int, default=50, help="Lower value runs first within Pending")
    enqueue_parser.add_argument("--source", default="")
    enqueue_parser.add_argument("--dispatch-path", default="")
    enqueue_parser.add_argument("--summary", default="")
    enqueue_parser.set_defaults(func=command_enqueue)

    claim_parser = subparsers.add_parser("claim")
    claim_parser.add_argument("--role", required=True)
    claim_parser.set_defaults(func=command_claim)

    complete_parser = subparsers.add_parser("complete")
    complete_parser.add_argument("--role", required=True)
    complete_parser.add_argument("--task-id", required=True)
    complete_parser.add_argument("--result", default="")
    complete_parser.add_argument("--claim-next", action="store_true")
    complete_parser.set_defaults(func=command_complete)

    preempt_parser = subparsers.add_parser("preempt")
    preempt_parser.add_argument("--role", required=True)
    preempt_parser.add_argument("--task-id", required=True)
    preempt_parser.add_argument("--reason-kind", choices=sorted(ALLOWED_PREEMPT_REASONS), required=True)
    preempt_parser.add_argument("--checkpoint", required=True)
    preempt_parser.add_argument("--approved-by", required=True)
    preempt_parser.set_defaults(func=command_preempt)

    status_parser = subparsers.add_parser("status")
    status_parser.add_argument("--role")
    status_parser.set_defaults(func=command_status)
    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()
    try:
        args.func(args)
        return 0
    except (RuntimeError, OSError, json.JSONDecodeError) as exc:
        print(json.dumps({"ok": False, "error": str(exc)}, ensure_ascii=False), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
