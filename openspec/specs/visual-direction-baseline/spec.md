# visual-direction-baseline Specification

## Purpose
TBD - created by archiving change b02-parallel-art-art001-art003-visual-foundations. Update Purpose after archive.
## Requirements
### Requirement: Visual exploration follows the approved style envelope
Every candidate SHALL use a bright, stylized medium-poly 3D direction with simplified PBR, clear silhouettes, readable functional structure, and controlled detail. Candidates MUST NOT depend on photoreal micro-surface density, AAA-level geometric detail, or a prominent comic outline as the default rendering treatment.

#### Scenario: Candidate style compliance is reviewed
- **WHEN** a visual candidate is submitted for comparison
- **THEN** it demonstrates the approved style envelope in environment, machine, and building examples without relying on an excluded default treatment

### Requirement: At least three candidates form complete comparable systems
ART-002 SHALL produce at least three distinct candidates, and each candidate SHALL cover environment, machine, building, palette, material treatment, lighting, post-processing, and top-down gameplay readability using the same representative content scope.

#### Scenario: Candidate set is ready for review
- **WHEN** the exploration phase ends
- **THEN** at least three candidates contain all required domains and no candidate is represented only by an isolated concept image or a single asset

### Requirement: Candidates use controlled LookDev conditions
The art project SHALL provide a LookDev scene with fixed camera framing, lighting setup, post-processing, scale references, representative object list, and capture viewpoints. Candidate comparison MUST use these controlled conditions.

#### Scenario: Review captures are generated
- **WHEN** candidate screenshots or play-mode views are captured
- **THEN** all candidates use matching cameras, lighting intent, scale references, content coverage, and output framing

### Requirement: Palette preserves gameplay state semantics
Normal environment and decoration colors MUST NOT make large uncontrolled use of the reserved activation and warning colors. Green, yellow, red, and gray-white SHALL remain readable for active, warning, damaged, and inactive states through color plus shape, icon, light behavior, or text where applicable.

#### Scenario: Machine states are placed in each candidate
- **WHEN** inactive, active, warning, and damaged machine examples are viewed against the candidate environment
- **THEN** each state remains distinguishable and is not masked by nearby decorative color usage

### Requirement: Top-down functional readability is evaluated
Each candidate SHALL demonstrate readable machine forward direction, movable parts, effectors, sensors, resource points, building entrances or work areas, traversable space, and visual hierarchy at representative gameplay camera distances.

#### Scenario: Candidate is reviewed from gameplay camera
- **WHEN** the reviewer views the candidate at far, middle, and near representative camera distances
- **THEN** required objects and functional cues remain identifiable without requiring presentation-only close-up framing

### Requirement: Candidate comparison uses a recorded matrix
The review SHALL score or otherwise explicitly compare style fit, top-down readability, machine function readability, state-color conflicts, production cost, modular reuse, procedural-motion compatibility, and scene-performance risk. Subjective preference alone MUST NOT be the only selection evidence.

#### Scenario: Direction decision is proposed
- **WHEN** the candidates are ready for selection
- **THEN** the proposal includes the completed comparison matrix, material evidence, captures, known risks, and a stated recommendation

### Requirement: Exploration converges to one visual baseline
The result of ART-002 SHALL be one approved visual baseline. It MAY select one candidate or combine documented elements from multiple candidates, but the final result SHALL define one coherent silhouette, palette, material, lighting, and detail hierarchy for downstream Art Bible work.

#### Scenario: Hybridized direction is selected
- **WHEN** elements from multiple candidates are combined
- **THEN** the decision records each adopted element and resolves conflicts into one consistent final rule set rather than retaining multiple parallel styles

