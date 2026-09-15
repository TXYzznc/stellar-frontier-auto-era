"""AutoEra Agent Watchdog 控制面板。

读取 Windows 计划任务中的守护状态，并让用户把本机注册表绑定到真实的
Codex 长期窗口：角色与窗口不再写死，可由“更新窗口”从本机置顶窗口重建，
也可直接在列表里双击角色单元格改绑。
"""

from __future__ import annotations

import json
import os
import socket
import sqlite3
import subprocess
import sys
import time
import tkinter as tk
from datetime import datetime, timezone
from pathlib import Path
from tkinter import messagebox, ttk

from watchdog_scheduler import MAX_INTERVAL_SECONDS, MIN_INTERVAL_SECONDS, configure, status
import watchdog_backoff as backoff

BASE = Path(sys.executable if getattr(sys, 'frozen', False) else __file__).resolve()
ROOT = next((p for p in BASE.parents if (p / '.ai/dispatch/window-registry.local.json').is_file()), Path(__file__).resolve().parents[1])
DISPATCH = ROOT / '.ai/dispatch'
CFG = DISPATCH / 'watchdog-ui.local.json'
REG = DISPATCH / 'window-registry.local.json'
QUEUE = DISPATCH / 'task-queue.local.json'
LIFECYCLE = DISPATCH / 'task-lifecycle.local.json'
HEARTBEATS = DISPATCH / 'heartbeats.local.json'
BINDINGS = DISPATCH / 'watchdog-ui-bindings.local.json'
IMGPROXY_DIR = ROOT / 'tools' / 'imgproxy'
IMGPROXY_CONFIG = IMGPROXY_DIR / 'config.json'
IMGPROXY_START = IMGPROXY_DIR / 'start_background.cmd'
IMGPROXY_LOG = IMGPROXY_DIR / 'logs' / 'img_proxy.log'
PROXY_LEVEL_COLORS = {'ok': '#1a7f37', 'warn': '#c77700', 'down': '#c0392b'}
CODEX_HOME = Path(os.environ.get('CODEX_HOME') or (Path.home() / '.codex'))
GLOBAL_STATE = CODEX_HOME / '.codex-global-state.json'
CATALOG_DB = CODEX_HOME / 'sqlite' / 'codex-dev.db'

# 标题命中即自动分配职能键；未命中的置顶窗口由用户手工指定角色。
ROLE_BY_TITLE = {
    'AutoEra｜制作人·协作验收': 'producer-collab',
    'AutoEra｜制作人': 'producer',
    'AutoEra｜Git集成': 'git-integration',
    'AutoEra｜快速执行': 'rapid-executor',
    'AutoEra｜程序（后端）': 'backend',
    'AutoEra｜3D原画': 'art-concept-3d',
    'AutoEra｜2D美术': 'art-2d',
    'AutoEra｜3D美术': 'art-3d',
    'AutoEra｜客户端': 'client',
    'AutoEra｜策划': 'design',
    'AutoEra｜测试': 'qa',
}


def load(path: Path, default):
    try:
        return json.loads(path.read_text(encoding='utf8'))
    except Exception:
        return default


def save_json(path: Path, data) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_suffix(path.suffix + '.tmp')
    tmp.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf8')
    tmp.replace(path)


def derive_role(title: str) -> str:
    title = (title or '').strip()
    if title in ROLE_BY_TITLE:
        return ROLE_BY_TITLE[title]
    if title.startswith('AutoEra'):
        name = title.split('｜', 1)[-1].strip()
        return 'autorea-' + (name or 'window')
    return ''


def default_port(path: str) -> int:
    return 8091 if 'ArtResource' in (path or '') else 8092


def human_delta(seconds: int) -> str:
    seconds = max(0, int(seconds))
    if seconds < 60:
        return f'{seconds}秒'
    if seconds < 3600:
        return f'{seconds // 60}分钟'
    if seconds < 86400:
        return f'{seconds // 3600}小时{(seconds % 3600) // 60:02d}分'
    return f'{seconds // 86400}天{(seconds % 86400) // 3600}小时'


def format_instant(value) -> str:
    """ISO 字符串或 epoch 时间 → 本地时间＋相对时长，便于人工核对是否仍在更新。"""
    if value in (None, ''):
        return ''
    try:
        if isinstance(value, (int, float)):
            seconds = float(value)
            if seconds > 1e12:  # 毫秒精度
                seconds /= 1000.0
            dt = datetime.fromtimestamp(seconds, tz=timezone.utc)
        else:
            text = str(value).strip()
            if text.endswith('Z'):
                text = text[:-1] + '+00:00'
            dt = datetime.fromisoformat(text)
            if dt.tzinfo is None:
                dt = dt.replace(tzinfo=timezone.utc)
        delta = int((datetime.now(timezone.utc) - dt).total_seconds())
        rel = f'（{human_delta(delta)}前）' if delta >= 0 else '（刚刚）'
        return dt.astimezone().strftime('%Y-%m-%d %H:%M:%S') + rel
    except Exception:
        return str(value)


def thread_activity(thread_id: str) -> str:
    """Codex 侧最近一次会话更新时间；目录库不可用时退回 rollout 文件修改时间。"""
    if not thread_id:
        return ''
    meta = read_catalog().get(thread_id)
    if not meta or not meta.get('updatedAt'):
        meta = scan_session_meta(thread_id) or {}
    return format_instant(meta.get('updatedAt'))


def read_pinned_ids() -> list[str]:
    """按侧边栏显示顺序返回本机置顶窗口 id。"""
    state = load(GLOBAL_STATE, {})
    atom = state.get('electron-persisted-atom-state', {}) if isinstance(state, dict) else {}
    ordered: list[str] = []
    # 权威来源：app-server 的置顶顺序与旧的 pinned-thread-ids 保持一致。
    for source in (atom.get('app-server-pinned-thread-order-v1'), state.get('pinned-thread-ids')):
        for item in source or []:
            if isinstance(item, str) and item not in ordered:
                ordered.append(item)
    if not ordered:
        # 仅当权威来源缺失时才退回旧格式；该字段可能是历史遗留的超集。
        for key in atom.get('unified-sidebar-pinned-order-v1') or []:
            if isinstance(key, str) and key.startswith('codex:thread:'):
                tid = key.rsplit(':', 1)[-1]
                if tid not in ordered:
                    ordered.append(tid)
    return ordered


def read_catalog() -> dict[str, dict]:
    """从 Codex 本地会话目录读取标题与工作目录。"""
    if not CATALOG_DB.is_file():
        return {}
    out: dict[str, dict] = {}
    try:
        con = sqlite3.connect('file:' + CATALOG_DB.as_posix() + '?mode=ro', uri=True)
        try:
            rows = con.execute(
                'select thread_id, display_title, cwd, source_updated_at, project_id from local_thread_catalog'
            ).fetchall()
        finally:
            con.close()
    except Exception:
        return {}
    for thread_id, title, cwd, updated, project_id in rows:
        out[thread_id] = {
            'title': title or '',
            'cwd': cwd or '',
            'updatedAt': updated,
            'projectId': project_id or '',
        }
    return out


def scan_session_meta(thread_id: str) -> dict:
    """目录库不可用时的兜底：从 rollout 首行读取 cwd。"""
    sessions = CODEX_HOME / 'sessions'
    if not sessions.is_dir():
        return {}
    for path in sessions.rglob('*' + thread_id + '.jsonl'):
        try:
            with path.open(encoding='utf8') as fh:
                first = json.loads(fh.readline())
            payload = first.get('payload') or {}
            return {'title': '', 'cwd': payload.get('cwd', ''), 'updatedAt': path.stat().st_mtime, 'projectId': ''}
        except Exception:
            continue
    return {}


class App:
    def __init__(self, root: tk.Tk) -> None:
        self.root = root
        self.root.title('AutoEra Agent Watchdog')
        self.root.geometry('1180x760')
        c = load(CFG, {'enabled': False, 'interval': MIN_INTERVAL_SECONDS})
        self.enabled = tk.BooleanVar(value=c.get('enabled', False))
        self.interval = tk.IntVar(value=c.get('interval', MIN_INTERVAL_SECONDS))
        self.only_autorea = tk.BooleanVar(value=True)
        self.results: dict = {}

        bar = ttk.Frame(root)
        bar.pack(fill='x', padx=10, pady=8)
        ttk.Checkbutton(bar, text='启用守护', variable=self.enabled, command=self.apply_settings).pack(side='left')
        ttk.Label(bar, text='检查间隔(秒)').pack(side='left', padx=(16, 4))
        spin = ttk.Spinbox(bar, from_=MIN_INTERVAL_SECONDS, to=MAX_INTERVAL_SECONDS, increment=60,
                           textvariable=self.interval, width=7, command=self.apply_settings)
        spin.pack(side='left')
        spin.bind('<Return>', self.apply_settings)
        spin.bind('<FocusOut>', self.apply_settings)
        ttk.Button(bar, text='立即检查', command=self.check).pack(side='left', padx=10)
        ttk.Button(bar, text='刷新', command=self.refresh).pack(side='left', padx=4)
        ttk.Button(bar, text='更新窗口', command=self.sync_windows).pack(side='left', padx=4)
        ttk.Button(bar, text='解除退避', command=self.clear_backoff).pack(side='left', padx=4)
        ttk.Button(bar, text='安全暂停', command=self.pause).pack(side='left')

        bar2 = ttk.Frame(root)
        bar2.pack(fill='x', padx=10)
        ttk.Checkbutton(bar2, text='仅纳入 AutoEra 窗口', variable=self.only_autorea).pack(side='left')
        ttk.Label(bar2, text='测试角色').pack(side='left', padx=(20, 4))
        self.testRole = tk.StringVar(value='art-3d')
        self.roleBox = ttk.Combobox(bar2, textvariable=self.testRole, values=[], width=18)
        self.roleBox.pack(side='left')
        ttk.Button(bar2, text='创建虚拟任务', command=self.create_test).pack(side='left', padx=6)
        ttk.Label(bar2, text='双击 role 单元格可改绑角色').pack(side='right')

        bar3 = ttk.Frame(root)
        bar3.pack(fill='x', padx=10, pady=(6, 0))
        ttk.Label(bar3, text='图片预处理代理').pack(side='left')
        self.proxyDot = tk.Canvas(bar3, width=14, height=14, highlightthickness=0)
        self.proxyDot.pack(side='left', padx=(8, 4))
        self.proxyDotId = self.proxyDot.create_oval(3, 3, 11, 11, fill=PROXY_LEVEL_COLORS['down'], outline='')
        self.proxyText = tk.StringVar(value='检测中…')
        ttk.Label(bar3, textvariable=self.proxyText).pack(side='left')
        ttk.Button(bar3, text='拉起代理', command=self.start_image_proxy).pack(side='left', padx=10)
        ttk.Button(bar3, text='检查代理状态', command=self.refresh_proxy_status).pack(side='left', padx=4)
        self.proxyChecked = tk.StringVar(value='')
        ttk.Label(bar3, textvariable=self.proxyChecked).pack(side='right')
        ttk.Label(bar3, text='绿=运行中  黄=异常  红=未运行').pack(side='right', padx=(0, 12))

        pane = ttk.Panedwindow(root, orient='vertical')
        pane.pack(fill='both', expand=True, padx=10, pady=(8, 0))

        top = ttk.Frame(pane)
        cols = ('role', 'title', 'thread', 'state', 'task', 'decision', 'delivery')
        self.tree = ttk.Treeview(top, columns=cols, show='headings')
        for name, width in zip(cols, (130, 210, 250, 110, 170, 190, 110)):
            self.tree.heading(name, text=name)
            self.tree.column(name, width=width, stretch=name in ('title', 'thread'))
        vs = ttk.Scrollbar(top, orient='vertical', command=self.tree.yview)
        self.tree.configure(yscrollcommand=vs.set)
        self.tree.pack(side='left', fill='both', expand=True)
        vs.pack(side='right', fill='y')
        pane.add(top, weight=3)

        bottom = ttk.LabelFrame(pane, text='选中窗口详情')
        self.detail = tk.Text(bottom, height=14, wrap='word', state='disabled')
        dvs = ttk.Scrollbar(bottom, orient='vertical', command=self.detail.yview)
        self.detail.configure(yscrollcommand=dvs.set)
        self.detail.pack(side='left', fill='both', expand=True, padx=(4, 0), pady=4)
        dvs.pack(side='right', fill='y', pady=4)
        pane.add(bottom, weight=2)

        self.tree.bind('<<TreeviewSelect>>', self.show_detail)
        self.tree.bind('<Double-1>', self.begin_role_edit)
        self.edit = None

        self.status = tk.StringVar(value='正在读取 Windows 守护任务状态…')
        ttk.Label(root, textvariable=self.status).pack(anchor='w', padx=10, pady=6)
        self.refresh()
        self.refresh_proxy_status()
        self.root.after(10000, self.poll_proxy)

    # ---------- 图片预处理代理 ----------

    def proxy_settings(self) -> dict:
        return load(IMGPROXY_CONFIG, {})

    def proxy_endpoint(self) -> tuple[str, int]:
        cfg = self.proxy_settings()
        host = str(cfg.get('listen_host') or '127.0.0.1')
        try:
            port = int(cfg.get('listen_port') or 15722)
        except (TypeError, ValueError):
            port = 15722
        return host, port

    @staticmethod
    def probe_port(host: str, port: int, timeout: float = 0.4) -> bool:
        try:
            with socket.create_connection((host, port), timeout=timeout):
                return True
        except OSError:
            return False

    def proxy_config_ok(self) -> bool:
        """Codex 的 base_url 是否仍指向本代理（guard 失守时提示异常）。"""
        cfg = self.proxy_settings()
        target = str(cfg.get('guard_target_base_url') or '').strip()
        path = str(cfg.get('guard_codex_config') or '').strip()
        if not target or not path:
            return True
        try:
            return target in Path(path).expanduser().read_text(encoding='utf8')
        except Exception:
            return False

    def paint_proxy(self, level: str, text: str) -> None:
        self.proxyDot.itemconfigure(self.proxyDotId,
                                    fill=PROXY_LEVEL_COLORS.get(level, PROXY_LEVEL_COLORS['down']))
        self.proxyText.set(text)
        self.proxyChecked.set('上次检测 ' + time.strftime('%H:%M:%S'))

    def refresh_proxy_status(self, event=None) -> bool:
        host, port = self.proxy_endpoint()
        if not self.probe_port(host, port):
            self.paint_proxy('down', f'未运行 · {host}:{port}')
            return False
        if self.proxy_config_ok():
            self.paint_proxy('ok', f'运行中 · {host}:{port}')
        else:
            self.paint_proxy('warn', f'运行中，但 Codex 未指向代理 · {host}:{port}')
        return True

    def start_image_proxy(self):
        host, port = self.proxy_endpoint()
        if self.probe_port(host, port):
            self.refresh_proxy_status()
            self.status.set(f'图片代理已在运行，无需重复拉起（{host}:{port}）')
            return
        if not IMGPROXY_START.is_file():
            messagebox.showerror('拉起代理', f'未找到启动脚本：\n{IMGPROXY_START}')
            return
        try:
            subprocess.Popen(['cmd.exe', '/c', str(IMGPROXY_START)], cwd=str(IMGPROXY_DIR),
                             creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        except Exception as exc:
            messagebox.showerror('拉起代理', '启动图片代理失败：' + str(exc))
            return
        self.paint_proxy('warn', f'正在启动… · {host}:{port}')
        self.status.set('已请求拉起图片代理，正在等待端口就绪…')
        self.wait_for_proxy(host, port, 20)

    def wait_for_proxy(self, host: str, port: int, remaining: int) -> None:
        if self.probe_port(host, port):
            self.refresh_proxy_status()
            self.status.set(f'图片代理已就绪：{host}:{port}')
            return
        if remaining <= 0:
            self.paint_proxy('down', f'启动未确认 · {host}:{port}')
            self.status.set('图片代理未在预期时间内就绪；请查看 tools/imgproxy/logs/launcher.log 与 img_proxy.log')
            return
        self.root.after(500, lambda: self.wait_for_proxy(host, port, remaining - 1))

    def poll_proxy(self) -> None:
        try:
            self.refresh_proxy_status()
        except Exception:
            pass
        self.root.after(10000, self.poll_proxy)

    # ---------- 设置与任务计划 ----------

    def save_cfg(self) -> None:
        save_json(CFG, {'enabled': self.enabled.get(), 'interval': int(self.interval.get())})

    def apply_settings(self, event=None):
        try:
            actual = configure(ROOT, self.enabled.get(), self.interval.get())
            interval = int(actual.get('intervalSeconds') or MIN_INTERVAL_SECONDS)
            enabled = bool(actual.get('enabled', False))
            self.enabled.set(enabled)
            self.interval.set(interval)
            self.save_cfg()
            self.status.set(('已启用' if enabled else '已暂停') + f'：Windows 后台任务每 {interval} 秒检查')
            self.refresh()
        except Exception as exc:
            self.status.set('配置失败：' + str(exc))

    def pause(self):
        self.enabled.set(False)
        self.apply_settings()

    def clear_backoff(self):
        """清除全部投递退避状态，用于窗口修复后立即恢复自动唤醒。"""
        backoff.reset()
        self.status.set('已清除全部投递退避状态')
        self.refresh()

    def create_test(self):
        tid = 'ui-watchdog-test-' + time.strftime('%Y%m%d%H%M%S')
        role = self.testRole.get()
        subprocess.run(['python', str(ROOT / 'tools/window_task_queue.py'), 'enqueue', '--role', role,
                        '--task-id', tid, '--title', '桌面守护虚拟测试任务', '--priority', '0',
                        '--source', 'ui-test', '--summary', '仅验证桌面守护唤醒链路；完成后清理，不修改项目文件。'], cwd=ROOT)
        self.status.set('已创建虚拟任务 ' + tid)
        self.check()

    def check(self):
        if not self.enabled.get():
            self.status.set('已暂停，未执行检查')
            return
        p = subprocess.run(['powershell.exe', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                            str(ROOT / 'tools/Start-AgentWatchdog.ps1'), '-Once'],
                           cwd=ROOT, capture_output=True, text=True, encoding='utf8', errors='replace')
        self.status.set('检查完成 ' + time.strftime('%H:%M:%S') if p.returncode == 0
                        else '检查失败：' + p.stderr.strip()[-160:])
        self.refresh()

    # ---------- 注册表绑定 ----------

    def bindings(self) -> dict:
        return load(BINDINGS, {})

    def sync_windows(self):
        """移除旧数据，读取本机所有置顶窗口后重建注册表。"""
        pinned = read_pinned_ids()
        if not pinned:
            messagebox.showwarning('更新窗口', f'未能读取到置顶窗口。\n请确认存在：{GLOBAL_STATE}')
            return
        catalog = read_catalog()
        registry = load(REG, {})
        old = registry.get('windows', {}) or {}
        by_thread = {v.get('threadId'): (k, v) for k, v in old.items() if v.get('threadId')}
        saved = self.bindings()
        windows: dict[str, dict] = {}
        skipped: list[str] = []
        for tid in pinned:
            meta = catalog.get(tid) or scan_session_meta(tid)
            title = meta.get('title') or ''
            role = saved.get(tid) or derive_role(title)
            if not role:
                if self.only_autorea.get():
                    skipped.append(title or tid)
                    continue
                role = 'window-' + tid[:8]
            base, suffix = role, 2
            while role in windows:
                role = f'{base}-{suffix}'
                suffix += 1
            prev = by_thread.get(tid, (None, {}))[1]
            windows[role] = {
                'title': title or prev.get('title', ''),
                'threadId': tid,
                'hostId': prev.get('hostId', 'local'),
                'projectId': meta.get('projectId') or prev.get('projectId', ''),
                'projectPath': meta.get('cwd') or prev.get('projectPath', ''),
                'status': 'ready',
                'unityPort': prev.get('unityPort', default_port(meta.get('cwd', ''))),
            }
        registry['windows'] = windows
        save_json(REG, registry)
        self.roleBox.configure(values=list(windows.keys()))
        parts = [f'已更新 {len(windows)} 个窗口']
        if skipped:
            parts.append(f'跳过非 AutoEra 置顶 {len(skipped)} 个')
        self.status.set('，'.join(parts))
        self.refresh()

    def set_role(self, old_role: str, new_role: str) -> bool:
        new_role = (new_role or '').strip()
        if not new_role or new_role == old_role:
            return False
        registry = load(REG, {})
        windows = registry.get('windows', {}) or {}
        if new_role in windows:
            messagebox.showwarning('改绑角色', f'角色 {new_role} 已被占用，请换一个名称。')
            return False
        entry = windows.pop(old_role, None)
        if entry is None:
            return False
        windows[new_role] = entry
        registry['windows'] = windows
        save_json(REG, registry)
        saved = self.bindings()
        if entry.get('threadId'):
            saved[entry['threadId']] = new_role
            save_json(BINDINGS, saved)
        self.status.set(f'角色已改绑：{old_role} -> {new_role}')
        return True

    def begin_role_edit(self, event):
        if self.tree.identify_column(event.x) != '#1':
            return
        row = self.tree.identify_row(event.y)
        if not row:
            return
        x, y, w, h = self.tree.bbox(row, '#1')
        old = self.tree.set(row, 'role')
        self.edit = tk.Entry(self.tree)
        self.edit.insert(0, old)
        self.edit.place(x=x, y=y, width=w, height=h)
        self.edit.focus_set()
        self.edit.select_range(0, 'end')

        def commit(_event=None):
            if self.edit is None:
                return
            value = self.edit.get()
            self.edit.destroy()
            self.edit = None
            if self.set_role(old, value):
                self.refresh()

        self.edit.bind('<Return>', commit)
        self.edit.bind('<FocusOut>', commit)
        self.edit.bind('<Escape>', lambda e: (self.edit.destroy(), setattr(self, 'edit', None)))

    # ---------- 列表与详情 ----------

    def refresh(self, results=None):
        if results is not None:
            self.results = results
        if results is None:
            try:
                actual = status()
                if actual.get('exists'):
                    self.enabled.set(bool(actual.get('enabled', False)))
                    self.interval.set(int(actual.get('intervalSeconds') or MIN_INTERVAL_SECONDS))
                    self.save_cfg()
                    self.status.set(('已启用' if self.enabled.get() else '已暂停')
                                    + f'：Windows 后台任务每 {self.interval.get()} 秒检查')
                else:
                    self.status.set('未配置：勾选“启用守护”后将创建 Windows 后台任务')
            except Exception as exc:
                self.status.set('读取任务计划程序失败：' + str(exc))

        self.tree.delete(*self.tree.get_children())
        windows = load(REG, {}).get('windows', {}) or {}
        roles = load(QUEUE, {}).get('roles', {}) or {}
        self.roleBox.configure(values=list(windows.keys()))
        labels = {
            'no_runnable_work': '无可执行任务',
            'lifecycle_waiting_or_complete': '任务等待验收/协作或已完成',
            'window_actually_running': '窗口正在运行',
            'runnable_task_but_window_not_running': '有可执行任务，窗口未运行',
            'explicitly_paused': '已明确暂停',
        }
        for role, w in windows.items():
            entry = roles.get(role, {}) or {}
            active = entry.get('active') or {}
            result = (self.results or {}).get(role, {}) or {}
            reason = result.get('reason', '未检查')
            verdict = backoff.evaluate(role, w.get('threadId'))
            decision = labels.get(reason, reason)
            if verdict.get('defer'):
                decision += f"｜退避中({verdict.get('reason')})"
            self.tree.insert('', 'end', values=(
                role,
                w.get('title', ''),
                w.get('threadId') or 'None',
                result.get('windowState', w.get('actualState', w.get('status', '未接入状态桥接'))),
                active.get('taskId', ''),
                decision,
                result.get('delivery', '—'),
            ))
        self.show_detail()

    def selected_role(self) -> str:
        sel = self.tree.selection()
        return self.tree.set(sel[0], 'role') if sel else ''

    def show_detail(self, event=None):
        role = self.selected_role()
        text = '在上方列表选中一行，这里显示该窗口的完整绑定与任务状态。'
        if role:
            windows = load(REG, {}).get('windows', {}) or {}
            w = windows.get(role, {})
            entry = (load(QUEUE, {}).get('roles', {}) or {}).get(role, {}) or {}
            life = load(LIFECYCLE, {})
            active = entry.get('active') or {}
            pending = [x.get('taskId', '') for x in entry.get('pending') or []]
            suspended = [x.get('taskId', '') for x in entry.get('suspended') or []]
            result = (self.results or {}).get(role, {}) or {}
            hb = (load(HEARTBEATS, {}).get(role) or {})
            task_id = active.get('taskId', '')
            life_entry = next((v for v in life.values() if task_id and v.get('taskId') == task_id), None) or {}
            state = life_entry.get('state')
            verdict = backoff.evaluate(role, w.get('threadId'))
            rows = [
                f'角色 (role)        : {role}',
                f'窗口标题 (title)   : {w.get("title", "")}',
                f'线程 (threadId)    : {w.get("threadId") or "未绑定"}',
                f'主机 (hostId)      : {w.get("hostId", "")}',
                f'项目 (projectId)   : {w.get("projectId") or "未记录"}',
                f'工作目录           : {w.get("projectPath", "")}',
                f'Unity 端口         : {w.get("unityPort", "")}',
                f'注册状态           : {w.get("status", "")}',
                '',
                f'上次心跳           : ' + (format_instant(hb.get('lastHeartbeat')) or '未记录（窗口尚未写心跳）'),
                f'心跳证据           : ' + (hb.get('lastEvidence') or '—'),
                f'Codex 会话更新     : ' + (thread_activity(w.get('threadId')) or '未获取'),
                '',
                f'窗口运行状态       : {result.get("windowState", "未检查")}',
                f'守护判定           : {result.get("reason", "未检查")}',
                f'投递结果           : {result.get("delivery", "—")}',
                f'投递退避           : {verdict.get("reason", "")}'
                + (f"（连续 {verdict.get('attempts')} 次未生效）" if verdict.get('attempts') else ''),
                f'下次允许投递       : {verdict.get("nextAllowedAt") or "—"}',
                '',
                f'当前任务 (active)  : {task_id or "无"}',
                f'生命周期状态       : {state or "无"}',
                f'任务结果时间       : ' + (format_instant(life_entry.get('resultReadyAt')) or '—'),
                f'排队任务 (pending) : {", ".join(pending) if pending else "无"}',
                f'挂起任务 (suspend) : {", ".join(suspended) if suspended else "无"}',
            ]
            text = '\n'.join(rows)
        self.detail.configure(state='normal')
        self.detail.delete('1.0', 'end')
        self.detail.insert('1.0', text)
        self.detail.configure(state='disabled')


if __name__ == '__main__':
    root = tk.Tk()
    if '--smoke-test' in sys.argv:
        root.withdraw()
        app = App(root)
        app.enabled.set(False)
        root.update_idletasks()
        assert app.tree.get_children(), 'Registry was not loaded'
        app.tree.selection_set(app.tree.get_children()[0])
        app.show_detail()
        assert 'role' in app.detail.get('1.0', 'end'), 'Detail panel did not render'
        root.destroy()
    else:
        App(root)
        root.mainloop()
