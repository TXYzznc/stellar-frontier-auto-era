## ADDED Requirements

### Requirement: Demo uses accepted replacement assets only
The acceptance demo SHALL use only replacement machine, workshop, door, conveyor, arm, support, and effector assets that have passed the project's pre-modeling visual gates and final model acceptance. Deprecated models and their visual evidence MUST NOT be used as demo input.

#### Scenario: Demo implementation begins
- **WHEN** the implementation window resolves scene asset references
- **THEN** every referenced 3D asset has recorded axonometric approval, production-document approval, modeling-route approval, and final model acceptance

### Requirement: Demo plays seven fixed acceptance segments
The demo SHALL present workshop overview, sliding door, conveyor, wheel grounding, steering, support-and-arm work, and effector exchange/interruption in that fixed order. Each segment SHALL last between five and eight seconds during default automatic playback.

#### Scenario: Automatic playback completes a cycle
- **WHEN** the user starts the demo without manual input
- **THEN** all seven segments play in the approved order with each segment remaining visible for five to eight seconds

### Requirement: User can control playback deterministically
The demo SHALL use `Space` for pause or resume, the left and right arrow keys for previous or next segment, and `R` for reset. Manual segment changes MUST close the current segment cleanly before entering the target segment.

#### Scenario: User resets during a moving mechanism segment
- **WHEN** the user presses `R` while a mechanism is moving
- **THEN** the demo restores its baseline, returns to the first segment, resets progress, and leaves no partial mechanism state

### Requirement: One camera uses three reproducible presets
The demo SHALL use one camera that transitions smoothly among overview, low wheel, and close arm presets. Segment playback MUST select a declared preset without creating competing runtime cameras.

#### Scenario: Demo moves from grounding to arm work
- **WHEN** playback advances from the wheel-grounding segment to the support-and-arm segment
- **THEN** the same camera transitions smoothly from the low wheel preset to the close arm preset and preserves reproducible framing

### Requirement: Acceptance overlay remains development-only
The demo SHALL show current segment name, segment progress, and pause state in a minimal top-right overlay. The overlay MUST NOT become part of the formal HUD or change formal UI navigation.

#### Scenario: Demo is paused
- **WHEN** the user pauses playback
- **THEN** the top-right overlay identifies the current segment and paused state while the segment progress remains stable

### Requirement: Demo restores baseline and preserves authority
The demo SHALL operate on dedicated scene instances and SHALL restore their recorded baseline when reset, stopped, disabled, or when Play Mode exits. It MUST NOT modify gameplay authority, source Prefabs, accepted source assets, or formal scenes.

#### Scenario: Demo is played twice
- **WHEN** the full seven-segment sequence completes, resets, and completes again
- **THEN** transforms, mechanism states, camera framing, and overlay behavior show no cumulative drift and source assets remain unchanged
