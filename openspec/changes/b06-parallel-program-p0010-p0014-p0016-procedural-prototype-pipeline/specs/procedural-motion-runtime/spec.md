## ADDED Requirements

### Requirement: Motion runtime is data driven and domain independent
Motion Core SHALL execute versioned static motion definitions without depending on GF_X, AutoEra gameplay components, product data, scene-specific objects, or runtime state stored in ScriptableObjects. A `MotionRig` SHALL bind stable joint IDs to Transforms, while motion assets SHALL declare strong parameter types, stable node IDs, nodes, and connections.

#### Scenario: The same motion asset is previewed in both projects
- **WHEN** ArtResource supplies a simulated parameter context and the main project supplies an AutoEra Adapter context
- **THEN** Motion Core evaluates the same graph and rig contract without requiring gameplay code in ArtResource

### Requirement: Motion graphs use a restricted primitive set
Motion Core SHALL provide rotation, translation or extension, aiming, opening or closing, continuous rotation, oscillation, and waiting primitives. It SHALL support sequence, parallel, loop, conditional wait, and finite branch composition. Motion assets MUST NOT execute arbitrary scripts or store scene-object and gameplay-component references.

#### Scenario: An author needs a new object-specific behavior
- **WHEN** existing primitives and finite composition can express the requested motion
- **THEN** the author configures the shared graph instead of adding an object-specific Update loop or arbitrary-script node

### Requirement: One executor owns runtime motion state per rig
Each active rig SHALL use one centralized `MotionExecutor` tick. The executor SHALL own preparation, running, completion, cancellation, and recovery state, and SHALL arbitrate exclusive joint channels so incompatible motions cannot write the same channel concurrently.

#### Scenario: A retract request interrupts active work
- **WHEN** the configured safe interruption boundary is reached
- **THEN** the executor releases or transfers channel ownership and moves smoothly from the current measured pose into the declared hold, retract, reset, or immediate-stop policy

### Requirement: Motion never owns gameplay outcomes
Motion Core and its Adapters SHALL consume authoritative state, phase, target Pose, normalized progress, efficiency tier, and interruption state only for presentation. They MUST NOT decide production output, task completion, navigation success, resource settlement, persistence, or offline results.

#### Scenario: A work animation reaches its visual end early
- **WHEN** the motion reaches its configured completion pose before the authoritative task is complete
- **THEN** no gameplay result is committed and presentation remains consistent with the current authoritative state

### Requirement: Adapter writes parameters rather than joints
The AutoEra Adapter SHALL translate product state into strong motion parameters and MUST NOT directly manipulate rig-joint Transforms. ArtResource SHALL use a simulated context implementing the same parameter contract without receiving AutoEra Adapter source.

#### Scenario: A gameplay state changes from working to power loss
- **WHEN** the Adapter observes the authoritative state transition
- **THEN** it updates typed parameters and interruption state, and the executor applies the graph's declared safe response

### Requirement: Motion evaluation is deterministic and allocation controlled
Given the same contract version, graph version, initial pose, parameter sequence, and fixed time steps, Motion Core SHALL produce equivalent states and poses. Runtime hot paths MUST NOT create sustained managed allocations, per-joint Update components, or unbounded coroutine and tween sequences.

#### Scenario: A motion regression is replayed with different frame partitions
- **WHEN** equal total time and parameter changes are evaluated using supported frame-step partitions
- **THEN** the resulting lifecycle state and joint poses remain within declared deterministic tolerances without accumulating drift

### Requirement: Presentation update level is degradable without changing authority
Motion presentation SHALL support full nearby evaluation, reduced-frequency interpolated evaluation, key-pose distant evaluation, and stopped invisible or unloaded evaluation. Degradation MUST NOT alter gameplay state, collision authority, production, navigation, or persistence.

#### Scenario: A running machine leaves the visible range
- **WHEN** its presentation update level is reduced or stopped
- **THEN** gameplay continues independently and the rig reconstructs a correct pose from current authoritative state when full presentation resumes
