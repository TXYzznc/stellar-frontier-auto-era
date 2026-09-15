from datetime import datetime,timezone,timedelta

def state_is_fresh(entry,minutes=5):
 if not entry or not entry.get('updatedAt'): return False
 try:return datetime.now(timezone.utc)-datetime.fromisoformat(entry['updatedAt'].replace('Z','+00:00')) <= timedelta(minutes=minutes)
 except ValueError:return False

def effective_state(file_entry,reported='unknown',minutes=5):
 if file_entry and state_is_fresh(file_entry,minutes): return file_entry.get('state','unknown')
 return reported if reported in {'running','inProgress','idle','notLoaded','paused','unknown'} else 'unknown'

