## ADDED Requirements

### Requirement: The pilot is predominantly AI-operated and reproducible
The G06 pilot SHALL use a versioned, non-interactive pipeline in which AI performs at least 90 percent of recipe authoring, graph generation or parameterization, texture generation, validation, Unity assembly, rendering, and iteration steps. The pipeline MUST record tool version, recipe, random seed, source fingerprints, output fingerprints, and manual actions required for each accepted candidate.

#### Scenario: An accepted candidate is regenerated
- **WHEN** the same approved source inputs, recipe, seed, and tool version are processed again
- **THEN** the pipeline produces equivalent texture content and a manifest that identifies every input and output without requiring routine GUI interaction

#### Scenario: Automation ratio is reviewed
- **WHEN** a G06 candidate reaches user review
- **THEN** its manifest shows that manual production actions other than user review and exceptional recovery account for no more than ten percent of recorded production steps

### Requirement: Material Maker is driven through a constrained recipe contract
The pipeline MUST represent project-facing material intent as schema-validated recipe JSON and a versioned Material Maker template. AI MUST change declared parameters, masks, seeds, and approved graph modules through the recipe or a reviewable template update; routine production MUST NOT depend on mouse-coordinate macros or an unrecorded interactive node graph.

#### Scenario: AI requests a new G06 material iteration
- **WHEN** AI changes wear strength, panel color, micro-surface scale, or emission treatment
- **THEN** the change is captured in a versioned recipe or template delta and can be batch processed without manual node rewiring

### Requirement: Blender inputs preserve the approved G06 structure contract
The pilot MUST preserve the approved G06 silhouette, geometry hierarchy, Pivots, joints, sockets, anchors, V17/V18 sources, and RuntimePreview. Blender processing SHALL provide a unique non-overlapping UV0 atlas and stable semantic or functional masks for every visible surface assigned to the single main material.

#### Scenario: G06 material inputs are prepared
- **WHEN** the Blender preparation stage completes
- **THEN** unintended UV overlap is zero, each emission-enabled texel belongs only to an approved state region, and no approved structural transform or anchor has changed

#### Scenario: A structural defect is discovered during texturing
- **WHEN** a visible gap, unsupported floating part, wrong Pivot, or incorrect anchor is detected
- **THEN** the pipeline reports the structural defect separately and does not conceal it with texture, normal, parallax, or emissive effects

### Requirement: The generated texture set follows the Unity URP channel contract
The accepted G06 candidate MUST provide BaseColor, Normal, Metallic with Smoothness in alpha, AO, and Emission Mask textures plus a machine-readable manifest. BaseColor SHALL be imported as sRGB; Normal SHALL use Unity Normal Map import; Metallic/Smoothness, AO, and Emission Mask SHALL be imported as linear data. Emission MUST be zero outside approved state or functional regions.

#### Scenario: Generated textures are assembled in Unity
- **WHEN** the batch output is imported into the G06 LookDev validation scene
- **THEN** one Unity main material references all required maps with the declared color-space and channel settings and no unrelated renderer receives emission

### Requirement: Material styling conforms to the approved G06 visual baseline
The final G06 material SHALL retain the bright stylized engineering direction of the approved `G06-formal-carrier-axonometric-v2-simplified.png`: warm light body surfaces, dark blue-gray structure, controlled orange or cyan-green functional accents, readable panel and seam hierarchy, and restrained function-driven wear. It MUST NOT rely on photoreal micro-noise, broad random grime, uncontrolled colored emission, or a uniformly plastic response.

#### Scenario: The candidate is reviewed at fixed LookDev distances
- **WHEN** the G06 candidate is viewed in the frozen neutral LookDev at overview and close-up distances
- **THEN** functional zones remain readable, surface response is coherent with the approved stylized baseline, wear is concentrated at plausible use locations, and no texture artifact dominates the silhouette

### Requirement: Automated validation precedes visual review
Every candidate MUST pass automated checks for output completeness, deterministic fingerprints, UV and semantic overlap, mask bounds, channel ranges, Unity importer settings, single-main-material binding, missing references, and Unity Console errors before main-art visual review. A normal failed check SHALL trigger diagnosis and another safe iteration without pausing for user or producer acknowledgement.

#### Scenario: Emission leaks into a body panel
- **WHEN** automated mask analysis or the fixed render detects non-zero emission outside approved regions
- **THEN** the candidate is rejected internally, the responsible UV or mask stage is corrected, and no user-review render is submitted

### Requirement: User acceptance requires two controlled renders
The final review package MUST contain at least two PNG renders at 1920×1080 or greater from the frozen neutral LookDev: one axonometric overview containing the complete G06 model and one close-up showing representative body, structural, seam, edge, state, and wear details. Main-art self-approval SHALL NOT replace explicit user visual acceptance.

#### Scenario: A candidate reaches final user review
- **WHEN** all technical checks and main-art visual checks pass
- **THEN** the user receives the axonometric overview and close-up together with a concise statement of what changed and can approve or request another iteration

#### Scenario: The user rejects the candidate
- **WHEN** the user identifies a visual defect or mismatch in either final render
- **THEN** the feedback becomes a new recorded iteration, the AI pipeline continues from the appropriate recipe, mask, or import stage, and the pilot remains incomplete

### Requirement: Pilot outputs remain isolated until acceptance
The pilot MUST use G06-specific source branches, output directories, material assets, and LookDev instances. It MUST NOT overwrite approved whitebox sources, old failure evidence, production gameplay content, or the user-owned task workbook. Passing the G06 pilot SHALL NOT automatically migrate other assets.

#### Scenario: The pilot is rolled back
- **WHEN** the toolchain or visual result is abandoned
- **THEN** pilot-specific assets can be disabled or removed while the approved G06 whitebox, V17/V18 structure, RuntimePreview, and historical V19 evidence remain intact
