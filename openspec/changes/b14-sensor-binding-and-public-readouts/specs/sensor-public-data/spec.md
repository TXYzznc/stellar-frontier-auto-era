## ADDED Requirements

### Requirement: Typed read-only providers
Sensors SHALL expose only declared public immutable data, with explicit unavailable reasons, and never infer player thresholds or decisions.

#### Scenario: Land cells
- **WHEN** soil and crop providers expose the same land
- **THEN** stable cell IDs align moisture and growth data, and unplanted soil remains readable without averaging away cells

#### Scenario: Missing production
- **WHEN** agriculture or forestry providers do not exist
- **THEN** runtime returns unavailable rather than fixture data, and tests do not certify production completeness

### Requirement: Integration evidence
The implementation SHALL separately verify shared machine lifecycle, real region basic data and fixture-only structured data, recording formal configuration readiness honestly.

#### Scenario: Shutdown
- **WHEN** the owner releases the sensor set
- **THEN** no subscriptions, compute leases or valid old readouts survive
