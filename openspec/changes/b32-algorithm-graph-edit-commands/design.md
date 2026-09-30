## Context

`AlgorithmDocument` stores nodes and edges and `AlgorithmValidator` already owns structural validation. `AlgorithmInstanceService` owns draft revisions and emits change notifications. Existing UI read models may only mutate a selected draft through focused commands.

## Decisions

### Focused commands instead of full-document replacement

Add `CreateNode`, `Connect` and `Disconnect` as focused mutations. Every mutation copies the draft, checks the expected revision, rejects while an application is publishing, increments revision exactly once on success, writes the copy back and raises `Changed`.

### Stable identity allocation

Only `AlgorithmInstanceService` owns `PersistentIdAllocator`; callers select a node kind and layout coordinate but never manufacture a stable node id. `CreateNode` allocates the id and returns it via `out ulong`.

### Safe default nodes

Creation uses a minimal closed default node per kind. Value-producing editable nodes receive a finite default value of their declared type; all other fields retain existing document defaults. The graph can still be structurally incomplete after creation, which is represented by the existing validation issues rather than hidden auto-wiring.

### Connection is validated before mutation

`Connect` resolves the source node/output and target node/input from `AlgorithmCatalog`. It rejects missing/deleted nodes, missing ports, incompatible types (including units/capabilities), and an already connected target input. It does not rely on a later compile failure to protect the graph.

### Disconnect uses the full edge identity

`Disconnect` removes exactly one `(from, output, to, input)` edge. It does not delete a node, a port, or unrelated fan-out edges.

## Alternatives considered

- **Expose `Edit` and let UI rewrite the entire document**: rejected because stale revisions, stable IDs and validation would be easy to bypass.
- **Let UI allocate node ids**: rejected because identity allocation belongs to the service and must remain collision-free.
- **Auto-connect compatible ports after node creation**: rejected because the project prohibits hidden graph behavior.

## Risks

Creation defaults can leave required ports disconnected; this is intentional and surfaces through existing issue rows. A later UI batch will select node kinds and invoke these commands; it must not silently create nodes on a click without an explicit choice.
