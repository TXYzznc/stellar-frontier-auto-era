#!/usr/bin/env python3
"""Window state adapter. A desktop bridge may atomically write this file."""
import argparse,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]; STATE=ROOT/'.ai/dispatch/window-state.local.json'
def main():
 ap=argparse.ArgumentParser();ap.add_argument('--role');ap.add_argument('--state',choices=['running','idle','notLoaded','paused','unknown']);ap.add_argument('--reason',default='');a=ap.parse_args();data={}
 if STATE.exists():
  try:data=json.loads(STATE.read_text(encoding='utf8'))
  except json.JSONDecodeError: data={}
 if a.role and a.state:
  data[a.role]={'state':a.state,'reason':a.reason,'updatedAt':__import__('datetime').datetime.now(__import__('datetime').timezone.utc).isoformat()};STATE.parent.mkdir(parents=True,exist_ok=True);tmp=STATE.with_suffix('.tmp');tmp.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf8');tmp.replace(STATE)
 print(json.dumps(data.get(a.role,data) if a.role else data,ensure_ascii=False,indent=2))
if __name__=='__main__':main()

