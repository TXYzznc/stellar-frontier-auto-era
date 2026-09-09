## MODIFIED Requirements

### Requirement: Machine blockouts are motion-ready
Any movable formal model produced after the b06 prototype gate SHALL use the approved product dimensions and SHALL preserve the versioned MotionRig joint, local-axis, limit, safe-pose, socket, work-point, VFX-point and clearance contract. The authoritative high-poly source MUST separate visible rigid parts by owning joint and MUST generate a lightweight contract proxy for Unity regression. Visual joints MUST NOT redefine the logical root, navigation authority, gameplay collision authority or stable MotionGraph binding. High-poly approval MUST NOT be represented as completion of materials, low-poly topology, final baking, LOD or runtime delivery.

#### Scenario: A formal high-poly machine is checked before material production
- **WHEN** its generated contract proxy replaces the b06 visual cubes and the relevant motion suite is executed
- **THEN** every movable part resolves by stable ID, reaches its declared range, returns to bind or safe pose without drift, preserves all required anchors, and the full high-poly source remains an ArtResource-only staged deliverable

#### Scenario: A high-poly source contains unsupported visible geometry
- **WHEN** structural inspection finds a visible object without a declared physical support or intentional suspension requirement
- **THEN** the asset fails publication until the source-level position or support structure is corrected
