import json,subprocess,tempfile
from pathlib import Path
p=Path(tempfile.mkdtemp());q=p/'q';r=p/'r';l=p/'l'
def run(qv,lv,st):
 q.write_text(json.dumps({'roles':{'x':qv}}));r.write_text(json.dumps({'windows':{'x':{'threadId':'TEST'}}}));l.write_text(json.dumps(lv));return json.loads(subprocess.check_output(['python','tools/agent_watchdog.py','--role','x','--window-state',f'x={st}','--queue-file',str(q),'--registry-file',str(r),'--lifecycle-file',str(l)],text=True))[0]
assert run({'active':{'taskId':'a'},'pending':[]},{'a':{'taskId':'a','state':'Active'}},'idle')['action']=='wake'
assert run({'active':{'taskId':'a'},'pending':[]},{'a':{'taskId':'a','state':'Active'}},'running')['action']=='none'
assert run({'active':{'taskId':'a'},'pending':[]},{'a':{'taskId':'a','state':'AwaitingProducerAcceptance'}},'idle')['action']=='none'
assert run({'active':None,'pending':[{'taskId':'p','state':'queued'}]},{},'notLoaded')['action']=='wake'
assert run({'active':None,'pending':[{'taskId':'p','state':'queued'}]},{},'paused')['action']=='none'
assert run({'active':None,'pending':[]},{},'idle')['action']=='none'
print('watchdog acceptance: 6/6 passed')

