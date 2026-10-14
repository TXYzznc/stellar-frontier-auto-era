"""Project-scoped client for the installed Codely Unity MCP stdio bridge."""
import argparse
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

def request(method, params):
    process = subprocess.Popen(
        ['codely', 'serve', 'unity-mcp', '--stdio', '--unity-project-path', str(ROOT)],
        stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
        text=True, encoding='utf-8', creationflags=subprocess.CREATE_NO_WINDOW,
    )
    def send(payload):
        process.stdin.write(json.dumps(payload, ensure_ascii=False) + '\n')
        process.stdin.flush()
    def receive(identity):
        while True:
            line = process.stdout.readline()
            if not line:
                raise RuntimeError('Unity MCP ended before replying')
            try:
                value = json.loads(line)
            except ValueError:
                continue
            if value.get('id') == identity:
                return value
    try:
        send({'jsonrpc':'2.0','id':1,'method':'initialize','params':{'protocolVersion':'2024-11-05','capabilities':{},'clientInfo':{'name':'formal-ui-integration','version':'1'}}})
        receive(1)
        send({'jsonrpc':'2.0','method':'notifications/initialized'})
        send({'jsonrpc':'2.0','id':2,'method':method,'params':params})
        return receive(2)
    finally:
        process.terminate()
        process.wait(timeout=5)

if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    parser=argparse.ArgumentParser()
    parser.add_argument('tool', nargs='?', default='list')
    parser.add_argument('--args-file')
    parser.add_argument('--script-file')
    parser.add_argument('--output')
    options=parser.parse_args()
    params=json.loads(Path(options.args_file).read_text(encoding='utf-8-sig')) if options.args_file else {}
    operation=options.tool
    if operation in ('import-ui','apply-ui','capture-ui','verify-ui'):
        scripts={
            'import-ui':'return AutoEra.Editor.UiProto.FormalUiArtIntegration.ImportAll();',
            'apply-ui':'System.Collections.IEnumerator ApplyFormalUi(){yield return AutoEra.Editor.UiProto.FormalUiArtIntegration.ImportAll();for(int family=1;family<=18;family++){yield return AutoEra.Editor.UiProto.FormalUiArtIntegration.ApplyFamily(family);yield return AutoEra.Editor.UiProto.FormalUiArtIntegration.CaptureFamily(family);}} return ApplyFormalUi();',
            'capture-ui':'System.Collections.IEnumerator CaptureFormalUi(){for(int family=1;family<=18;family++)yield return AutoEra.Editor.UiProto.FormalUiArtIntegration.CaptureFamily(family);} return CaptureFormalUi();',
            'verify-ui':'var contracts=AutoEra.Editor.UiProto.AutoEraContractGate1Checker.CheckAll();var fonts=AutoEra.Editor.UiProto.AutoEraUiFontTool.Check();return new {contracts,fonts};',
        }
        options.tool='exec_editor_script'
        params={'script':scripts[operation],'summary':operation,'capture_logs':True,'timeoutSeconds':600}
    if options.script_file:
        params={'script':Path(options.script_file).read_text(encoding='utf-8-sig'),'summary':'正式 UI 工程接入与验证','capture_logs':True,'timeoutSeconds':600}
    result=request('tools/list', {}) if options.tool=='list' else request('tools/call', {'name':options.tool,'arguments':params})
    output=json.dumps(result,ensure_ascii=False,indent=2)
    if options.output:
        Path(options.output).write_text(output,encoding='utf-8')
        print(options.output)
    else:
        print(output)
    failed='error' in result or result.get('result',{}).get('isError',False)
    for content in result.get('result',{}).get('content',[]):
        if content.get('type')=='text':
            try:
                value=json.loads(content['text'])
                failed=failed or value.get('success') is False or value.get('hasErrors') is True
            except (ValueError,AttributeError):
                pass
    sys.exit(1 if failed else 0)
