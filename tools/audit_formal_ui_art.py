"""Read-only source preservation, mapping, and UI serialization audit."""
from pathlib import Path
import hashlib,json,re,sys
from prepare_formal_ui_art import ROOT,CHANGE,read,walk

def main():
 errors=[];assets=read(CHANGE/'art/import-plan.json');pages=read(CHANGE/'art/page-plan.json')
 for a in assets:
  p=ROOT/a['path']
  if hashlib.sha256(p.read_bytes()).hexdigest()!=a['sha256']:errors.append('源 PNG 改变: '+a['path'])
  meta=p.with_suffix(p.suffix+'.meta').read_text(encoding='utf-8-sig')
  if not re.search(r'^  textureType: 8$',meta,re.M):errors.append('非 Sprite: '+a['path'])
  if not re.search(r'^    enableMipMap: 0$',meta,re.M):errors.append('mipmap 未关闭: '+a['path'])
  backup=ROOT/'.ai/backups/b54-before-art'/p.relative_to(ROOT)
  old_meta=backup.with_suffix(backup.suffix+'.meta')
  if old_meta.exists():
   guid=re.search(r'^guid: (\w+)$',meta,re.M).group(1)
   old_guid=re.search(r'^guid: (\w+)$',old_meta.read_text(encoding='utf-8-sig'),re.M).group(1)
   if guid!=old_guid:errors.append('GUID 改变: '+a['path'])
 pngs=list((CHANGE/'evidence/previews').glob('*.png'))
 for p in pages:
  name=f'{p["family"]:02d}-{p["form"]}-{p["page"]}.png'
  if not (CHANGE/'evidence/previews'/name).exists():errors.append('缺少渲染: '+name)
  d=read(ROOT/p['contract']); nodes={n['name']:n for n in walk(d['root'])}
  if p['page'] not in nodes:errors.append('合同缺页: '+p['page'])
  if p['family']!=3 and not any(n.startswith('Grp_FormalArtwork') for n in nodes):errors.append('未记录正式图层: '+p['form'])
 report={'assets':len(assets),'pages':len(pages),'previews':len(pngs),'sourcePngsUnchanged':not any('PNG 改变' in e for e in errors),'errors':errors}
 (CHANGE/'evidence/source-and-coverage-audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
 print(json.dumps(report,ensure_ascii=False));return bool(errors)

if __name__=='__main__':sys.exit(main())
