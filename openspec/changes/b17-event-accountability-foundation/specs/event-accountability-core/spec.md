## ADDED Requirements

### Requirement: Session-scoped correlation allocation
The system SHALL allocate correlation IDs from a session-scoped monotonic allocator where zero is reserved as invalid, and correlation IDs SHALL be independent of persistent object identity allocation.

#### Scenario: Monotonic and valid
- **WHEN** a session allocates correlation IDs repeatedly
- **THEN** each ID is valid, strictly increasing and never reissued within the session

### Requirement: Facts-only event bus boundary
The system SHALL restrict the event bus to fact envelopes and SHALL NOT provide command or query event types on the bus; commands open an explicit correlation ticket and queries use explicit read-only interfaces.

#### Scenario: Command is not an event
- **WHEN** a caller opens a command correlation and no terminal fact has been published
- **THEN** no event reaches the bus and the journal contains only the pending command record

### Requirement: Trigger traceability
The system SHALL journal commands and facts so that one trigger can be traced from its correlation to the source, the fact chain and the final outcome recorded by the terminal fact.

#### Scenario: Command to final result
- **WHEN** a command correlation receives a terminal fact carrying that correlation as causation
- **THEN** the trace resolves to the source action and the fact's outcome, and later reads keep returning the resolved result

### Requirement: Bounded journal memory
The system SHALL keep the journal in a fixed-capacity preallocated ring buffer that overwrites the oldest records without unbounded growth.

#### Scenario: Capacity wraparound
- **WHEN** more records than the journal capacity are appended
- **THEN** the oldest records are replaced and the journal memory footprint stays constant

### Requirement: Steady-state allocation discipline
The system SHALL keep event publishing and journaling free of steady-state heap allocation on the regular path, using pooled fact envelopes and preallocated journal storage.

#### Scenario: Repeated publishing
- **WHEN** facts are published repeatedly during normal operation
- **THEN** no new journal storage is allocated per publish and fact envelopes return to their pool