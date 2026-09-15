## ADDED Requirements

### Requirement: Public area approach
The system SHALL select reachable compatible positions from public work regions, required capability, carrier size and facing, using the existing region reservation queue.

#### Scenario: Contended work channel
- **WHEN** another machine owns the channel
- **THEN** the requester waits without repeated path planning and receives a waiting prompt after 30 seconds rather than navigation failure

#### Scenario: Mine and water differences
- **WHEN** a work target describes a mine region or a ballistic water envelope
- **THEN** mine approach stays inside its effective region and water compatibility uses the supplied envelope without a fixed 4 metre range

#### Scenario: Public waiting position
- **WHEN** the occupied work channel provides a waiting position
- **THEN** the carrier plans at most one approach to that position, releases movement compute on stopping, and only plans work approach after the reservation is granted

#### Scenario: Reservation handoff
- **WHEN** the owner cancels or explicitly releases the channel after work
- **THEN** the next valid requester can approach and reservation alone never counts as arrival

### Requirement: Native path and actual motion
The system SHALL use the installed Unity NavMesh module for static routing and basic multi-agent avoidance and derive Motion projection from real movement.

#### Scenario: Isolated formal resource regression
- **WHEN** formal wheeled instances navigate around a static obstacle in an isolated PlayMode fixture
- **THEN** paths and actual positions avoid the obstacle, machines stop and align, and wheel motion reflects travelled distance

#### Scenario: Safe test teardown
- **WHEN** the fixture finishes or fails
- **THEN** temporary instances and NavMesh data are removed and formal resources, scenes, xlsx and global settings remain untouched
