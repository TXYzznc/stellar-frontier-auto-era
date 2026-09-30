## ADDED Requirements

### Requirement: Explicit report exits close the form
Report forms with `EscapeClose=false` SHALL close through their explicit Return/Close button handlers without relying on input-module Escape behavior.

#### Scenario: Progress report returns to the source form
- **WHEN** the player clicks ProgressReportForm's Return button
- **THEN** the report form closes and the source HUD resumes

### Requirement: Ordinary forms expose one primary exit
Ordinary full-screen forms SHALL expose one primary Return action unless a documented multi-step flow requires a distinct abort action.

#### Scenario: Ordinary form has one primary exit
- **WHEN** the form is a normal read/edit page without a separate abort step
- **THEN** its contract exposes one Return action and no duplicate Close action

### Requirement: Algorithm editing is a complete draft workflow
The algorithm editor SHALL support creating a draft from a template, editing nodes and typed edges, editing bindings and public parameters, validating and applying the draft, and reading recent execution diagnostics.

#### Scenario: Player edits and applies a template instance
- **WHEN** a player instantiates a template, adds or connects nodes, binds endpoints, changes a public parameter, and applies
- **THEN** the draft validates, the machine receives the new revision at a safe point, and the result is visible in the editor

### Requirement: HUD details are independently loadable
Field HUD resident status and world-object detail panels SHALL be independently loadable UIForm prefabs with session propagation and input restoration on close.

#### Scenario: World object detail is opened from the resident HUD
- **WHEN** the player selects a machine or resource object and opens its detail panel
- **THEN** a child UIForm loads with the same session and closes without destroying the resident HUD

### Requirement: UI closure restores world interaction
After any HUD or detail form is closed, the input router SHALL permit camera movement, WASD control, object selection and world interaction according to the remaining visible forms.

#### Scenario: Closing a detail panel restores world controls
- **WHEN** the player closes a HUD child form and no blocking management form remains
- **THEN** camera, WASD, selection, and world interaction are accepted
