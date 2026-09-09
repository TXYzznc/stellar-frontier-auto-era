## ADDED Requirements

### Requirement: Each asset has one authoritative Blender source
The DCC workflow SHALL maintain exactly one authoritative high-poly source per asset version with fixed `HP_VISUAL`, `MOTION_RIG` and `PROXY_SHAPES` responsibilities. A separately hand-maintained proxy model MUST NOT become a second source of truth.

#### Scenario: An artist edits a joint or visible part
- **WHEN** the authoritative Blender source changes
- **THEN** all proxy and contract outputs are regenerated from that same saved source and carry its source fingerprint

### Requirement: Contract proxy publication is deterministic
The publication tool SHALL generate a lightweight contract proxy FBX, a machine-readable motion contract and a validation report from the authoritative source. Repeating publication without source changes MUST produce logically identical hierarchy, transforms, stable IDs and contract content.

#### Scenario: The same source is published twice
- **WHEN** two publications use the same saved source and profile
- **THEN** their normalized contracts and proxy hierarchy compare equal and no duplicate stable identity is created

### Requirement: DCC validation reports actionable structural defects
Publication MUST validate naming, parent relationships, local axes, bind transforms, units, required anchors, unsupported visible parts and declared motion sweeps. A failure MUST identify the affected object or stable ID and MUST block the proxy from being treated as accepted.

#### Scenario: A visible component has no structural parent
- **WHEN** publication finds an undeclared floating or orphaned visible component
- **THEN** the report names that component, marks publication failed and does not issue an accepted proxy manifest

#### Scenario: A motion part violates an extreme-pose clearance
- **WHEN** an automatic sweep finds self-intersection or host interference inside the configured range
- **THEN** the report identifies the joint and failing pose and blocks acceptance

### Requirement: Unity consumes only the lightweight high-poly-stage proxy
The high-poly stage SHALL import the contract proxy rather than the complete high-poly mesh into the main project. Axis conversion MAY exist only under a visual import wrapper; the deployment root and semantic joints MUST remain identity and unit scale.

#### Scenario: A proxy replaces b06 visual slots
- **WHEN** the proxy is imported and bound to an existing MotionRig
- **THEN** stable references resolve, the deployment root remains identity, and the original MotionGraphAssets execute without rebinding by object name guesses
