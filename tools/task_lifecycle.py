#!/usr/bin/env python3
"""Guard result delivery: result-ready, producer accept, or rework."""
import argparse,json
from datetime import datetime,timezone
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]; PATH=ROOT/'.ai/dispatch/task-lifecycle.local.json'
def stamp(): return datetime.now(timezone.utc).isoformat().replace('+00:00','Z')
def load(): return json.loads(PATH.read_text(encoding='utf-8')) if PATH.exists() else {}
def save(x): PATH.parent.mkdir(parents=True,exist_ok=True);tmp=PATH.with_suffix('.tmp');tmp.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');tmp.replace(PATH)
ap=argparse.ArgumentParser();sp=ap.add_subparsers(dest='cmd',required=True)
r=sp.add_parser('result-ready');r.add_argument('--task-id',required=True);r.add_argument('--artifact',action='append',required=True);r.add_argument('--evidence',action='append',required=True);r.add_argument('--validation',required=True);r.add_argument('--gaps',default='');r.add_argument('--released',default='')
a=sp.add_parser('accept');a.add_argument('--task-id',required=True);a.add_argument('--by',required=True)
w=sp.add_parser('rework');w.add_argument('--task-id',required=True);w.add_argument('--reason',required=True)
x=ap.parse_args();d=load()
if x.cmd=='result-ready': d[x.task_id]={'taskId':x.task_id,'state':'AwaitingProducerAcceptance','artifacts':x.artifact,'evidence':x.evidence,'validation':x.validation,'gaps':x.gaps,'released':x.released,'resultReadyAt':stamp(),'producerAcceptedAt':None,'reworkReason':None};save(d);print(json.dumps(d[x.task_id],ensure_ascii=False))
else:
 if x.task_id not in d: raise SystemExit('task has no result-ready record')
 d[x.task_id]['state']='Accepted' if x.cmd=='accept' else 'Rework';d[x.task_id]['producerAcceptedAt']=stamp() if x.cmd=='accept' else None;d[x.task_id]['acceptedBy']=getattr(x,'by',None);d[x.task_id]['reworkReason']=getattr(x,'reason',None);save(d);print(json.dumps(d[x.task_id],ensure_ascii=False))
