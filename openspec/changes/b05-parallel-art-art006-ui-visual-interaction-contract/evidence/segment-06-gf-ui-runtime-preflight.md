# Segment 06 — GF UI runtime integration preflight

Date: 2026-09-04

## Visual entry points

- `Assets/Game/Prefabs/UI/ART006_UI/BaseCommandHubForm_VisualFinal_V04.prefab`
- `Assets/Game/Prefabs/UI/ART006_UI/FieldHudForm_VisualFinal_V04.prefab`

The two V04 assets remain the visual entry points. Their V04 → V03 → V02 → V01
variant chains and the two StructurePrototype parents are not unpacked or replaced.

## Implemented, compilation-safe runtime bridge

- `AutoEraUiFormBase` provides GF `UIFormBase` lifecycle forwarding, request-version
  invalidation on close/recycle, and an intent-only entry point. It does not poll
  input devices or own a second input system.
- `BaseCommandHubForm` keeps the five frozen hub pages in one form and applies page
  visibility plus active navigation state from a single page selection source.
- `FieldHudForm` applies operation presentation bindings without owning the
  operation authority.
- `AutoEraUiOperationSnapshot` and `AutoEraUiOperationPresentation` make terminal
  state, trusted progress, long-wait hint, cancellation, retry and details affordance
  depend only on authoritative state. Client elapsed time cannot infer failure or a
  lost request.

## Verification

- `asset_refresh` completed successfully after the bridge scripts were added.
- `script_get_compile_feedback` for `BaseCommandHubForm.cs`: `isCompiling=false`,
  `hasErrors=false`, `errorCount=0`.
- `debug_get_errors`: `count=0`.
- QA / rapid-executor job `16a7f942`:
  `AutoEraUiOperationContractsEditModeTests`, 4/4 passed, 0 failed, 0 skipped,
  0 inconclusive. Covered authority state, trusted progress, stale request-version
  rejection and five-page circular selection.
- QA / rapid-executor job `973a2e4f`:
  `AutoEraUiOperationContractsEditModeTests`, 6/6 passed, 0 failed, 0 skipped,
  0 inconclusive. In addition to the bridge coverage, this verifies the fixed
  confirmation layout (missing description resource disables `Danger`, initial
  focus remains Cancel) and the 1.2-second hold-to-confirm release gate. The
  earlier pre-fix test assembly result is deliberately not used as evidence.

## Deliberately pending integration

No ART-006 `UIViews`, `UITable`, `UIGroupTable`, or project `InputModule` entry is
currently available in the user-controlled GameData source. Therefore this segment
does not mark task 3.2 (nor 3.3–3.5) complete: the forms cannot yet be registered,
opened through GF, bound to the V04 hierarchy, or routed from the framework input
abstraction. No xlsx was modified.
