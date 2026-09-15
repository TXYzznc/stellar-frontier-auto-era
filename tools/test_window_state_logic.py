from datetime import datetime,timezone,timedelta
from window_state_logic import state_is_fresh,effective_state
fresh={'state':'running','updatedAt':datetime.now(timezone.utc).isoformat()}
stale={'state':'running','updatedAt':(datetime.now(timezone.utc)-timedelta(minutes=6)).isoformat()}
assert state_is_fresh(fresh)
assert not state_is_fresh(stale)
assert effective_state(fresh)=='running'
assert effective_state(stale)=='unknown'
assert effective_state(None,'idle')=='idle'
print('window state acceptance: 5/5 passed')


