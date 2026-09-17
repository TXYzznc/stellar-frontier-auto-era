## ADDED Requirements

### Requirement: Session-owned event service
The system SHALL own the event service inside the world session with symmetric construction and disposal, without static state or global registries.

#### Scenario: Session disposal
- **WHEN** a world session is disposed
- **THEN** its event service stops accepting publishes and holds no references that outlive the session

### Requirement: Framework bridge without core coupling
The system SHALL publish session facts through an injected publisher abstraction; the pure C# core SHALL NOT reference Unity framework entry points, and the application layer SHALL provide the GF bridge.

#### Scenario: Pure session without GF
- **WHEN** a session is created without a publisher in EditMode tests
- **THEN** journaling and traceability still work and no framework component is touched

#### Scenario: GF-attached session
- **WHEN** a session is created with the GF publisher bridge
- **THEN** every published fact is fired through the GF event component

### Requirement: Optional adoption keeps existing contracts
The system SHALL add the event service to existing scheduling surfaces through optional parameters so that absent services skip publishing and existing behavior is unchanged.

#### Scenario: Task queue without service
- **WHEN** a machine task queue is constructed without an event service
- **THEN** task scheduling behaves exactly as before and publishes nothing