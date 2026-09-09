## ADDED Requirements

### Requirement: Product scale and functional motion use separate authorities
The production workflow SHALL use product greybox specifications as the authority for dimensions, footprint, installation envelope and forward direction, while retaining b06 as the authority for stable joint semantics, local axes, motion ranges, anchors and safe states. Runtime parent scaling MUST NOT be used to reconcile conflicts.

#### Scenario: Prototype geometry conflicts with product size
- **WHEN** a b06 prototype envelope or link length exceeds the approved product dimensions
- **THEN** the model repositions joints and redesigns visible structure within the product contract while preserving stable motion semantics and limits

### Requirement: High-poly assets are motion-ready at source
Every movable high-poly asset SHALL separate visible parts by their owning MotionRig joint from the start. Each movable object MUST use the real Pivot and declared local axis, MUST have rotation zero and scale one in bind pose, and MUST NOT weld geometry across two joints.

#### Scenario: An articulated high-poly source is inspected
- **WHEN** the source is evaluated in bind pose and every declared extreme pose
- **THEN** each visible part follows one correct joint, the chain remains connected, and no part self-intersects or becomes unsupported

### Requirement: High-poly detail is structural and bake-ready
High-poly acceptance SHALL require real geometry for silhouettes, load-bearing structures, hard-surface seams, rails, bearings, hinges, protected cables, recessed vents, interfaces, major fasteners and motion clearances. Triangle counts SHALL be reported as evaluated warning ranges and MUST NOT replace visual or structural acceptance.

#### Scenario: A model reaches its triangle warning range
- **WHEN** evaluated geometry is counted after high-poly modifiers
- **THEN** the model passes only if close curves are smooth, bevel highlights are continuous, structural depth is real and excess subdivisions are not hidden or meaningless

#### Scenario: A model is below its warning range
- **WHEN** the evaluated triangle count is below the agreed asset-family range
- **THEN** the asset is rejected or the owner records concrete evidence that all hard quality gates still pass

### Requirement: Formal asset dimensions and motion limits remain testable
The high-poly sources SHALL implement the frozen vehicle, wheel, mount, carrier, arm, effector, cargo, door and conveyor dimensions and motion limits recorded by this change. Working sweeps MAY exceed stowed envelopes only through declared KeepOut volumes.

#### Scenario: A compact effector is fully exercised
- **WHEN** the effector is checked in stowed, working extreme and safe-retract poses
- **THEN** it fits the common mount and stowed envelope, reaches every declared motion limit, avoids the host and neighboring slot, and returns to a valid safe pose

### Requirement: Materials and runtime optimization are deferred explicitly
This stage MUST NOT claim completion of production materials, textures, shaders, low-poly topology, final UV baking or LOD. High-poly completion SHALL remain a prerequisite for those later stages.

#### Scenario: A high-poly batch passes visual review
- **WHEN** the user approves the batch's form and detail
- **THEN** the batch is marked ready for a separate material and low-poly workflow rather than ready for runtime shipping
