## ADDED Requirements

### Requirement: Authority-preserving bridge
The bridge SHALL project navigation and work authority into Motion presentation without completing, cancelling, or changing the authoritative task by presentation state alone.

#### Scenario: Navigation reaches target
- **WHEN** the authoritative navigation reports completion
- **THEN** the bridge transitions the matching MotionExecutor presentation and leaves task outcome unchanged

### Requirement: Symmetric lifecycle
The bridge SHALL release presentation ownership and work reservations on cancellation, power loss, region exit, and repeated execution without residual subscriptions or duplicate commands.

#### Scenario: Region exits while working
- **WHEN** the region releases its machine and work queue
- **THEN** the bridge releases motion channels and reservations exactly once and emits no late command
