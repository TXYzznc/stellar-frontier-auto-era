#!/usr/bin/env python3
import argparse,json,uuid
from datetime import datetime,timezone
from pathlib import Path
from window_state_logic import effective_state
ROOT=Path(__file__).resolve().parents[1]; Q=ROOT/'.ai/dispatch/task-queue.local.json'; R=ROOT/'.ai/dispatch/window-registry.local.json'; L=ROOT/'.ai/dispatch/task-lifecycle.local.json'; WS=ROOT/'.ai/dispatch/window-state.local.json'; O=ROOT/'.ai/dispatch/watchdog-decisions.local.jsonl'
def load(p):
 try:return json.loads(p.read_text(encoding='utf8'))
 except (FileNotFoundError,json.JSONDecodeError):return {}
def main():
 ap=argparse.ArgumentParser();ap.add_argument('--role',action='append');ap.add_argument('--window-state',action='append',default=[]);ap.add_argument('--queue-file',default=str(Q));ap.add_argument('--registry-file',default=str(R));ap.add_argument('--lifecycle-file',default=str(L));ap.add_argument('--append',action='store_true');a=ap.parse_args();q=load(Path(a.queue_file)).get('roles',{});r=load(Path(a.registry_file)).get('windows',{});life=load(Path(a.lifecycle_file));filews=load(WS);ws=dict(x.split('=',1) for x in a.window_state if '=' in x); out=[]
 for role in (a.role or q.keys()):
  s=q.get(role,{}); active=s.get('active'); pending=[x for x in s.get('pending',[]) if str(x.get('state','pending')).lower() in ('pending','queued','排队中')]; state=effective_state(filews.get(role),ws.get(role,r.get(role,{}).get('actualState','unknown'))); ls=next((v.get('state') for v in life.values() if active and v.get('taskId')==active.get('taskId')),None); runnable=bool(pending) or ls in ('Active','Rework','InProgress')
  if not runnable: action,reason='none','no_runnable_work'
  elif s.get('paused') or r.get(role,{}).get('paused') or state=='paused': action,reason='none','explicitly_paused'
  elif ls in ('AwaitingProducerAcceptance','WaitingCollaboration','Accepted'): action,reason='none','lifecycle_waiting_or_complete'
  elif state in ('running','inProgress','active'): action,reason='none','window_actually_running'
  else: action,reason='wake','runnable_task_but_window_not_running'
  d={'decisionId':str(uuid.uuid4()),'at':datetime.now(timezone.utc).isoformat(),'role':role,'action':action,'reason':reason,'threadId':r.get(role,{}).get('threadId'),'taskId':(pending[0].get('taskId') if pending else (active or {}).get('taskId')),'windowState':state,'lifecycleState':ls};out.append(d)
  if a.append and action=='wake': O.parent.mkdir(parents=True,exist_ok=True);O.open('a',encoding='utf8').write(json.dumps(d,ensure_ascii=False)+'\n')
 print(json.dumps(out,ensure_ascii=False,indent=2))
if __name__=='__main__':main()








