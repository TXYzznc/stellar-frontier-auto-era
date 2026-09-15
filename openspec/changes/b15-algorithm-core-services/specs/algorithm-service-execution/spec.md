## ADDED Requirements

### Requirement: Deterministic atomic evaluation
The system SHALL lock trigger inputs, obtain full compute cost, and commit state and command intents only after successful evaluation; later events SHALL not recursively execute within that batch.

#### Scenario: Failed computation
- **WHEN** division by zero or illegal data occurs after earlier planned writes
- **THEN** no state or world command is committed and diagnostic history identifies the failing node

### Requirement: Authority adapters and bounded events
The system SHALL use existing sensor generations, task queues and navigation authority, retain ordered instantaneous events up to 32, and expose normal waiting/rejection without fatal invalidation or hidden retry.

#### Scenario: Sensor generation changes
- **WHEN** an old binding callback arrives after rebinding or release
- **THEN** it cannot submit a task or replace the current snapshot

#### Scenario: Real service integration
- **WHEN** a real region sensor feeds an explicitly configured graph on a roster machine observed by the existing hardware UI
- **THEN** accepted task navigation and result tracing use the same identity, UI close does not cancel accepted work, and teardown releases listeners and compute

### Requirement: Historical causal diagnostics
The system SHALL retain up to 50 instance runs with revision, trigger inputs, actual path, task/request results and failure reason without pausing the world.

#### Scenario: Current data changes
- **WHEN** current sensor data differs from a retained run
- **THEN** historical inputs remain unchanged and no step or breakpoint control is exposed
