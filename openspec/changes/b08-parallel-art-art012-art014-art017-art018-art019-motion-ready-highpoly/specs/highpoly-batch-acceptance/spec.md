## ADDED Requirements

### Requirement: Technical self-review is autonomous per asset
The owning art window SHALL complete and repair each asset's dimensions, hierarchy, Pivot, axes, limits, anchors, support, clearance and publication report without requesting routine producer or user approval. Only a genuine scope conflict, missing authority or unresolved cross-functional dependency may block the task.

#### Scenario: Automatic validation finds a local modeling defect
- **WHEN** a mesh, Pivot, anchor or sweep fails within the frozen contract
- **THEN** the owning window repairs and reruns validation without pausing for an acknowledgement

### Requirement: Unity proxy regression proves motion compatibility
Every accepted asset proxy SHALL bind to the b06 MotionRig contract and pass its relevant bind, named-action, extreme-range, interruption, safe-retract and reset tests without missing references or cumulative drift.

#### Scenario: A formal proxy replaces the functional cubes
- **WHEN** the corresponding b06 motion suite runs against the proxy
- **THEN** all expected stable joints and anchors remain bound and repeated play-reset cycles return exactly to the declared bind or safe pose

### Requirement: User visual review occurs at three dependency gates
The workflow SHALL group user visual review into three ordered batches: wheel/carrier/cargo; fixed carrier/arm/water/saw/drill; sliding door/conveyor. Each asset MUST provide a complete axonometric view and a key-mechanism close-up. A later batch MUST NOT begin before the preceding batch is approved.

#### Scenario: A dependency batch is ready for review
- **WHEN** every asset in the batch has passed autonomous technical validation and proxy regression
- **THEN** the user receives the batch's full views and close-ups together and can approve or return concrete visual defects

### Requirement: Historical models cannot satisfy formal acceptance
Historical ART-004/005 meshes and b06 cube geometry MAY remain as research evidence but MUST NOT be used as formal high-poly inputs, final demonstration meshes or proof of this change's visual completion.

#### Scenario: A candidate derives visible geometry from an old model
- **WHEN** provenance or source comparison shows that the candidate reuses a prohibited historical mesh as its formal base
- **THEN** the candidate fails the batch gate even if its proxy motion tests pass

### Requirement: Batch completion records separate technical and visual evidence
Each batch SHALL record source versions, validation reports, proxy regression results, evaluated triangle counts, full-view images, close-up images and the user's visual decision. Technical evidence MUST NOT be presented as a substitute for visual approval.

#### Scenario: A batch is marked complete
- **WHEN** the batch completion record is reviewed
- **THEN** it contains both passing automated evidence and an explicit user visual approval for every included asset
