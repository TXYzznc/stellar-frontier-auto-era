## ADDED Requirements

### Requirement: Draft and applied state isolation
The system SHALL separate mutable drafts from applied plans and runtime state, revalidate versions and bindings at safe publication, and preserve the old plan on failure.

#### Scenario: Draft changes during pending apply
- **WHEN** an accepted revision is waiting at a safe boundary and the draft changes
- **THEN** the new draft is not silently included in that request and stale publication is rejected

#### Scenario: Parameters versus structure
- **WHEN** a validated parameter-only update or structural update is safely applied
- **THEN** parameters preserve state while changed structure resets only the affected instance

#### Scenario: Other instances during machine restart
- **WHEN** a structural application enters the machine restart boundary
- **THEN** other local instances pause during that boundary and resume with their preserved state, in accordance with DEC-097

### Requirement: Templates and memory restore
The system SHALL strip concrete bindings and running state from templates, protect system templates, and restore explicit versioned memory DTOs at complete batch boundaries.

#### Scenario: Template instantiated twice
- **WHEN** one template creates two instances
- **THEN** each receives independent node IDs and state and requires explicit binding

#### Scenario: Memory restore
- **WHEN** state is captured and restored at a safe boundary
- **THEN** saved state and pending application identity are preserved without replaying a completed event or claiming disk/offline support
