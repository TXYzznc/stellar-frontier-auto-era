## ADDED Requirements

### Requirement: Modular assets pass pre-modeling visual gates
Every object in a modular-art production batch SHALL receive an axonometric view before modeling. After user approval, objects selected by the user SHALL receive structure-consistent front, side, and top views with type-appropriate technical annotations; objects explicitly exempted from three-view production MUST instead receive dimensions, grounding, orientation, material regions, and necessary structural notes. Each object MUST receive explicit user approval for Blender, AI 3D, or hybrid production before modeling starts.

#### Scenario: Batch is ready to begin modeling
- **WHEN** an artist requests permission to model any object in the batch
- **THEN** the complete batch inventory, approved axonometric view, user-selected documentation branch, approved production drawing or simplified notes, and approved per-asset modeling route are recorded

#### Scenario: Structural change is discovered during modeling
- **WHEN** a proposed change affects silhouette, proportion, structure, module relationships, or movable parts
- **THEN** modeling stops for that object and the changed axonometric design returns to user approval before production resumes

### Requirement: Completed modular models pass independent functional and support review
Before submitting a completed modular model, the art lead SHALL independently verify it against all declared functional requirements, approved visual references, technical documentation, assembly relationships, and mechanism contracts. Ordinary findings MUST be corrected within the art window and MUST NOT require per-asset producer review. Unless an approved task package or technical contract explicitly requires levitation, flight, magnetic suspension, holographic presentation, or equivalent behavior, every visible object and part MUST have a credible support, connection, attachment, embedding, or contact relationship. Unsupported floating objects, detached loose parts, visible unintended gaps, and transform-, hierarchy-, origin-, or export-axis-induced drift SHALL be hard acceptance failures.

#### Scenario: Modular model is ready for user review
- **WHEN** the art lead completes the pre-submission structural review
- **THEN** every required function maps to valid model structure, every visible part has an approved support or attachment relationship, ordinary defects have already been corrected, and no producer approval was requested for that routine self-review

#### Scenario: Unapproved floating part is found
- **WHEN** a visible part has no credible support or attachment and no explicit suspension exception exists
- **THEN** the model fails immediately and returns to source-position, hierarchy, origin, or export correction before any user acceptance evidence is submitted

#### Scenario: Preview-only camera or camera anchor is reviewed
- **WHEN** a camera or `CameraAnchor` exists only to frame screenshots, videos, or look-development evidence and is not declared as a runtime gameplay, camera-system, program-interface, or deterministic-capture dependency
- **THEN** its exact transform and orientation do not gate model or asset acceptance, while the resulting evidence must still be readable

### Requirement: Modular buildings use the approved grid and spans
The art production workflow SHALL use a `1m` base grid and `2m`, `4m`, and `8m` standard spans for the first modular workshop or processing-station family. Modules MUST align without per-building mesh surgery.

#### Scenario: Two standard modules are assembled
- **WHEN** an artist combines approved wall, floor, roof, or corner modules on the base grid
- **THEN** their dimensions, pivots, seams, and interfaces align at the declared spans

### Requirement: The first module library covers skeleton and functional interfaces
The first production library SHALL include floors, walls, columns, roofs, corners, doors, entrance platforms, pipeline interfaces, operation panels, status lights, and a limited railing set. Decorative interiors and broad prop variety MUST NOT be prerequisites for validating the library.

#### Scenario: Representative workshop is assembled
- **WHEN** the first workshop or processing station is built from the library
- **THEN** it demonstrates enclosure, entrance, functional connection, operation, status, and safety-edge needs without requiring an unplanned unique module

### Requirement: Terrain adaptation preserves the building baseline
Building bodies SHALL retain a horizontal authored baseline and SHALL adapt to Terrain through adjustable feet, steps, ramps, skirts, or filler modules. The workflow MUST NOT require uniquely sculpting Terrain or the main building mesh for every placement.

#### Scenario: Workshop is placed on uneven terrain
- **WHEN** the representative workshop is evaluated on an approved uneven site
- **THEN** adaptation pieces resolve visible gaps and access while the main modular body remains unchanged

### Requirement: Materials follow a shared hybrid production route
Structural and functional parts SHALL use separate primary Trim Sheet responsibilities plus base tiling materials, vertex color, and controlled local decals or masks. A new secondary sheet or unique mask MUST be justified by repeatable visual benefit and MUST NOT replace shared material rules by default.

#### Scenario: Functional facade needs hero detail
- **WHEN** an entrance, panel, interface, or high-value facade requires additional detail
- **THEN** the artist first uses the shared structural or functional material system and adds only a scoped decal, mask, or justified secondary sheet

### Requirement: Complete assets default to one Unity main material with functional maps
Each complete modular asset SHALL default to one Unity main material and MAY use BaseColor, Normal, Metallic/Smoothness or packed Mask, AO, and Emission Mask textures. Status lights, screens, and energy windows SHOULD use regions in the same material's Emission Mask, with color and intensity controlled by Unity parameters or program state. A separate material SHALL be allowed only for transparent or semi-transparent surfaces or a genuinely independent dynamic asset, and every exception MUST record its responsibility and rationale. Shared Trim Sheets and material templates MAY be reused across many assets; the rule constrains per-asset material responsibilities and does not require unique material copies. Blender SHALL own UVs, material-region authoring, and texture baking, while Unity SHALL own the final Shader and runtime feedback.

#### Scenario: Modular asset is prepared for Unity
- **WHEN** its material delivery is reviewed
- **THEN** it has one main material by default, a declared functional-map set, Emission Mask semantics where needed, and documented justification for every transparent or independent-dynamic material exception

### Requirement: Asset tiers constrain texture and material cost
Assets SHALL be classified as hero, standard, or background. Initial standalone texture targets SHALL be `2K`, `1K`, and `512` respectively when shared materials are insufficient. Asset tier MUST NOT by itself authorize extra material slots; the one-main-material default and its transparent or independent-dynamic exceptions SHALL apply to every tier.

#### Scenario: Asset is prepared for review
- **WHEN** a representative asset is submitted
- **THEN** its tier, texture sources, material slots, exception rationale, and expected scene density are recorded

### Requirement: Detail and wear communicate function
High detail SHALL concentrate at entrances, panels, interfaces, status lights, primary facades, joints, and frequent-contact edges. Wear, dust, heat marks, and maintenance labels MUST follow plausible functional locations while large surfaces preserve clean silhouettes and color blocks.

#### Scenario: Workshop surface treatment is reviewed
- **WHEN** the asset is viewed from near, middle, and far gameplay distances
- **THEN** functional areas remain readable and broad surfaces are not covered by uniform random dirt or undirected micro-detail

### Requirement: Stylized readability does not depend on a global outline
Assets SHALL use silhouette, bevel highlights, material partitioning, entrance framing, functional color, and decals for stylized readability. A global comic outline MUST NOT be required for the production route.

#### Scenario: Representative asset is shown in LookDev
- **WHEN** the outline effect is absent
- **THEN** the asset retains readable structure, function, orientation, and hierarchy from the gameplay camera

### Requirement: LOD responsibility follows asset tier
Hero assets SHALL normally provide `LOD0/LOD1/LOD2`, standard modules SHALL normally provide `LOD0/LOD1`, and background assets SHALL be authored lightweight unless evidence requires additional LODs. Distant states MUST remove insignificant parts, decals, secondary emission, and shadow duties together with geometry reduction.

#### Scenario: Asset transitions to a distant representation
- **WHEN** the representative camera crosses an approved LOD threshold
- **THEN** silhouette and functional identity remain stable while small geometry and unnecessary rendering duties are removed without visible errors

### Requirement: Production budgets remain evidence-driven
The sample SHALL record triangles, Renderers, draw calls, SetPass calls, shadow duties, and CPU/GPU frame timing under declared conditions. Initial limits SHALL be treated as adjustable gates until a representative area with realistic density is profiled before mass production.

#### Scenario: Temporary budget gate is reviewed
- **WHEN** the representative workshop and environment are profiled
- **THEN** the evidence records conditions, observed values, pass or watch status, bottlenecks, and the trigger for revising the gate
