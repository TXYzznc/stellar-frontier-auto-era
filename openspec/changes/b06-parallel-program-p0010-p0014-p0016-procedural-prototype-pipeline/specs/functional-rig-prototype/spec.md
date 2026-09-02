## ADDED Requirements

### Requirement: Functional prototypes precede movable visual models
Every movable or key-assembly asset SHALL have a program-built functional prototype accepted before a formal visual model is produced. The prototype SHALL use basic geometry to establish functional dimensions, hierarchy, Pivots, anchors, clearances, collision envelopes, bind poses, safe poses, and motion limits.

#### Scenario: Art modeling is requested for a movable asset
- **WHEN** a movable machine, effector, door, conveyor, or equivalent mechanism is ready for visual production
- **THEN** an accepted functional prototype and matching versioned `FunctionalRigContract` exist before formal modeling begins

### Requirement: FunctionalRigContract is the cross-project structural authority
The system SHALL export a deterministic, versioned `FunctionalRigContract` containing the asset-family identifier, coordinate convention, overall bounds, joint definitions, anchor definitions, clearance and collision volumes, visual slots, bind and safe poses, and compatibility metadata. Art technical documents and delivered visual assets MUST identify the exact contract version they implement.

#### Scenario: Functional contract is exported twice without changes
- **WHEN** the same accepted prototype version is exported repeatedly
- **THEN** the logical contract content, stable identifiers, ordering, and content fingerprint remain identical

#### Scenario: A functional dimension must change during art production
- **WHEN** art review discovers that a Pivot, Socket, motion limit, clearance, or functional dimension must change
- **THEN** the program prototype and contract version are updated and reaccepted before the art document or model adopts the change

### Requirement: Prototype generation is deterministic and idempotent
An Editor Builder SHALL generate or update the prototype hierarchy from declared contract data. Rebuilding the same version MUST preserve stable object identities, local Transforms, hierarchy, bindings, and visual-slot names without duplicating roots, joints, anchors, or geometry.

#### Scenario: Prototype is regenerated after an unrelated project reload
- **WHEN** the Builder runs against an unchanged contract
- **THEN** the resulting hierarchy and serialized bindings match the previously accepted prototype with no duplicate objects

### Requirement: Logic, rig, and visual replacement boundaries remain separate
The prototype SHALL separate the GF.Entity or product logic root, the functional rig hierarchy, and replaceable visual-slot children. Moving a visual joint MUST NOT move the logical root, navigation authority, or gameplay collision authority. A formal model SHALL replace only visual-slot contents unless a new functional contract is approved.

#### Scenario: A formal mesh replaces blockout geometry
- **WHEN** an imported visual model is bound to an accepted prototype
- **THEN** joint Transforms, stable IDs, anchors, limits, logic root, and gameplay authority remain unchanged while only declared visual slots change

### Requirement: Visible prototype parts have credible structural support
Unless a contract explicitly declares flight, suspension, holographic display, or another supported exception, every visible prototype part MUST have a credible connection, attachment, embedding, or contact relationship. Detached, floating, or transform-drifted parts SHALL fail structural validation.

#### Scenario: A generated blockout part has no support path
- **WHEN** structural validation cannot trace a visible part to its intended joint, mount, chassis, rail, socket, or contact surface
- **THEN** the prototype fails before motion or visual-model production is authorized

### Requirement: First vertical slice covers representative functional structures
The first accepted prototype batch SHALL include a wheeled carrier, four wheel structures, a multi-joint arm, replaceable effectors, a sliding door, and a conveyor. Together they MUST expose rotation, translation, looping, target alignment, ground-contact input, interruption, safe recovery, and replaceable visual slots.

#### Scenario: Prototype scope is submitted for first-gate acceptance
- **WHEN** the first vertical slice reaches review
- **THEN** all six representative structures and their required functional contracts are present rather than substituting isolated object-specific demos

## MODIFIED Requirements

### Requirement: Machine blockouts are motion-ready
Any representative movable machine or key mechanism SHALL first be generated as a contract-level program prototype composed of replaceable basic-geometry visual slots. The accepted prototype and its versioned `FunctionalRigContract` SHALL define independently movable rigid parts, correct Pivots, local axes, stable joint identifiers, joint limits, default and safe poses, sockets, work points, VFX points, clearance volumes, collision envelopes, and visual replacement boundaries before formal art modeling begins. Delivered visual joints MUST bind to the accepted slots and MUST NOT redefine the logical root, navigation authority, gameplay collision authority, or contract identifiers.

#### Scenario: Representative machine contract is validated
- **WHEN** the program prototype is inspected before formal visual modeling
- **THEN** every movable part can be addressed and moved within its declared range, every required anchor and clearance is machine-readable, and visual geometry can be replaced without moving the logical root or changing the functional contract

#### Scenario: Delivered art attempts to change a functional binding
- **WHEN** a visual replacement changes a declared Pivot, Socket, joint ID, limit, bind pose, or clearance without a newer accepted contract
- **THEN** integration fails and returns the change to the program-prototype contract workflow
