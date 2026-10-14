"""Prepare source-preserving UI import/layout instructions from approved documents.

Reads PNG alpha only; never edits source pixels. Unity applies the resulting plan.
"""
from __future__ import annotations
import hashlib
import json
import re
import shutil
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parent.parent
CHANGE=ROOT/'openspec/changes/b54-formal-ui-art-integration'
DESIGN=ROOT/'Docs/GameDesign/11-美术规范/界面设计文档'
BOARD=DESIGN/'异星拓荒_UI效果图_20261014'
OUT=CHANGE/'art'

def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
def key(p):
 n=p.name.split('@')[0]
 return 'M'+n[10:] if n.startswith('ui-master-') else 'S'+n[4:10] if n.startswith('scr-') else 'L'+n[5:]
def walk(n):
 yield n
 for c in n.get('children',[]):yield from walk(c)

def panels(page,f,sections):
 """Functional panel rectangles, in the same 1920x1080 space as the boards."""
 n=len(sections);p=page
 if f==1:
  return {'MainMenu':[[286,396,452,70],[780,814,820,70]],'SaveSlots':[[276,386,580,390],[892,282,746,456]],'SaveDetail':[[276,400,580,340],[928,292,690,518]],'NewProgress':[[286,388,562,316],[908,338,690,518]],'Recovery':[[300,490,550,192],[900,450,690,280]]}.get(p,[])
 if f==2:
  if p=='DisplaySettings':return [[554,288,312,442],[908,288,604,454]]
  return [[554,280,944,514]] if n==1 else [[554,280,944,360],[554,664,944,120]]
 if f in (5,6,7) and p!='MachinePreparation':
  x,w=(1440,456) if p=='MachineOverview' else (1304,592)
  return [[x+32,540,w-64,152],[x+32,710,w-64,138],[x+32,864,w-64,86]][:n]
 if p=='MachinePreparation':return [[1472,292,392,182],[1472,492,392,182],[1472,690,392,100],[1472,804,392,74]][:n]
 if f==4:
  if p=='HubOverview':return [[418,236,270,210],[710,236,442,210],[1174,476,582,348]]
  if p=='HubStats':return [[418,240,392,176],[834,240,922,176],[418,438,1338,380]]
  if n==3:return [[418,240,394,566],[834,246,922,274],[834,542,922,264]]
  return [[418,240,680,572],[1130,240,626,572]][:n]
 if f==8:
  if p=='Workshop':return [[144,320,400,470],[568,244,610,596],[1202,244,494,596]]
  return [[144,320,400,470],[568,244,1128,596]][:n]
 if f==9:
  if n==3:return [[424,324,774,236],[424,580,774,248],[1224,246,512,592]]
  return [[424,324,774,506],[1224,246,512,592]][:n]
 if f==10:return [[184,320,970,514],[1184,320,552,514]][:n]
 if f==11:
  return [[184,324,672,478],[1064,324,672,478],[896,324,128,478]][:n]
 if f==12:return [[184,324,686,500],[896,324,840,500]][:n]
 if f==13:
  if p=='PublicParameters':return [[388,198,1000,546],[1412,198,404,674]][:n]
  return [[104,198,260,674],[388,198,1000,546],[1412,198,404,674],[388,768,1000,124],[478,144,910,44]][:n]
 if f==14:return [[1528,158,368,146],[1528,320,368,144],[446,926,410,126]][:n]
 if f==15:return [[432,326,570,476],[1030,244,726,586]][:n]
 if f==16:
  if n==1:return [[1228,248,528,584]]
  return [[418,248,782,584],[1228,248,528,584]][:n]
 if f==17:
  if p=='Rename':return [[488,478,1080,266]]
  if p=='StrongConfirm':return [[282,438,636,388],[1044,446,480,120],[1044,590,480,236]]
  return [[982,446,616,338]] if n==1 else [[282,650,654,178],[982,446,616,338]][:n]
 if f==18:
  if p in ('Loading','BackupProgress','SafeWait'):return [[352,490,1216,156],[416,712,1088,108]][:n]
  return [[164,664,850,164],[1042,240,714,588]][:n]
 return [[250+i*650,300,610,480] for i in range(n)]

def main():
 OUT.mkdir(parents=True,exist_ok=True)
 manifest=read(BOARD/'manifest.json')
 refs={}
 for entry in manifest:
  for ref in entry['references']:refs.setdefault(ref['logicalId'],[]).append(ref)
 assets=[];paths={}
 borders={1:(96,96,96,96),2:(56,8,56,8),4:(48,0,48,0),5:(32,12,32,12),6:(40,40,40,40),7:(16,0,16,0),8:(24,8,24,8),9:(24,8,24,8),10:(8,0,8,0),11:(32,24,32,24),12:(24,40,24,40)}
 for p in sorted((ROOT/'Assets/Game/Sprites/UI').rglob('*.png')):
  k=key(p);paths[k]=p.relative_to(ROOT).as_posix()
  with Image.open(p) as im:
   w,h=im.size;bb=im.convert('RGBA').getchannel('A').point(lambda alpha:255 if alpha>8 else 0).getbbox() or (0,0,w,h)
  nominal=tuple(map(int,re.search(r'@(\d+)x(\d+)',p.name).groups()))
  family=int(k[1:4]) if k.startswith('M') else 0
  sliced=any(r['mode']=='slice' for r in refs.get(k,[]))
  if family in (1,2,5,6,9,11):sliced=True
  if family in (3,13,14) or any(s in k for s in ('endpoint','checkbox','knob','handle','node-port','edge-corner','tile','dashed')):sliced=False
  if k.startswith('M012-node-') and 'port' not in k:sliced=True
  tile='tile' in k or 'dashed' in k
  crop=not tile and not k.startswith(('M001','M011-deco','L'))
  x,y,r,b=bb if crop else (0,0,w,h);cw,ch=r-x,b-y
  base=borders.get(family,(32,32,32,32)) if sliced else (0,0,0,0)
  if 'port-in' in k or 'port-out' in k:base=(24,8,24,8)
  if 'deco-frame' in k:base=(96,96,96,96)
  if 'scrollbar-track-v' in k:base=(0,12,0,12)
  if 'scrollbar-track-h' in k:base=(12,0,12,0)
  if 'divider-v' in k:base=(0,8,0,8)
  if 'edge-straight' in k:base=(16,0,16,0)
  scale=(ch/nominal[1]) if 'track-v' in k or 'divider-v' in k else cw/nominal[0]
  border=[round(v*scale,3) for v in base]
  border[0]=min(border[0],cw*.45);border[2]=min(border[2],cw*.45)
  border[1]=min(border[1],ch*.45);border[3]=min(border[3],ch*.45)
  assets.append(dict(key=k,path=paths[k],sha256=hashlib.sha256(p.read_bytes()).hexdigest(),sourceSize=[w,h],nominalSize=nominal,rect=[x,h-b,cw,ch],border=border,ppu=round(100*scale,4),sliced=sliced,tile=tile,cropped=crop and (cw,ch)!=(w,h),dimensionMismatch=(w,h)!=nominal))
 write(OUT/'import-plan.json',assets)
 pages=[];seen=set()
 for contract in sorted((ROOT/'Docs/Development/UI-PrefabLayouts').glob('*.contract.json')):
  doc=read(contract)
  for node in walk(doc['root']):
   if not node['name'].startswith('Panel_Page'):continue
   name=node['name'][10:]
   candidates=[b for b in manifest if b['page']==name and b.get('kind')!='main']
   if not candidates:continue
   entry=candidates[0]
   f=entry['family'];seen.add((f,name))
   frame=next((v['rect'] for v in entry['regions'] if v['name']=='Panel_Frame'),None)
   if f in (5,6,7):frame=[1440,124,456,832] if name in ('MachineOverview','MachinePreparation') else [1304,132,592,884]
   if f in (3,14):frame=[0,0,1920,1080]
   sections=[c['name'] for c in node.get('children',[]) if c['name'].startswith('Panel_') and not c['name'].startswith('Panel_Page')]
   artwork=[]
   for ref in entry['references']:
    k=ref['logicalId'];x,y,w,h=ref['rect']
    if ref['role'].startswith('world-') or ref['role']=='damage-overlay-same-transform':continue
    if k.startswith('L') or (k.startswith('S') and w>=180 and h>=120 and ref['mode']!='tile'):
     # Static illustrations only. Live badges, counts, paths and progress remain program-owned.
     if f==13 or f==14:continue
     artwork.append(dict(key=k,path=paths[k],rect=ref['rect'],mode=ref['mode'],opacity=ref['opacity']))
   if name=='MainMenu':artwork=[dict(key='L002-lockup',path=paths['L002-lockup'],rect=[286,248,450,150],mode='fit',opacity=1),dict(key='S01-001',path=paths['S01-001'],rect=[754,258,840,570],mode='fit',opacity=1)]
   # Technical content stays readable beneath illustration slots.
   rects=panels(name,f,sections)
   plan=dict(form=doc['form'],prefab=doc['prefabPath'],contract=contract.relative_to(ROOT).as_posix(),page=node['name'],family=f,state=entry['state'],frame=frame,sections=[dict(name=n,rect=r) for n,r in zip(sections,rects)],artwork=artwork,source=entry['file'])
   if name=='MainMenu':plan.update(entryMenu=True,legacyLogo='Img_MainMenuLogo',actionGroup='Grp_MainMenuActions')
   if f==3:plan['sections']=[]
   pages.append(plan)
 pages.sort(key=lambda p:(p['family'],p['form'],p['page']))
 write(OUT/'page-plan.json',pages)
 missing=sorted({(b['family'],b['page']) for b in manifest if b.get('kind')!='main'}-seen)
 write(OUT/'coverage.json',{'prefabs':len(list((ROOT/'Assets/Game/Prefabs/UI').rglob('*.prefab'))),'pages':len(pages),'missing':[list(v) for v in missing],'semanticConflicts':read(BOARD/'semantic-source-routing.json')})
 backup=ROOT/'.ai/backups/b54-before-art'
 for base in ('Assets/Game/Sprites/UI','Assets/Game/Prefabs/UI','Docs/Development/UI-PrefabLayouts'):
  for p in (ROOT/base).rglob('*'):
   if p.is_file() and p.suffix in ('.meta','.prefab','.json','.md'):
    dst=backup/p.relative_to(ROOT)
    if not dst.exists():dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dst)
 print(json.dumps({'assets':len(assets),'pages':len(pages),'missing':missing,'cropped':sum(a['cropped'] for a in assets),'sliced':sum(a['sliced'] for a in assets)},ensure_ascii=False))

if __name__=='__main__':main()
