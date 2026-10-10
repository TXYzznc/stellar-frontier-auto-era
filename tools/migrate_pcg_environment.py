"""Copy NEW accepted environment assets, preserving GUIDs. Never overwrite project assets."""
import argparse
import hashlib
import json
import re
import shutil
from pathlib import Path

PREVIEW = {"PCGStreamPreview", "PCGSurfacePreview", "PCGPreviewMover", "PCGPreviewInputGateway"}

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--target", type=Path, required=True)
    parser.add_argument("--manifest", type=Path, required=True)
    args = parser.parse_args()
    source, target = args.source.resolve(), args.target.resolve()
    def index(root):
        result = {}
        for meta in (root / "Assets").rglob("*.meta"):
            match = re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8-sig"), re.M)
            if match:
                result[match[1]] = Path(str(meta)[:-5])
        return result
    origin, existing = index(source), index(target)
    todo = [source / "Assets/Art/Environment/PCGSurfaceResponse/ResponseWorld.asset",
            source / "Assets/Art/Environment/PCGSurfaceResponse/SurfaceSettings.asset"]
    todo += [p for p in (source / "Assets/Game/Scripts/AutoEra/Art/Streaming").glob("*.cs") if p.stem not in PREVIEW]
    todo += list((source / "Assets/Game/Shaders/PCG").glob("*.hlsl"))
    seen = set()
    while todo:
        path = todo.pop()
        if path in seen or not path.is_file():
            continue
        if path.suffix == ".cs" and path.stem in PREVIEW:
            raise RuntimeError(f"Preview dependency: {path}")
        seen.add(path)
        if path.suffix.lower() in {".asset", ".mat", ".terrainlayer", ".prefab", ".shader", ".cs", ".hlsl"}:
            data = path.read_bytes().decode("utf-8-sig", errors="ignore")
            for guid in re.findall(r"guid: ([0-9a-f]{32})", data):
                if guid in origin:
                    todo.append(origin[guid])
            for include in re.findall(r'#include "([^\"]+)"', data):
                local = path.parent / include
                if local.is_file():
                    todo.append(local)
    records, plan = [], []
    def destination(path):
        ext = path.suffix.lower()
        if ext == ".cs": folder = "Scripts/AutoEra/PCG/Environment"
        elif ext in {".shader", ".hlsl"}: folder = "Shaders/PCG/Environment"
        elif ext == ".mat": folder = "Materials/Environment"
        elif ext in {".png", ".jpg", ".tga", ".exr", ".psd", ".tif"}: folder = "Textures/Environment"
        elif ext == ".terrainlayer": folder = "ScriptableAssets/Environment/TerrainLayers"
        elif ext == ".asset" and path.name in {"ResponseWorld.asset", "SurfaceSettings.asset"}: folder = "ScriptableAssets/Environment"
        elif ext in {".asset", ".fbx", ".obj"}: folder = "Models/Environment"
        else: raise RuntimeError(f"Unclassified dependency: {path}")
        name = {"ResponseWorld.asset": "EnvironmentWorld.asset", "SurfaceSettings.asset": "EnvironmentSurface.asset"}.get(path.name, path.name)
        return target / "Assets/Game" / folder / name
    used = {}
    for path in sorted(seen):
        meta = Path(str(path) + ".meta")
        guid = re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8-sig"), re.M)[1]
        reuse = guid in existing
        output = existing[guid] if reuse else destination(path)
        if reuse and digest(path) != digest(output):
            raise RuntimeError(f"GUID conflict with different contents: {path} -> {output}")
        if output in used and used[output] != guid:
            output = output.with_name(f"{output.stem}_{guid[:8]}{output.suffix}")
        if not reuse and output.exists():
            raise RuntimeError(f"Path conflict: {output}")
        used[output] = guid
        plan.append((path, meta, output, reuse))
        records.append(dict(source=path.relative_to(source).as_posix(), target=output.relative_to(target).as_posix(), guid=guid, sha256=digest(path), reused=reuse))
    for path, meta, output, reuse in plan:
        if reuse: continue
        output.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, output)
        shutil.copy2(meta, Path(str(output) + ".meta"))
    args.manifest.parent.mkdir(parents=True, exist_ok=True)
    args.manifest.write_text(json.dumps(dict(assets=records, count=len(records)), ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"PCG migrated {len(records)} assets, reused {sum(r['reused'] for r in records)} identical existing GUIDs")

if __name__ == "__main__":
    main()
