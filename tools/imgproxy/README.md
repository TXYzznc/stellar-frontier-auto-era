# AutoEra Image Proxy（本地图片预处理）

Codex/Responses 请求里，剪贴板图片是**原尺寸 PNG** 直接 base64 上传的：一张 2.4 MB
截图编码后约 3.2 MB，历史与 compaction 反复携带后会撞到上游 token 限额
（实测报 `HTTP 429 ... Limit type: tokens, Current limit: 5000000`，表现很像"图片太大"）。

这个代理挂在 Codex 和已有 cc-switch 代理之间：本地把图片降采样 + 转 JPEG，再转发
缩小后的请求体。**上传前和查看前都会经过这里**，原图文件本身不会被修改。

## 数据流

```
Codex  ->  127.0.0.1:15722 (img_proxy.py)  ->  127.0.0.1:15721 (cc-switch)  ->  上游
                 ^ 只改写 input_image；其余请求/响应/鉴权头/SSE 原样透传
```

## 日常使用

一般无需操作：已在开机启动目录放好快捷方式，且守护线程会每 3 秒检查
`~/.codex/config.toml` 的 `[model_providers.custom] base_url`，被 cc-switch 改回
15721 时自动修回 15722（首次修改前会备份为 `config.toml.bak-imgproxy`）。

- 前台启动（看日志）：`run.cmd`
- 静默启动：`start_background.cmd`
- 手动预处理单张图片（兜底，不改原文件）：
  ```
  .venv\Scripts\python.exe tools\imgproxy\img_proxy.py compress 输入.png 输出.jpg
  ```
- 自检：`... img_proxy.py selftest`
- 日志：`tools\imgproxy\logs\img_proxy.log`

## 可调参数（`config.json`，改完重启代理生效）

| 键 | 默认 | 说明 |
|---|---|---|
| `max_edge` | 1280 | 最长边上限，不放大 |
| `quality` | 85 | 起始 JPEG 质量 |
| `min_quality` | 62 | 质量下限 |
| `max_bytes` | 350000 | 单图目标字节上限，达不到才继续降质量/尺寸 |
| `min_bytes` | 150000 | 小于此值且尺寸不超限的图不动它 |
| `min_edge` | 640 | 尺寸下限 |
| `cache_entries` | 64 | 改写结果 LRU 缓存条目数 |

## 在 AutoEraWatchdog 里查看/拉起

`tools\desktop\AutoEraWatchdog.exe` 顶部新增「图片预处理代理」一行：

- **状态灯**：绿=运行中且 Codex 已指向本代理；黄=端口在跑但 Codex 的
  `base_url` 没指过来（guard 失守）；红=端口未监听。
- **状态文字**：`运行中/未运行 · host:port`，右侧显示上次检测时间（每 10 秒自动刷新）。
- **拉起代理**：调用 `start_background.cmd`（自带端口占用跳过 + 等待 15721 就绪），
  脚本本身幂等，重复点击不会起出第二个进程。
- **检查代理状态**：立即刷新一次灯与文字，不必等轮询。

端口与 host 从本文件同目录的 `config.json` 读取，改端口后无需重新打包 Watchdog。
## 临时停用

把 `~/.codex/config.toml` 的 `base_url` 改回 `http://127.0.0.1:15721/v1`，
并停掉 `img_proxy.py` 进程即可（守护线程会把它改回 15722，所以两步都要做）。

## 已知边界

- 只处理 `data:image/...;base64,` 的内联图片；远程 URL 图片不下载、不改写。
- 透明图会合成到白底（JPEG 无 alpha 通道）。
- 已在目标尺寸内且小于 `min_bytes` 的图不改写，避免无谓有损。
- 依赖仅 Pillow + requests，均来自项目 `.venv`。
