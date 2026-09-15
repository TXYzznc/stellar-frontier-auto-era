## ADDED Requirements

### Requirement: Typed stable graph documents
The system SHALL retain stable node IDs, explicit typed ports and defaults, graph versions and invalid edge references independently of layout.

#### Scenario: Round trip and invalid connection
- **WHEN** a graph is saved and restored or an incompatible connection is proposed
- **THEN** IDs remain stable and type/unit/enum/capability violations produce located errors rather than implicit conversion

### Requirement: Complete validation before compilation
The system SHALL reject missing required inputs, unsupported nodes, capacity overflow and synchronous cycles and SHALL compile a private immutable plan only after validation.

#### Scenario: Invalid draft
- **WHEN** a draft contains an immediate cycle or missing binding
- **THEN** validation reports the affected nodes and no executable plan is published
