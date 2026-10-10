"""Build and run a Unity 2022.3 domain Player benchmark without touching product settings."""
import argparse, hashlib, json, os, shutil, subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCES = [
    'Assets/Game/Scripts/AutoEra/Energy/EnergyGrid.cs',
    'Assets/Game/Scripts/AutoEra/Energy/EnergyContracts.cs',
    'Assets/Game/Scripts/AutoEra/Energy/MachineEnergyConsumer.cs',
    'Assets/Game/Scripts/AutoEra/Energy/FirstVersionEnergyFacilities.cs',
    'Assets/Game/Scripts/AutoEra/Machines/MachineInstance.cs',
    'Assets/Game/Scripts/AutoEra/Machines/MachineDefinition.cs',
    'Assets/Game/Scripts/AutoEra/World/Identity/PersistentId.cs',
    'Assets/Game/Tests/AutoEra/Editor/Support/EnergyGridReference.cs',
    'tools/benchmarks/EnergyBenchmarkRunner.cs',
]

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--unity', required=True)
    parser.add_argument('--label', required=True, choices=['before', 'after'])
    args = parser.parse_args()
    project = ROOT / 'Temp/B46PlayerBenchmark'
    evidence = ROOT / 'openspec/changes/b46-p5006-p5009-p5010-energy-hotpath/evidence'
    (project / 'Assets/Editor').mkdir(parents=True, exist_ok=True)
    (project / 'Packages').mkdir(exist_ok=True)
    (project / 'ProjectSettings').mkdir(exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    hashes = {}
    for name in SOURCES:
        source = ROOT / name
        content = source.read_bytes()
        if args.label == 'before' and source.name == 'EnergyGrid.cs':
            # Reproduce the frozen pre-optimization body, even after production has changed.
            reference = (ROOT / 'Assets/Game/Tests/AutoEra/Editor/Support/EnergyGridReference.cs').read_text(encoding='utf-8')
            current = source.read_text(encoding='utf-8')
            prefix = current[:current.index('    public sealed class EnergyGrid')]
            body = reference[reference.index('    public sealed class EnergyGridReference'):].replace('class EnergyGridReference', 'class EnergyGrid', 1)
            content = (prefix + body).encode('utf-8')
        (project / 'Assets' / source.name).write_bytes(content)
        hashes[name] = hashlib.sha256(content).hexdigest()
    shutil.copyfile(ROOT / 'tools/benchmarks/EnergyBenchmarkBuild.cs', project / 'Assets/Editor/EnergyBenchmarkBuild.cs')
    (project / 'Packages/manifest.json').write_text(json.dumps({'dependencies': {'com.unity.modules.jsonserialize': '1.0.0'}}), encoding='utf-8')
    version = (ROOT / 'ProjectSettings/ProjectVersion.txt').read_text(encoding='utf-8')
    (project / 'ProjectSettings/ProjectVersion.txt').write_text(version, encoding='utf-8')
    output = evidence / ('energy-player-' + args.label + '.json')
    buildlog = project / ('build-' + args.label + '.log')
    flags = subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0
    print('Building isolated Development Player:', args.label, flush=True)
    build = subprocess.run([args.unity, '-batchmode', '-quit', '-projectPath', str(project),
                            '-executeMethod', 'AutoEra.Tests.EnergyBench.EnergyBenchmarkBuild.Build', '-logFile', str(buildlog)], creationflags=flags)
    shutil.copyfile(buildlog, evidence / ('player-build-' + args.label + '.log'))
    if build.returncode:
        raise SystemExit('Unity build failed; see ' + str(buildlog))
    shutil.copyfile(project / 'Build/build-summary.json', evidence / ('player-build-' + args.label + '.json'))
    (evidence / ('player-source-' + args.label + '.json')).write_text(json.dumps(hashes, indent=2), encoding='utf-8')
    print('Running benchmark:', args.label, flush=True)
    run = subprocess.run([str(project / 'Build/EnergyBenchmark.exe'), '-batchmode', '-nographics',
                          '-benchmarkOutput', str(output), '-logFile', str(project / ('player-' + args.label + '.log'))], creationflags=flags)
    if run.returncode or not output.exists():
        raise SystemExit('Player failed; see ' + str(project / ('player-' + args.label + '.log')))
    data = json.loads(output.read_text(encoding='utf-8'))
    if not data['development'] or len(data['measurements']) != 54 or data.get('allocationProbeSamples', 0) < 1:
        raise SystemExit('Incomplete or non-Development Player benchmark')
    print(json.dumps({'unity': data['unity'], 'cpu': data['cpu'], 'measurements': len(data['measurements']), 'output': str(output)}, ensure_ascii=False), flush=True)

if __name__ == '__main__':
    main()
