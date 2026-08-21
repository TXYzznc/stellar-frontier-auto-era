## ADDED Requirements

### Requirement: ArtResource is the sole art source of truth
The production workflow SHALL treat `D:\unity\UnityProject\ArtResource` as the sole authority for editable art sources, Unity art authoring assets, LookDev scenes, and export-ready art prefabs. The main project MUST consume delivered assets and MUST NOT become an alternate source for art revisions.

#### Scenario: An imported asset needs revision
- **WHEN** review in the main project identifies a visual or technical defect in a delivered asset
- **THEN** the defect is corrected in `ArtResource`, re-exported, and re-imported instead of editing the main-project copy as the new source

### Requirement: The art project operates independently
The art project SHALL open, author, preview, and review its assets without requiring GF_X or AutoEra gameplay code. Shared tooling MUST be limited to explicitly versioned art-facing contracts or packages.

#### Scenario: Artist reviews assets without the game project
- **WHEN** `ArtResource` is opened by itself
- **THEN** its LookDev scenes and source prefabs can be inspected without loading the main project or resolving AutoEra gameplay components

### Requirement: Delivery contract fixes spatial and asset conventions
The delivery contract SHALL define one Unity unit as one meter and SHALL define coordinate handedness, model forward, origin, Pivot, local joint axes, naming, material and texture rules, prefab boundaries, sockets, work points, VFX points, status-light points, and required metadata.

#### Scenario: A sample asset is checked before export
- **WHEN** the sample prefab is validated against the delivery contract
- **THEN** every required convention and anchor has an explicit pass or actionable failure result

### Requirement: Editable sources are separated from Unity delivery assets
Editable DCC sources and intermediate authoring files MUST be stored separately from Unity-ready exported models, textures, materials, prefabs, and configuration assets. A delivery package MUST contain only files required by its declared runtime or review purpose.

#### Scenario: Minimal package contents are reviewed
- **WHEN** the ART-001 sample package is prepared
- **THEN** editable DCC sources, temporary renders, caches, and unrelated project assets are excluded from the package

### Requirement: Minimal package round-trip preserves identity and appearance
The workflow SHALL export a versioned minimal `.unitypackage` from `ArtResource` and import it into the main project while preserving `.meta` files, GUIDs, scale, orientation, materials, anchors, prefab references, and declared dependencies. Re-importing the same package version MUST NOT duplicate assets or break references.

#### Scenario: First clean import succeeds
- **WHEN** the minimal package is imported into its declared main-project destination
- **THEN** the sample appears with the same scale, orientation, materials, anchors, GUIDs, and prefab references as in `ArtResource`

#### Scenario: Same version is imported again
- **WHEN** the identical package version is re-imported
- **THEN** Unity updates the same assets without creating duplicate identities or losing existing bindings

### Requirement: Art and motion-tool packages remain separate
Art delivery packages MUST NOT contain GF_X, AutoEra gameplay code, AutoEra Adapter code, or the source of Motion Core and Motion Editor. Versioned Motion Core and Editor tooling, when available, SHALL be distributed independently from art resource packages.

#### Scenario: Art package dependency audit runs
- **WHEN** the minimal art package manifest and contents are inspected
- **THEN** no framework core, project gameplay code, Adapter code, or motion-tool source is included

### Requirement: Machine blockouts are motion-ready
Any representative machine used by this change SHALL consist of independently movable rigid parts with correct Pivots, local axes, stable joint identifiers, joint limits, default poses, sockets, work points, VFX points, and safe-retract poses. Visual joints MUST NOT redefine the logical root, navigation authority, or gameplay collision authority.

#### Scenario: Representative machine contract is validated
- **WHEN** the representative machine blockout is inspected before medium-detail modeling
- **THEN** every movable part can be addressed and moved within its declared range without moving the logical root or invalidating required anchors

### Requirement: Task workbook remains user-owned
The change SHALL treat `第一版开发任务表.xlsx` as AI read-only. Implementation evidence MAY recommend task status, actual effort, or dependency changes, but automated work MUST NOT write them into the workbook.

#### Scenario: ART-001 evidence is completed
- **WHEN** delivery-contract acceptance evidence is produced
- **THEN** the evidence is recorded outside the task workbook and any workbook update is left for the user
