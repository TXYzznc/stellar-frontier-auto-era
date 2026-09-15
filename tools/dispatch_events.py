#!/usr/bin/env python3
"""Reliable coordination event bus and scheduler decision helper."""
from __future__ import annotations
import argparse,json,uuid
from datetime import datetime,timezone,timedelta
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]; EVENTS=ROOT/'.ai/dispatch/events.local.jsonl'; HEART=ROOT/'.ai/dispatch/heartbeats.local.json'; LEASES=ROOT/'.ai/dispatch/leases.local.json'; QUEUE=ROOT/'.ai/dispatch/task-queue.local.json'; REGISTRY=ROOT/'.ai/dispatch/window-registry.local.json'
def now(): return datetime.now(timezone.utc).isoformat().replace('+00:00','Z')
def read_events(): return [json.loads(x) for x in EVENTS.read_text(encoding='utf-8').splitlines() if x.strip()] if EVENTS.exists() else []
def write_events(xs):
 EVENTS.parent.mkdir(parents=True,exist_ok=True);tmp=EVENTS.with_suffix('.tmp');tmp.write_text(''.join(json.dumps(x,ensure_ascii=False)+'\n' for x in xs),encoding='utf-8');tmp.replace(EVENTS)
def main():
 ap=argparse.ArgumentParser();sp=ap.add_subparsers(dest='cmd',required=True)
 a=sp.add_parser('emit');a.add_argument('--target-role',required=True);a.add_argument('--kind',required=True);a.add_argument('--priority',type=int,default=50);a.add_argument('--payload',default='{}');a.add_argument('--ttl-hours',type=float,default=168)
 q=sp.add_parser('list');q.add_argument('--target-role');q.add_argument('--unacknowledged',action='store_true')
 k=sp.add_parser('ack');k.add_argument('--event-id',required=True)
 h=sp.add_parser('heartbeat');h.add_argument('--role',required=True);h.add_argument('--task-id',default='');h.add_argument('--evidence',default='');h.add_argument('--lease-id',default='')
 l=sp.add_parser('lease');l.add_argument('--role',required=True);l.add_argument('--task-id',required=True);l.add_argument('--ttl-minutes',type=float,default=15)
 w=sp.add_parser('watch');w.add_argument('--role',required=True);w.add_argument('--stale-minutes',type=float,default=3);w.add_argument('--window-status',choices=('active','inProgress','idle','notLoaded','unknown'),default='unknown')
 args=ap.parse_args();xs=read_events()
 if args.cmd=='emit':
  e={'eventId':str(uuid.uuid4()),'targetRole':args.target_role,'kind':args.kind,'priority':args.priority,'createdAt':now(),'expiresAt':(datetime.now(timezone.utc)+timedelta(hours=args.ttl_hours)).isoformat().replace('+00:00','Z'),'payload':json.loads(args.payload),'acknowledgedAt':None};xs.append(e);write_events(xs);print(json.dumps(e,ensure_ascii=False));return
 if args.cmd=='ack':
  matches=[e for e in xs if e['eventId']==args.event_id]
  if not matches: raise SystemExit('event not found')
  matches[0]['acknowledgedAt']=now();write_events(xs);return
 if args.cmd=='heartbeat':
  data=json.loads(HEART.read_text(encoding='utf-8')) if HEART.exists() else {};data[args.role]={'taskId':args.task_id,'leaseId':args.lease_id,'lastActivity':now(),'lastHeartbeat':now(),'lastEvidence':args.evidence};HEART.parent.mkdir(parents=True,exist_ok=True);HEART.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps(data[args.role],ensure_ascii=False));return
 if args.cmd=='lease':
  data=json.loads(LEASES.read_text(encoding='utf-8')) if LEASES.exists() else {};e={'leaseId':str(uuid.uuid4()),'role':args.role,'taskId':args.task_id,'issuedAt':now(),'expiresAt':(datetime.now(timezone.utc)+timedelta(minutes=args.ttl_minutes)).isoformat().replace('+00:00','Z'),'state':'active'};data[args.role]=e;LEASES.parent.mkdir(parents=True,exist_ok=True);LEASES.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps(e,ensure_ascii=False));return
 if args.cmd=='watch':
  role_state={}
  if QUEUE.exists(): role_state=json.loads(QUEUE.read_text(encoding='utf-8')).get('roles',{}).get(args.role,{})
  active=role_state.get('active')
  blocked=[t for t in role_state.get('suspended',[]) if t.get('state')=='blocked']
  cutoff=datetime.now(timezone.utc)-timedelta(minutes=args.stale_minutes);hb=json.loads(HEART.read_text(encoding='utf-8')).get(args.role) if HEART.exists() else None
  stale=bool(active and (args.window_status in ('idle','notLoaded') or not hb or datetime.fromisoformat(hb['lastHeartbeat'].replace('Z','+00:00'))<cutoff))
  registry=json.loads(REGISTRY.read_text(encoding='utf-8')) if REGISTRY.exists() else {}
  window=registry.get('windows',{}).get(args.role,{})
  lease=json.loads(LEASES.read_text(encoding='utf-8')).get(args.role) if LEASES.exists() else None
  lease_expired=bool(active and (not lease or datetime.fromisoformat(lease['expiresAt'].replace('Z','+00:00'))<datetime.now(timezone.utc)))
  pending=[e for e in xs if e.get('targetRole')==args.role and not e.get('acknowledgedAt') and datetime.fromisoformat(e['expiresAt'].replace('Z','+00:00'))>datetime.now(timezone.utc)]
  mismatch=bool(active and window.get('threadId') and hb and hb.get('taskId') and hb.get('taskId')!=active.get('taskId'))
  action='WakeRegisteredWindow' if (stale or lease_expired or mismatch) else ('RouteBlockedTask' if blocked and not active else ('WakeIdleWindow' if pending and not active else 'RecordOnly'))
  print(json.dumps({'role':args.role,'active':active,'blocked':blocked,'stale':stale,'leaseExpired':lease_expired,'stateMismatch':mismatch,'action':action,'threadId':window.get('threadId'),'events':pending},ensure_ascii=False,indent=2));return
 out=[e for e in xs if (not getattr(args,'target_role',None) or e['targetRole']==args.target_role) and (not getattr(args,'unacknowledged',False) or not e.get('acknowledgedAt'))];print(json.dumps(out,ensure_ascii=False,indent=2))
if __name__=='__main__': main()
