"""Produce a local review gallery from Unity captures; no image pixels are changed."""
import html,json,os
from pathlib import Path
from prepare_formal_ui_art import ROOT,CHANGE,DESIGN,BOARD,read

def main():
 pages=read(CHANGE/'art/page-plan.json');evidence=CHANGE/'evidence'
 docs={int(p.name[:2]):p for p in (DESIGN/'界面').glob('[0-9][0-9]-*.md') if not p.name.startswith('00')}
 sections=[]
 for family,doc in docs.items():
  cards=[]
  for p in pages:
   if p['family']!=family:continue
   filename=f'{family:02d}-{p["form"]}-{p["page"]}.png'
   caption=html.escape(p['page'][10:]+' · '+p['form'])
   ref=os.path.relpath(BOARD/p['source'],evidence).replace('\\','/')
   cards.append(f'<article data-family="{family:02d}"><a href="previews/{filename}"><img loading="lazy" src="previews/{filename}" alt="{caption}"></a><b>{caption}</b><p><a href="previews/{filename}">Unity 原尺寸渲染</a> · <a href="{html.escape(ref)}">对应效果图</a></p></article>')
  sections.append(f'<section data-family="{family:02d}"><h2>{html.escape(doc.stem)}</h2><div class="grid">'+''.join(cards)+'</div></section>')
 stylesheet='body{background:#111923;color:#e9eef2;font:16px system-ui;margin:24px}header{position:sticky;top:0;background:#111923ed;padding:12px;z-index:2}h1{font-size:24px}p{color:#aebcc7}a{color:#f0a24b}select{font:inherit;padding:8px;background:#2b3745;color:white}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:18px}article{background:#1b2633;padding:12px;border-radius:6px}img{width:100%;display:block;margin-bottom:12px}b{font-size:15px}h2{margin-top:38px}'
 options='<option value="all">全部 18 族</option>'+''.join(f'<option value="{i:02d}">{html.escape(p.stem)}</option>' for i,p in docs.items())
 page='<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>正式 UI 接入 · Unity 渲染复核</title><style>'+stylesheet+'</style><header><h1>正式 UI 接入 · Unity 渲染复核</h1><p>349 张源 PNG 保留原字节。以下为 105 个现有页面根的 1920×1080 独立 Prefab 渲染；不是业务流程或所有动态状态完成的证明。动态验证另见 runtime-ready.json 与运行记录。</p><select id="family">'+options+'</select></header>'+''.join(sections)+'<script>document.querySelector("#family").onchange=e=>document.querySelectorAll("section").forEach(s=>s.hidden=e.target.value!=="all"&&s.dataset.family!==e.target.value);</script></html>'
 if (evidence/'runtime/ui-navigation.mp4').exists():
  runtime='<div><h2>实际运行导航</h2><p>主菜单 → 存档槽位 → 返回 → 设置 → 返回。使用现有按钮回调验证，未创建、加载或删除存档。</p><video controls preload="metadata" style="width:100%;max-width:1280px" src="runtime/ui-navigation.mp4"></video><p><a href="implementation.md">接入报告及缺口</a> · <a href="runtime-navigation.json">导航检查结果</a></p></div>'
  page=page.replace('</header>','</header>'+runtime,1)
 (evidence/'index.html').write_text(page,encoding='utf-8')
 tests=read(evidence/'ui-tests.json') if (evidence/'ui-tests.json').exists() else []
 summary={'assets':349,'sliced':sum(a['sliced'] for a in read(CHANGE/'art/import-plan.json')),'pageRoots':len(pages),'renderings':len(list((evidence/'previews').glob('*.png'))),'families':len(docs),'testsPassed':sum(t.get('passedTests',0) for t in tests),'testFailures':sum(t.get('failedTests',0) for t in tests),'fixtures':len(tests)}
 (evidence/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
 coverage=read(CHANGE/'art/coverage.json')
 assets={a['key']:a for a in read(CHANGE/'art/import-plan.json')}
 lines=['# 缺失独立页与素材冲突','',
  '本轮只调整现有界面。以下两项没有另建独立页面根，使用已有界面或模式承载。','',
  '| 设计项 | 当前承载 | 本轮处理 |','|---|---|---|',
  '| 05 / RemoteMachine | 04 族 BaseCommandHubForm / Panel_PageRemoteMachine | 保留共享页，不新建 Form |',
  '| 13 / AlgorithmDiagnosis | 现有算法编辑器诊断模式 | 保留已有模式，不新建 Form |','',
  '效果图包对以下 19 项给出了不同素材路由。按用户已确认边界，本轮保持正式设计文档的 ID 和工程资源引用；不执行这些互换。原 PNG 字节均未修改。','',
  '| 正式文档 ID | 当前工程源文件 | 效果图包建议使用 ID |','|---|---|---|']
 for key,conflict in coverage['semanticConflicts'].items():
  current=Path(assets[key]['path']).name if key in assets else key
  lines.append(f'| `{key}` | `{current}` | `{conflict["sourceId"]}` |')
 lines+=['','详细源路径和指纹见 [覆盖矩阵](../art/coverage.json)。这些源文件语义差异仍可能影响滑条、分类图标与资源图标的外观。','']
 (evidence/'gaps-and-conflicts.md').write_text('\n'.join(lines),encoding='utf-8')
 print(json.dumps(summary,ensure_ascii=False))

if __name__=='__main__':main()
