## ADDED Requirements

### Requirement: Carrier navigation lifecycle
The system SHALL treat navigation as a carrier capability and use the existing task activity lifecycle without adding an effector slot.

#### Scenario: Fixed carrier rejection
- **WHEN** a carrier without movement capability receives a move request
- **THEN** the request is rejected without allocating compute or a task activity

#### Scenario: Actual arrival
- **WHEN** a valid route finishes
- **THEN** success requires actual proximity, stopped movement and required facing, not animation progress

### Requirement: Compute and recovery gates
The system SHALL reserve 10 compute for each initial or repeated plan and 5 protected compute during movement, with configuration-backed timing and speed.

#### Scenario: Insufficient compute
- **WHEN** planning or movement budget is unavailable
- **THEN** the machine remains stopped and at most one pending planning request exists

#### Scenario: Blocked recovery
- **WHEN** effective displacement stops for 3 seconds
- **THEN** the service attempts a budgeted replan and warns after 10 seconds of unresolved blockage, failing after 30 seconds

#### Scenario: Power loss
- **WHEN** power or core capacity is lost during movement
- **THEN** movement stops safely and does not continue an unbudgeted route

### Requirement: Exactly once cleanup
The system SHALL stop within 0.5 seconds of cancellation and terminate once for cancellation, preemption, invalid target, recovery or region exit, releasing compute and reservations.

#### Scenario: Repeated cancellation and disposal
- **WHEN** cancellation is followed by disposal or target removal
- **THEN** only one terminal result and one task activity completion occur and no owned compute or reservation remains
