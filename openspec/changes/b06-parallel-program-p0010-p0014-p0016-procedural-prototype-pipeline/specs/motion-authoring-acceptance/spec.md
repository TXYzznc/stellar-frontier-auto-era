## ADDED Requirements

### Requirement: Motion authoring exposes inspectable configuration and validation
Motion Editor SHALL provide Inspector authoring for rig bindings, motion assets, typed parameters, phase curves, interruption policies, and test contexts. It SHALL provide joint-range Gizmos and static validation for missing joints, duplicate IDs, invalid limits, parameter-type mismatches, channel conflicts, unsupported connections, and contract-version mismatches.

#### Scenario: An author previews an invalid graph
- **WHEN** a required joint is missing or two incompatible nodes claim the same channel
- **THEN** validation reports an actionable error before export or Play Mode execution

### Requirement: Visual node editor is not a first-stage dependency
The first production stage SHALL remain fully authorable and testable through structured assets and Inspector tooling. A visual node editor MUST NOT block the functional prototype, Motion Core, Editor preview, package publication, or vertical-slice acceptance.

#### Scenario: Inspector workflow completes the representative slice
- **WHEN** all required representative motions can be authored and debugged with structured assets
- **THEN** no visual node-editor work is added to the current change

### Requirement: Motion tools publish independently with stable identity
Motion Core and Motion Editor SHALL be published from the main project to ArtResource as a versioned tool package with fixed GUIDs and an explicit manifest. The package MUST exclude GF_X, AutoEra Adapter, gameplay code, product scenes, and art-delivery assets. Reimporting the same package version MUST preserve motion-asset and rig references.

#### Scenario: ArtResource receives the same tool version twice
- **WHEN** the versioned Motion package is imported again
- **THEN** existing rig, contract, and motion-asset references remain bound without duplicate tool assets

### Requirement: Acceptance uses three independent evidence layers
Every accepted representative prototype SHALL pass automatic structure validation, deterministic motion regression, and a fixed visual demonstration. Structure and motion checks SHALL be completed by the responsible program and test roles without requiring routine user or producer acknowledgment; the user SHALL review the final visual result.

#### Scenario: Numerical motion tests pass but geometry visibly intersects
- **WHEN** automated tests pass and the fixed demonstration exposes unsupported, intersecting, discontinuous, or unreadable movement
- **THEN** the prototype remains unaccepted and formal modeling is not authorized

### Requirement: Fixed test panel provides repeatable controls
The development test panel SHALL expose each completed motion capability through named controls and editable test parameters, including play, pause, reset, interruption, recovery, positive cases, and negative cases. Reset or stopping preview MUST restore the accepted bind baseline with no cumulative drift.

#### Scenario: User or tester replays a completed capability
- **WHEN** the corresponding panel action is triggered repeatedly
- **THEN** the same expected behavior is observable and every reset returns to the same baseline

### Requirement: Vertical slice validates shared motion rather than bespoke demos
The fixed acceptance scene SHALL exercise the wheeled carrier, wheel structures, arm, effectors, sliding door, and conveyor through shared Motion Core assets and declared Adapters or test contexts. Object-specific preview scripts MUST NOT substitute for shared executor, lifecycle, channel, and interruption behavior.

#### Scenario: First vertical slice is submitted
- **WHEN** the program team declares `P0-016` representative integration ready
- **THEN** movement, steering, target work, safe interruption, power-loss recovery, effector exchange, door occupancy handling, conveyor blocking, repeated playback, pooling, and scene re-entry have reproducible evidence

### Requirement: Formal visual replacement reruns the same acceptance suite
After art replaces blockout geometry, the same contract validation, motion regression, and visual demonstration SHALL run without moving functional Pivots, anchors, limits, or logic roots. The replacement SHALL not be accepted solely from static renders.

#### Scenario: A formal model is imported for an accepted rig
- **WHEN** visual slots are replaced with the delivered model
- **THEN** contract compatibility, all representative motions, reset behavior, and user-visible continuity pass again before the model enters production use
