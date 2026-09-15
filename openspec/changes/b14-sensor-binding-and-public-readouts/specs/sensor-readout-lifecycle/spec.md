## ADDED Requirements

### Requirement: Identity and freshness
The service SHALL bind one permanent target per installed sensor, preserve identity across rename, and reject stale binding generations.

#### Scenario: Rebinding or invalidation
- **WHEN** a target changes, disappears, or a sensor stops
- **THEN** live reads become invalid and old samples remain diagnostic only

### Requirement: Fixed sampling and compute
All supported sensor levels SHALL sample every second, reserve 10 compute, and use object/soil ranges of 6/4 metres from an explicit component anchor to the closest public region point.

#### Scenario: Range and missing provider
- **WHEN** the target is out of range
- **THEN** monitoring retains compute but live data is invalid

#### Scenario: Release conditions
- **WHEN** unbound, permanently deleted, provider unavailable, stopped, sleeping, unpowered, detached or leaving region
- **THEN** waiting and running leases are released exactly once

#### Scenario: Scheduling
- **WHEN** compute is insufficient or yielded at a sampling boundary
- **THEN** at most one pending request remains and recovery samples current state without history catch-up
