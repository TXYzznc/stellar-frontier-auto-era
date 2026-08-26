## ADDED Requirements

### Requirement: Procedural assets pass pre-modeling visual gates
Every procedural machine or mechanism in the production batch SHALL receive an approved axonometric design before modeling. The user SHALL select whether each object requires structure-consistent orthographic views or simplified production notes, and SHALL explicitly approve Blender, AI 3D, or hybrid production per asset. Orthographic documentation for movable assets MUST identify part separation, axes, travel, limits, sockets, work points, and clearance responsibilities.

#### Scenario: Machine modeling is authorized
- **WHEN** the artist is ready to create the machine mesh
- **THEN** the approved axonometric design, user-selected documentation branch, required technical annotations, and user-approved modeling route exist before any DCC or AI 3D operation begins

#### Scenario: Only technical topology changes
- **WHEN** topology, UVs, or LODs change without affecting approved appearance, dimensions, pivots, motion limits, anchors, or interfaces
- **THEN** the artist records the technical change and MAY continue without reopening user visual approval

### Requirement: Movable assets implement the six-layer presentation contract
Every representative movable asset SHALL declare movable structure, motion primitives, environment-sensing inputs, state phases, presentation sequencing, and detail feedback. Art assets MUST provide structure and presentation parameters while runtime gameplay remains authoritative for state, traversal, solving, interruption, and control.

#### Scenario: Machine asset contract is inspected
- **WHEN** the representative machine is prepared for procedural preview
- **THEN** all six layers have explicit responsibilities and no art-side component claims gameplay, navigation, collision, or save authority

### Requirement: Core motion is procedurally driven
Core machine and building-mechanism poses SHALL be driven by parameterized joints, linear or rotational primitives, and phase curves rather than authored keyframe clips. Motion SHALL follow a shared anticipation, acceleration, controlled deceleration, and slight settling language; VFX MAY respond to phase events but MUST NOT control pose.

#### Scenario: Work cycle speed changes
- **WHEN** runtime progress or efficiency parameters change the work cycle
- **THEN** joint timing, pauses, feedback, and completion settling adapt without switching to a keyframed pose sequence

### Requirement: Art assets expose stable procedural anchors and limits
Movable assets SHALL provide correct Pivots, local axes, joint limits, default and safe poses, reusable joint-type defaults, contact points, `KeepOut` volumes, `WorkPoint_*` anchors, and `Socket_*` anchors. Stable identifiers MUST survive art iteration and delivery.

#### Scenario: Updated mesh is re-delivered
- **WHEN** visual geometry changes without an approved contract change
- **THEN** procedural identifiers, pivots, limits, anchors, and safe poses remain compatible with the consuming preview or runtime system

### Requirement: The wheeled prototype supports independent ground contact
The first wheeled prototype SHALL expose four independent wheel contacts, suspension travel, wheel radius, steering axes, and steering limits for four-point smoothed chassis fitting. Continuous slope and step-height rules MUST remain independently configurable, with an initial slope limit of `35°` and step threshold based on wheel radius.

#### Scenario: Prototype traverses representative terrain
- **WHEN** the machine is previewed on flat ground, an approved slope, and a wheel-threshold step
- **THEN** wheel contact, suspension, chassis pose, and passability feedback remain visually coherent without the presentation layer forcing an invalid traversal

### Requirement: Steering and wheel rotation reflect actual movement
The wheeled prototype SHALL support opposite-direction four-wheel steering and same-direction crab steering for low-speed alignment. Wheel rotation MUST derive from actual movement distance and contact state, stop when the machine stops, and spin without translation only during an explicit slip state.

#### Scenario: Machine performs crab alignment
- **WHEN** the chassis moves laterally or diagonally at the allowed alignment speed
- **THEN** all wheel steering pivots, wheel rotation, and chassis heading visually match the commanded crab movement

### Requirement: Work deployment uses lockable wheels and ground-sensing supports
The work sequence SHALL align the chassis, lock wheels, deploy independently ground-sensing supports, and then unfold the arm. Each support SHALL reach a contact within its maximum travel and MAY align its foot pad to the local normal; the chassis MAY apply limited smoothed compensation but MUST NOT simulate full force dynamics.

#### Scenario: Machine begins work on uneven ground
- **WHEN** all required contacts are reachable and the work area is valid
- **THEN** wheels lock, supports establish stable visible contacts, the chassis settles, and only then may the arm enter its work pose

### Requirement: Arm targeting respects reach and clearance
Arm targeting SHALL consume a target position, surface normal, and recommended approach direction. The solver MUST respect joint limits and declared `KeepOut` regions; an unreachable or obstructed target SHALL produce a reposition-required state and a safe hold or retract pose.

#### Scenario: Target is outside reachable space
- **WHEN** the requested work target violates reach or clearance constraints
- **THEN** the arm does not extend through its limits and the system presents a reposition-required state before retrying

### Requirement: Large movement and alignment movement have distinct safety rules
Large chassis movement SHALL require the arm to retract. Low-speed crab alignment MAY retain a raised alignment pose only while the effector is detached from the target, remains inside declared clearance, and performs limited compensation.

#### Scenario: Alignment becomes a relocation
- **WHEN** requested chassis movement exceeds the configured low-speed alignment envelope
- **THEN** the machine safely retracts the arm before executing the relocation

### Requirement: Effector exchange is parameterized and recoverable
Effector exchange SHALL sequence interface alignment, latch unlock, old-effector separation, new-effector insertion, lock, and state confirmation. Cancellation, power loss, or failure MUST stop only at a safe phase boundary and SHALL either roll back an attached effector or retain a safely supported detached state with actionable feedback.

#### Scenario: Power loss occurs after separation
- **WHEN** the old effector has detached and the exchange sequence loses power
- **THEN** the mechanism preserves a safe supported state and reports the recovery action instead of snapping to another effector or dropping the part

### Requirement: Sliding doors and conveyors are parameterized mechanism families
Sliding doors SHALL support single or double leaves, `2m/4m/8m` widths, travel, speed, stop points, status lights, and a safety-sensing region. Conveyors SHALL expose belt or chain circulation, roller rotation, `LoadPoint` and `UnloadPoint`, speed, efficiency, and recoverable blocked-state behavior.

#### Scenario: Door detects temporary occupancy
- **WHEN** the safety region becomes occupied during closing
- **THEN** the door slows, pauses, and reopens or rebounds with warning feedback, escalating to fault only for a persistent abnormal condition

#### Scenario: Conveyor downstream is blocked
- **WHEN** downstream flow cannot accept material
- **THEN** belt, rollers, and represented cargo decelerate in control, preserve their positions, show a waiting state, and resume smoothly after clearance

### Requirement: Status presentation uses three coordinated layers
Operational state SHALL be expressed through machine motion, world-space lights or bands, and a concise HUD-facing summary contract. Green, yellow, and red SHALL represent normal work, alignment/retract/waiting, and fault/interruption respectively, and MUST be paired with icon, shape, or rhythm redundancy.

#### Scenario: Viewer cannot distinguish status color
- **WHEN** color differentiation is unavailable or impaired
- **THEN** motion state plus icon, shape, rhythm, or text still distinguishes normal, waiting, and fault conditions

### Requirement: Prototype acceptance produces visual and measured evidence
The vertical slice SHALL include one wheeled machine, one sliding door, and one conveyor and SHALL provide fixed-camera screenshots, short video or animated evidence, and basic performance readings. Evidence MUST cover near/middle/far readability, terrain, steering, repositioning, supports, arm work, exchange, interruption, mechanism states, and three-layer feedback.

#### Scenario: Prototype is submitted for acceptance
- **WHEN** ART-005 review begins
- **THEN** every required case has a reproducible visual result, expected behavior, measured baseline where applicable, and recorded unresolved risk
