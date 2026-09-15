#!/usr/bin/env python3
"""AutoEra Image Pre-Processor (local rewrite proxy for Codex / OpenAI-Responses traffic).

Why this exists
---------------
Codex attaches clipboard images as raw ``data:image/png;base64,...`` payloads and
never scales them down.  A single 2.4 MB screenshot therefore becomes ~3.2 MB of
base64 text, and when the history/compaction re-sends earlier images the request
body can reach tens of MB.  Measured upstream failure for such bodies is
``HTTP 429 Rate limit exceeded ... Limit type: tokens`` -- i.e. the images eat the
token budget of the account, which looks to the user like an "image too large"
server problem.

This proxy sits between Codex and the existing local cc-switch proxy, decodes every
``input_image`` payload, downscales + re-encodes it locally (JPEG), and forwards the
smaller body.  Everything else (headers, auth, streaming SSE, other endpoints) is
passed through byte for byte.

Modes
-----
* ``serve``          : run the HTTP rewrite proxy (default).
* ``compress PATH``  : one-shot local pre-processing of a file, no server involved.
* ``selftest``       : spin up a fake upstream + the proxy and assert the rewrite works.

Only dependencies: Pillow + requests (both already present in the project venv).
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import io
import json
import os
import re
import sys
import threading
import time
from collections import OrderedDict
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

import requests
from PIL import Image

DEFAULT_UPSTREAM = "http://127.0.0.1:15721"
DEFAULT_LISTEN_HOST = "127.0.0.1"
DEFAULT_LISTEN_PORT = 15722

# Tuning defaults.  Overridable from config.json / command line / env.
DEFAULTS = {
    "listen_host": DEFAULT_LISTEN_HOST,
    "listen_port": DEFAULT_LISTEN_PORT,
    "upstream": DEFAULT_UPSTREAM,
    "max_edge": 1280,          # longest pixel edge kept (no upscaling)
    "quality": 85,             # starting JPEG quality
    "min_quality": 62,
    "max_bytes": 350_000,      # per-image byte budget after re-encode
    "min_bytes": 150_000,      # never touch images already this small
    "min_edge": 640,           # never shrink below this longest edge
    "cache_entries": 64,
    "log_file": "",

    # config guard: cc-switch rewrites ~/.codex/config.toml whenever the user
    # switches provider, pointing Codex straight back at its own port and
    # bypassing this proxy.  When guard_codex_config is set to that file's path,
    # a background thread keeps the *local* base_url aimed at this proxy.
    "guard_codex_config": "",
    "guard_target_base_url": "",
    "guard_interval": 5,
}

DATA_URL_RE = re.compile(r"^data:(image/[A-Za-z0-9.+-]+);base64,(.*)$", re.DOTALL)

_have_native_jpeg = False
try:  # optional, only used to preserve quality when available
    import pillow_jpeg_plugin  # type: ignore  # noqa: F401

    _have_native_jpeg = True
except Exception:  # pragma: no cover - optional
    _have_native_jpeg = False


# --------------------------------------------------------------------------- #
# logging
# --------------------------------------------------------------------------- #
_log_lock = threading.Lock()
_log_path: Path | None = None


def log(msg: str) -> None:
    line = f"{time.strftime('%Y-%m-%d %H:%M:%S')} {msg}"
    try:
        print(line, flush=True)
    except Exception:
        pass
    if _log_path is not None:
        try:
            with _log_lock:
                _log_path.parent.mkdir(parents=True, exist_ok=True)
                with _log_path.open("a", encoding="utf-8") as fh:
                    fh.write(line + "\n")
        except Exception:
            pass


# --------------------------------------------------------------------------- #
# image shrinking
# --------------------------------------------------------------------------- #
def _flatten_to_rgb(img: Image.Image) -> Image.Image:
    """Return an RGB image, compositing transparency over white."""
    if img.mode in ("RGBA", "LA") or (img.mode == "P" and "transparency" in img.info):
        rgba = img.convert("RGBA")
        bg = Image.new("RGB", rgba.size, (255, 255, 255))
        bg.paste(rgba, mask=rgba.split()[-1])
        return bg
    if img.mode not in ("RGB", "L"):
        return img.convert("RGB")
    return img.convert("RGB") if img.mode != "RGB" else img


def _encode_jpeg(img: Image.Image, quality: int) -> bytes:
    buf = io.BytesIO()
    img.save(buf, format="JPEG", quality=quality, optimize=True, progressive=True)
    return buf.getvalue()


def shrink_image_bytes(
    data: bytes,
    *,
    max_edge: int,
    quality: int,
    min_quality: int,
    max_bytes: int,
    min_bytes: int,
    min_edge: int,
) -> tuple[bytes, dict] | None:
    """Downscale/re-encode ``data``.  Returns ``(new_bytes, meta)`` or None when
    the image should be left untouched."""
    try:
        img = Image.open(io.BytesIO(data))
        img.load()
    except Exception as exc:  # not an image we can decode -> leave alone
        return None

    src_bytes = len(data)
    src_w, src_h = img.size
    longest = max(src_w, src_h)

    if longest <= max_edge and src_bytes <= min_bytes:
        return None  # already small on both axes -> don't degrade it
    if longest <= 1 or src_bytes == 0:
        return None

    rgb = _flatten_to_rgb(img)

    edge = min(longest, max_edge)
    q = quality
    best: bytes | None = None
    chosen = (edge, q)

    for _ in range(8):
        scale = edge / longest
        if scale < 1.0:
            size = (max(1, round(src_w * scale)), max(1, round(src_h * scale)))
            work = rgb.resize(size, Image.LANCZOS)
        else:
            work = rgb
        encoded = _encode_jpeg(work, q)
        if best is None or len(encoded) < len(best):
            best = encoded
            chosen = (edge, q)
        if len(encoded) <= max_bytes:
            best = encoded
            chosen = (edge, q)
            break
        if q > min_quality:
            q = max(min_quality, q - 8)
            continue
        if edge <= min_edge:
            break
        edge = max(min_edge, int(edge * 0.85))
        q = quality

    if best is None:
        return None
    if len(best) >= src_bytes:
        return None  # re-encode did not help -> keep the original bytes

    meta = {
        "src_bytes": src_bytes,
        "src_size": [src_w, src_h],
        "out_bytes": len(best),
        "out_size": [round(src_w * chosen[0] / longest), round(src_h * chosen[0] / longest)],
        "edge": chosen[0],
        "quality": chosen[1],
        "mime": "image/jpeg",
    }
    return best, meta


class _RewriteCache:
    """LRU cache keyed by (sha1 of payload, params) -> result tuple."""

    def __init__(self, max_entries: int) -> None:
        self._max = max(1, max_entries)
        self._data: OrderedDict[str, tuple[bytes, dict] | None] = OrderedDict()
        self._lock = threading.Lock()

    def get(self, key: str):
        with self._lock:
            if key in self._data:
                self._data.move_to_end(key)
                return self._data[key]
        return "MISS"

    def put(self, key: str, value) -> None:
        with self._lock:
            self._data[key] = value
            self._data.move_to_end(key)
            while len(self._data) > self._max:
                self._data.popitem(last=False)


# --------------------------------------------------------------------------- #
# request body rewriting
# --------------------------------------------------------------------------- #
class BodyRewriter:
    def __init__(self, cfg: dict) -> None:
        self.cfg = cfg
        self.cache = _RewriteCache(int(cfg.get("cache_entries", DEFAULTS["cache_entries"])))

    def _shrink(self, payload: bytes) -> tuple[bytes, dict] | None:
        cfg = self.cfg
        key = hashlib.sha1(
            payload
            + b"|%d|%d|%d|%d" % (cfg["max_edge"], cfg["quality"], cfg["max_bytes"], cfg["min_bytes"])
        ).hexdigest()
        cached = self.cache.get(key)
        if cached != "MISS":
            return cached
        try:
            res = shrink_image_bytes(
                payload,
                max_edge=int(cfg["max_edge"]),
                quality=int(cfg["quality"]),
                min_quality=int(cfg["min_quality"]),
                max_bytes=int(cfg["max_bytes"]),
                min_bytes=int(cfg["min_bytes"]),
                min_edge=int(cfg["min_edge"]),
            )
        except Exception as exc:
            log(f"[warn] shrink failed: {exc!r}")
            res = None
        self.cache.put(key, res)
        return res

    def _handle_node(self, node: dict, stats: dict) -> bool:
        """Rewrite one ``input_image`` object in place.  Returns True if changed."""
        url = node.get("image_url")
        holder: tuple[dict, str] | None = None
        if isinstance(url, str):
            holder = (node, "image_url")
        elif isinstance(url, dict) and isinstance(url.get("url"), str):
            holder = (url, "url")
        else:
            return False

        target: str = holder[0][holder[1]]
        m = DATA_URL_RE.match(target.strip())
        if not m:
            return False  # remote URL / already rewritten / unknown scheme

        try:
            raw = base64.b64decode(m.group(2), validate=False)
        except Exception:
            return False

        stats["images"] += 1
        stats["bytes_in"] += len(raw)
        res = self._shrink(raw)
        if res is None:
            stats["bytes_out"] += len(raw)
            stats["skipped"] += 1
            return False

        payload, meta = res
        holder[0][holder[1]] = "data:image/jpeg;base64," + base64.b64encode(payload).decode("ascii")
        stats["rewritten"] += 1
        stats["bytes_out"] += len(payload)
        stats["details"].append(meta)
        return True

    def rewrite(self, node, stats: dict) -> bool:
        changed = False
        if isinstance(node, dict):
            if node.get("type") == "input_image" or (
                "image_url" in node and isinstance(node.get("image_url"), (str, dict))
            ):
                if self._handle_node(node, stats):
                    changed = True
            for value in list(node.values()):
                if self.rewrite(value, stats):
                    changed = True
        elif isinstance(node, list):
            for value in node:
                if self.rewrite(value, stats):
                    changed = True
        return changed

    def rewrite_body(self, raw: bytes) -> tuple[bytes, dict | None]:
        if not raw:
            return raw, None
        try:
            doc = json.loads(raw)
        except Exception:
            return raw, None
        if not isinstance(doc, (dict, list)):
            return raw, None
        stats = {
            "images": 0,
            "rewritten": 0,
            "skipped": 0,
            "bytes_in": 0,
            "bytes_out": 0,
            "details": [],
        }
        try:
            changed = self.rewrite(doc, stats)
        except Exception as exc:
            log(f"[warn] rewrite pass failed: {exc!r}")
            return raw, None
        if not changed:
            return raw, None
        out = json.dumps(doc, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
        stats["body_in"] = len(raw)
        stats["body_out"] = len(out)
        return out, stats


# --------------------------------------------------------------------------- #
# HTTP proxy
# --------------------------------------------------------------------------- #
HOP_BY_HOP = {
    "host",
    "content-length",
    "connection",
    "keep-alive",
    "transfer-encoding",
    "upgrade",
    "te",
    "trailer",
    "proxy-connection",
    "proxy-authorization",
}


class ProxyServer(ThreadingHTTPServer):
    daemon_threads = True

    def __init__(self, addr, handler, cfg: dict) -> None:
        super().__init__(addr, handler)
        self.cfg = cfg
        self.rewriter = BodyRewriter(cfg)
        self.upstream = cfg["upstream"].rstrip("/")
        self._local = threading.local()
        self.totals = {"requests": 0, "rewritten_requests": 0, "images": 0, "saved": 0}

    @property
    def session(self) -> requests.Session:
        """One keep-alive pool per worker thread (Session is not thread-safe)."""
        s = getattr(self._local, "session", None)
        if s is None:
            s = requests.Session()
            self._local.session = s
        return s


class Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"
    server_version = "AutoEraImageProxy/1.0"
    sys_version = ""

    server: ProxyServer  # type: ignore[assignment]

    def log_message(self, fmt: str, *args) -> None:  # keep the console readable
        return

    # -- helpers ---------------------------------------------------------- #
    def _read_body(self) -> bytes:
        te = (self.headers.get("Transfer-Encoding") or "").lower()
        if "chunked" in te:
            return self._read_chunked()
        length = self.headers.get("Content-Length")
        if not length:
            return b""
        try:
            n = int(length)
        except ValueError:
            return b""
        if n <= 0:
            return b""
        return self.rfile.read(n)

    def _read_chunked(self) -> bytes:
        """Minimal HTTP/1.1 chunked reader (requests can send TE: chunked)."""
        chunks: list[bytes] = []
        while True:
            line = self.rfile.readline(65536).strip()
            if not line:
                break
            try:
                size = int(line.split(b";", 1)[0], 16)
            except ValueError:
                return b"".join(chunks)
            if size == 0:
                while True:  # consume trailers
                    trailer = self.rfile.readline(65536)
                    if trailer in (b"\r\n", b"\n", b""):
                        break
                break
            chunks.append(self.rfile.read(size))
            self.rfile.read(2)  # trailing CRLF
        return b"".join(chunks)

    def _forward_headers(self) -> dict:
        out = {}
        for key, value in self.headers.items():
            if key.lower() in HOP_BY_HOP:
                continue
            if key.lower() == "accept-encoding":
                continue  # force identity: keeps SSE latency low and bytes intact
            out[key] = value
        out["Accept-Encoding"] = "identity"
        return out

    def _relay(self, resp: requests.Response, extra_headers: dict | None = None) -> None:
        self.send_response(resp.status_code)
        length = resp.headers.get("Content-Length")
        for key, value in resp.headers.items():
            lk = key.lower()
            if lk in ("transfer-encoding", "connection", "keep-alive", "content-length"):
                continue
            self.send_header(key, value)
        if extra_headers:
            for key, value in extra_headers.items():
                self.send_header(key, value)
        if length is not None:
            self.send_header("Content-Length", length)
        else:
            self.send_header("Transfer-Encoding", "chunked")
        self.end_headers()

        if self.command == "HEAD" or resp.status_code in (204, 304):
            resp.close()
            return

        try:
            if length is not None:
                for chunk in resp.raw.stream(64 * 1024, decode_content=False):
                    if chunk:
                        self.wfile.write(chunk)
                        self.wfile.flush()
            else:
                for chunk in resp.raw.stream(16 * 1024, decode_content=False):
                    if not chunk:
                        continue
                    self.wfile.write(b"%x\r\n" % len(chunk))
                    self.wfile.write(chunk)
                    self.wfile.write(b"\r\n")
                    self.wfile.flush()
                self.wfile.write(b"0\r\n\r\n")
                self.wfile.flush()
        except (BrokenPipeError, ConnectionResetError):
            log("[info] client disconnected mid-response")
        finally:
            resp.close()

    # -- request entry points --------------------------------------------- #
    def _handle(self) -> None:
        started = time.perf_counter()
        raw = self._read_body()
        body = raw
        stats = None
        if raw and self.command in ("POST", "PUT", "PATCH"):
            body, stats = self.server.rewriter.rewrite_body(raw)

        url = self.server.upstream + self.path
        headers = self._forward_headers()
        if body is not None:
            headers["Content-Length"] = str(len(body))

        extra = None
        if stats:
            self.server.totals["rewritten_requests"] += 1
            self.server.totals["images"] += stats["rewritten"]
            self.server.totals["saved"] += max(0, stats["body_in"] - stats["body_out"])
            extra = {
                "X-AutoEra-Images-Rewritten": str(stats["rewritten"]),
                "X-AutoEra-Bytes-In": str(stats["body_in"]),
                "X-AutoEra-Bytes-Out": str(stats["body_out"]),
            }

        try:
            resp = self.server.session.request(
                self.command,
                url,
                data=body if body else None,
                headers=headers,
                stream=True,
                timeout=(30, 900),
                allow_redirects=False,
            )
        except requests.RequestException as exc:
            log(f"[error] upstream {self.command} {self.path} failed: {exc!r}")
            payload = json.dumps(
                {"error": {"message": f"AutoEraImageProxy: upstream unreachable: {exc}", "type": "proxy_error"}}
            ).encode("utf-8")
            self.send_response(502)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(payload)))
            self.end_headers()
            self.wfile.write(payload)
            return

        self.server.totals["requests"] += 1
        ms = (time.perf_counter() - started) * 1000
        if stats:
            detail = ", ".join(
                f"{d['src_size'][0]}x{d['src_size'][1]} {d['src_bytes']//1024}KB -> "
                f"{d['out_size'][0]}x{d['out_size'][1]} q{d['quality']} {d['out_bytes']//1024}KB"
                for d in stats["details"][:6]
            )
            log(
                f"[rewrite] {self.command} {self.path} imgs={stats['rewritten']}/{stats['images']} "
                f"body {stats['body_in']//1024}KB -> {stats['body_out']//1024}KB "
                f"({ms:.0f}ms) {detail}"
            )
        else:
            log(f"[pass]    {self.command} {self.path} {self.headers.get('Content-Length') or 0}B -> {resp.status_code} ({ms:.0f}ms)")

        self._relay(resp, extra)

    do_GET = _handle
    do_POST = _handle
    do_PUT = _handle
    do_PATCH = _handle
    do_DELETE = _handle
    do_HEAD = _handle
    do_OPTIONS = _handle


# --------------------------------------------------------------------------- #
# CLI
# --------------------------------------------------------------------------- #
def load_config(script_dir: Path) -> dict:
    cfg = dict(DEFAULTS)
    path = script_dir / "config.json"
    if path.is_file():
        try:
            cfg.update(json.loads(path.read_text(encoding="utf-8")))
        except Exception as exc:
            log(f"[warn] config.json invalid: {exc!r}")
    return cfg


BASE_URL_LINE_RE = re.compile(r'^(\s*base_url\s*=\s*)"(http://127\.0\.0\.1:\d+/v1)"(\s*)$', re.MULTILINE)


def ensure_codex_base_url(path: Path, target: str, *, dry_run: bool = False) -> bool:
    """Point the custom provider's local base_url at ``target``.  Returns True if changed.

    Deliberately narrow: only a quoted ``base_url`` whose value is already a
    loopback URL is touched, and only inside ``[model_providers.custom]``.
    """
    try:
        text = path.read_text(encoding="utf-8")
    except OSError:
        return False
    if target in text:
        return False

    # locate [model_providers.custom] and the next section header after it
    header = re.search(r'^\[model_providers\.custom\]\s*$', text, re.MULTILINE)
    if not header:
        return False
    rest = text[header.end():]
    nxt = re.search(r'^\[', rest, re.MULTILINE)
    block_end = header.end() + (nxt.start() if nxt else len(rest))
    block = text[header.end():block_end]

    m = BASE_URL_LINE_RE.search(block)
    if not m:
        return False
    if m.group(2) == target:
        return False

    new_block = block[: m.start()] + m.group(1) + '"' + target + '"' + m.group(3) + block[m.end():]
    new_text = text[: header.end()] + new_block + text[block_end:]

    if dry_run:
        return True
    backup = path.with_suffix(path.suffix + ".bak-imgproxy")
    try:
        if not backup.exists():
            backup.write_text(text, encoding="utf-8")
    except OSError:
        pass
    tmp = path.with_suffix(path.suffix + ".tmp-imgproxy")
    tmp.write_text(new_text, encoding="utf-8")
    os.replace(tmp, path)
    return True


def guard_loop(cfg: dict) -> None:
    path = Path(str(cfg["guard_codex_config"])).expanduser()
    target = str(cfg["guard_target_base_url"]) or f"http://{cfg['listen_host']}:{cfg['listen_port']}/v1"
    interval = max(1, int(cfg.get("guard_interval", 5)))
    log(f"[guard] watching {path} -> keep base_url at {target}")
    while True:
        try:
            if ensure_codex_base_url(path, target):
                log(f"[guard] repaired {path}: base_url -> {target}")
        except Exception as exc:  # never let the guard kill the proxy
            log(f"[guard] warn: {exc!r}")
        time.sleep(interval)


def serve(cfg: dict) -> int:
    global _log_path
    if cfg.get("log_file"):
        _log_path = Path(cfg["log_file"]).expanduser()
        if not _log_path.is_absolute():
            _log_path = Path(__file__).resolve().parent / _log_path
    host = cfg["listen_host"]
    port = int(cfg["listen_port"])
    log(
        f"AutoEra image proxy listening on http://{host}:{port} -> {cfg['upstream']} "
        f"(max_edge={cfg['max_edge']} q={cfg['quality']} budget={cfg['max_bytes']}B)"
    )
    try:
        httpd = ProxyServer((host, port), Handler, cfg)
    except OSError as exc:
        log(f"[error] cannot bind {host}:{port}: {exc!r}")
        return 2
    if cfg.get("guard_codex_config"):
        threading.Thread(target=guard_loop, args=(cfg,), daemon=True).start()
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        log("interrupted, shutting down")
    finally:
        httpd.server_close()
    return 0


def compress_file(cfg: dict, src: Path, dst: Path | None) -> int:
    data = src.read_bytes()
    res = shrink_image_bytes(
        data,
        max_edge=int(cfg["max_edge"]),
        quality=int(cfg["quality"]),
        min_quality=int(cfg["min_quality"]),
        max_bytes=int(cfg["max_bytes"]),
        min_bytes=0,               # explicit request: always re-encode
        min_edge=int(cfg["min_edge"]),
    )
    if res is None:
        log(f"[skip] {src} -> not compressible")
        return 1
    payload, meta = res
    out = dst or src.with_name(f"{src.stem}_opt.jpg")
    out.write_bytes(payload)
    log(
        f"[done] {src} {meta['src_size'][0]}x{meta['src_size'][1]} {meta['src_bytes']//1024}KB"
        f" -> {out} {meta['out_size'][0]}x{meta['out_size'][1]} q{meta['quality']} {meta['out_bytes']//1024}KB"
    )
    return 0


def _make_screenshot_like(w: int, h: int) -> "Image.Image":
    """Deterministic synthetic "screenshot": flat panels, rules and text-like noise."""
    from PIL import ImageDraw

    img = Image.new("RGB", (w, h), (246, 246, 248))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, w - 1, 120], fill=(32, 38, 52))
    d.rectangle([40, 180, w - 40, 900], fill=(255, 255, 255), outline=(200, 204, 212))
    d.rectangle([40, 960, w - 40, h - 60], fill=(255, 255, 255), outline=(200, 204, 212))
    for i in range(60):
        y = 210 + i * 11
        d.line([(70, y), (70 + 300 + (i * 37) % 900, y)], fill=(90, 96, 110), width=2)
    for i in range(200):
        y = 200 + i * 13
        d.line([(60, y), (60 + (i * 53) % 1000, y)], fill=(120, 126, 140), width=1)
    for i in range(26):
        y = 990 + i * 34
        d.line([(80, y), (80 + 250 + (i * 71) % 1600, y)], fill=(70, 76, 90), width=3)
    # subtle per-pixel noise: real screenshots (subpixel AA / gradients / photos)
    # defeat PNG's flat-run compression and land in the same size class as the
    # clipboard PNGs we measured (1-2.5 MB for ~1400x1100).
    px = img.load()
    seed = 12345
    for y in range(h):
        for x in range(w):
            seed = (seed * 1103515245 + 12345) & 0x7FFFFFFF
            n = ((seed >> 16) & 7) - 3
            r, g, b = px[x, y]
            px[x, y] = (max(0, min(255, r + n)), max(0, min(255, g + n)), max(0, min(255, b + n)))
    return img


def selftest(cfg: dict) -> int:
    """Fake upstream + real proxy: assert the rewrite happens and the body shrinks."""
    from http.server import BaseHTTPRequestHandler as B, HTTPServer

    seen: dict = {}

    class Upstream(B):
        def log_message(self, *a):
            pass

        def do_POST(self):
            n = int(self.headers.get("Content-Length") or 0)
            body = self.rfile.read(n)
            seen["body"] = body
            out = json.dumps({"id": "resp_fake", "bytes": len(body)}).encode()
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(out)))
            self.end_headers()
            self.wfile.write(out)

    up = HTTPServer(("127.0.0.1", 0), Upstream)
    threading.Thread(target=up.serve_forever, daemon=True).start()
    cfg = dict(cfg)
    cfg["upstream"] = f"http://127.0.0.1:{up.server_port}"

    prox = ProxyServer((cfg["listen_host"], 0), Handler, cfg)
    threading.Thread(target=prox.serve_forever, daemon=True).start()

    # build a big PNG in memory
    img = _make_screenshot_like(2600, 1900)
    buf = io.BytesIO()
    img.save(buf, "PNG")
    png = buf.getvalue()
    data_url = "data:image/png;base64," + base64.b64encode(png).decode()

    body = json.dumps(
        {
            "model": "m",
            "input": [
                {"type": "message", "role": "user", "content": [{"type": "input_text", "text": "hi"}]},
                {"type": "message", "role": "user", "content": [{"type": "input_image", "image_url": data_url, "detail": "high"}]},
            ],
        }
    ).encode()

    url = f"http://{cfg['listen_host']}:{prox.server_port}/v1/responses"
    r = requests.post(url, data=body, headers={"Content-Type": "application/json"}, timeout=120)
    sent = seen.get("body", b"")
    sent_doc = json.loads(sent)
    new_url = sent_doc["input"][1]["content"][0]["image_url"]
    new_raw = base64.b64decode(new_url.split(",", 1)[1])

    checks = []
    checks.append(("upstream got 200", r.status_code == 200))
    checks.append(("body shrank", len(sent) < len(body)))
    checks.append(("image became jpeg", new_url.startswith("data:image/jpeg;base64,")))
    checks.append(("image much smaller", len(new_raw) < len(png) // 4))
    checks.append(("text preserved", sent_doc["input"][0]["content"][0]["text"] == "hi"))
    checks.append(("detail preserved", sent_doc["input"][1]["content"][0]["detail"] == "high"))
    checks.append(("model preserved", sent_doc["model"] == "m"))

    ok = True
    for name, good in checks:
        if not good:
            ok = False
        log(f"  [{'ok' if good else 'FAIL'}] {name}")
    log(f"  origin png={len(png)//1024}KB -> jpeg={len(new_raw)//1024}KB, body {len(body)//1024}KB -> {len(sent)//1024}KB")
    prox.shutdown()
    up.shutdown()
    log("[selftest] PASS" if ok else "[selftest] FAIL")
    return 0 if ok else 1


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="AutoEra image pre-processor for Codex/Responses traffic")
    parser.add_argument("--config", type=Path, default=None, help="path to config.json")
    parser.add_argument("--upstream", default=None)
    parser.add_argument("--listen-host", default=None)
    parser.add_argument("--listen-port", type=int, default=None)
    parser.add_argument("--max-edge", type=int, default=None)
    parser.add_argument("--quality", type=int, default=None)
    parser.add_argument("--max-bytes", type=int, default=None)
    parser.add_argument("--min-bytes", type=int, default=None)
    parser.add_argument("--log-file", default=None)
    sub = parser.add_subparsers(dest="mode")
    p_cmp = sub.add_parser("compress", help="compress one image file locally")
    p_cmp.add_argument("input", type=Path)
    p_cmp.add_argument("output", type=Path, nargs="?")
    sub.add_parser("selftest", help="run an end-to-end rewrite check")

    args = parser.parse_args(argv)
    script_dir = Path(__file__).resolve().parent
    cfg = load_config(Path(args.config).resolve().parent if args.config else script_dir)
    if args.config:
        cfg = {**cfg, **json.loads(Path(args.config).read_text(encoding="utf-8"))}

    for key in ("upstream", "listen_host", "listen_port", "max_edge", "quality", "max_bytes", "min_bytes", "log_file"):
        val = getattr(args, key)
        if val is not None:
            cfg[key] = val

    if args.mode == "compress":
        return compress_file(cfg, args.input, args.output)
    if args.mode == "selftest":
        return selftest(cfg)
    return serve(cfg)


if __name__ == "__main__":
    sys.exit(main())
