# authored-environment-generation Specification

## Purpose
TBD - created by archiving change b02-parallel-art-art001-art003-visual-foundations. Update Purpose after archive.
## Requirements
### Requirement: Gameplay skeleton is authored and stable
The initial map's base, roads, farms, resource points, primary routes, and buildable spaces SHALL be placed or approved as a fixed gameplay skeleton. Visual generation MUST NOT move, remove, or invalidate these authoritative gameplay elements.

#### Scenario: Visual environment is regenerated
- **WHEN** an authorized visual region is regenerated in the Editor
- **THEN** fixed gameplay objects, routes, work areas, and buildable clearances retain their approved transforms and usable space

### Requirement: Editor generation owns only visual environment content
Editor tooling MAY generate terrain regions, surface transitions, vegetation communities, rocks, and non-interactive decoration inside declared visual regions. It MUST NOT create authoritative gameplay identities for each decorative instance or determine production, inventory, navigation, or save-state outcomes.

#### Scenario: Dense mineral decoration is generated
- **WHEN** many decorative ore fragments are placed around a resource point
- **THEN** the fragments remain visual instances while the resource point remains the single authoritative gameplay object

### Requirement: Generated results are persistent and manually editable
Generated visual content SHALL be baked or saved into a deterministic scene result that can be reviewed, manually adjusted, selectively regenerated, and reopened without requiring runtime generation.

#### Scenario: Artist manually refines generated vegetation
- **WHEN** the artist removes or moves generated instances around a focal area and saves the scene
- **THEN** the refinement persists on reopen and is not overwritten unless the affected region is explicitly regenerated

### Requirement: First version uses one determined map
All first-version saves SHALL use the same authored scene result. The implementation MUST NOT add per-save random map seeds, infinite chunks, runtime terrain reconstruction, or migration of old saves between generation algorithm versions.

#### Scenario: Two new saves enter the first-version map
- **WHEN** two independent first-version saves load the initial area
- **THEN** both receive the same gameplay skeleton and baked visual environment layout

### Requirement: Representative sample compares implementation approaches
ART-003 SHALL use the same representative test area to perform lightweight comparisons of Unity Terrain, modular construction, hand dressing, and the approved hybrid approach. The comparison SHALL evaluate visual quality, authoring speed, reuse, performance risk, manual override, and extension cost.

#### Scenario: Scene route recommendation is finalized
- **WHEN** comparison evidence is complete
- **THEN** the result documents why the hybrid route remains viable, assigns responsibilities to each technique, and records any revisit trigger that was reached

### Requirement: Scene sample covers required gameplay and visual cases
The sample SHALL include a base or engineering facility area, natural environment, clear traversal route, one authoritative resource point with a visual community, terrain transitions, vegetation, rocks, and at least two representative lighting conditions or equivalent day and night readability evidence.

#### Scenario: Sample review package is assembled
- **WHEN** ART-003 is submitted for review
- **THEN** the package provides fixed gameplay-camera views that show every required case at useful far, middle, and near distances

### Requirement: Generated decoration respects gameplay clearances
Visual generation SHALL exclude or constrain content around roads, navigation corridors, machine work points, loading points, selection areas, building footprints, and other declared gameplay clearances.

#### Scenario: Vegetation generation reaches a machine work area
- **WHEN** the generator evaluates positions inside the work-area exclusion volume
- **THEN** it omits or relocates those instances so operation and visual readability remain unobstructed

### Requirement: Scene decision includes measured performance evidence
The sample SHALL record representative object counts, Renderer and material characteristics, batching or instancing conditions, CPU and GPU frame timing, and observed bottlenecks in the approved Unity 2022.3 environment. Final density and LOD budgets MAY remain unresolved, but review MUST NOT rely only on subjective smoothness.

#### Scenario: Performance evidence is reviewed
- **WHEN** the representative sample is profiled from its declared gameplay camera and quality settings
- **THEN** the report contains reproducible scene conditions, measured evidence, detected bottlenecks, and follow-up budget decisions or explicitly recorded unknowns

### Requirement: Visual generation remains outside runtime save authority
Generated decoration and its editor tooling MUST NOT alter permanent gameplay IDs, runtime save schemas, offline simulation results, or authoritative collision and navigation ownership.

#### Scenario: Baked visual layout is replaced during development
- **WHEN** a new approved visual bake replaces the previous development scene layout
- **THEN** gameplay identity and persistence contracts remain unchanged and no migration logic is introduced for decorative instances

