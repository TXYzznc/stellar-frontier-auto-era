"""Reproduce B51 JSON contracts from the saved baseline; Unity applies them in-place."""
import copy
import hashlib
import json
from pathlib import Path

CHANGE = Path(__file__).resolve().parents[1]
ROOT = CHANGE.parents[2]
DEST = ROOT / 'Docs/Development/UI-PrefabLayouts'
FORMS = ['MachineLibraryForm', 'FieldHudResidentForm', 'FieldHudMachineOverviewForm', 'BaseCommandHubForm']


def index(node):
    result = {node['name']: node}
    for child in node.get('children', []):
        result.update(index(child))
    return result


def rect(n, size, pos=(0, 0), amin=(0, 1), amax=None, pivot=None):
    n.update(sizeDelta=list(size), anchoredPosition=list(pos), anchorMin=list(amin),
             anchorMax=list(amax if amax is not None else amin), pivot=list(pivot if pivot is not None else amin))


def component(n, kind, **values):
    c = next((c for c in n.setdefault('components', []) if c['type'] == kind), None)
    if c is None:
        c = {'type': kind}
        n['components'].append(c)
    c.update(values)
    return c


def node(name, size, pos=(0, 0), text=None):
    n = {'name': name, 'components': [], 'children': []}
    rect(n, size, pos)
    if text is not None:
        component(n, 'TextMeshProUGUI', text=text, fontSize=22, color=[.94,.95,.96,1],
                  fontPath='Assets/Game/Fonts/UI/SIMHEI SDF.asset', raycastTarget=False, wordWrap=True)
    return n


def text(n, value):
    component(n, 'TextMeshProUGUI', text=value)


def panel(nodes, key, size, pos, body=None):
    p = nodes['Panel_' + key]
    rect(p, size, pos)
    component(p,'Image',color=[.086,.102,.122,1],raycastTarget=True)
    heading = nodes.get('Txt_' + key + 'Heading')
    if heading:
        rect(heading, (-24,32), (0,-10), (0,1),(1,1),(.5,1))
    if body is not None and 'Txt_' + key + 'Body' in nodes:
        text(nodes['Txt_' + key + 'Body'], body)
    for n in index(p).values():
        if n['name'].startswith('Item_'):
            n['active'] = False
        if n['name'].startswith('Txt_') and n['name'].endswith('Body'):
            component(n, 'TextMeshProUGUI', fontSize=20)


def placeholder(parent, name, size, pos, label):
    p = node('Panel_' + name, size, pos)
    component(p, 'Image', color=[.085,.10,.12,1], raycastTarget=False)
    p['children'].append(node('Txt_' + name + 'Heading', (size[0]-40,40), (20,-20), label))
    visual = node('Grp_' + name + 'Visual', (size[0]-80,size[1]-190), (40,-90))
    # Deliberately neutral geometry marks the reserved object area, not a finished asset.
    box = node('Img_' + name + 'Placeholder', (180,230))
    rect(box,(180,230),(0,0),(.5,.5))
    component(box,'Image',color=[.18,.21,.24,1],raycastTarget=False,preserveAspect=True)
    visual['children'].append(box)
    p['children'].append(visual)
    caption = node('Txt_' + name + 'Caption',(size[0]-40,64),(20,-size[1]+82),'机器展示占位\n正式资源待制作')
    component(caption,'TextMeshProUGUI',fontSize=20,color=[.68,.71,.75,1])
    p['children'].append(caption)
    parent.setdefault('children',[]).append(p)


def undriven(n):
    kinds=['HorizontalLayoutGroup','VerticalLayoutGroup','GridLayoutGroup']
    n['removeComponents']=kinds
    n['components']=[c for c in n.get('components',[]) if c['type'] not in kinds]


def hud_group(nodes, key, area, rows):
    g=nodes['Grp_'+key]
    undriven(g)
    rect(g,area[0],area[1])
    for name,size,pos in rows:
        rect(nodes[name],size,pos)


def main():
    documents={}
    for name in FORMS:
        source=CHANGE/'evidence'/('baseline/'+name+'.contract.json' if not name.startswith('FieldHud') else name+'.authored.json')
        documents[name]=json.loads(source.read_text(encoding='utf-8-sig'))

    d=documents['MachineLibraryForm']; n=index(d['root']); page=n['Panel_PageMachinePreparation']
    text(n['Txt_MachinePreparationTitle'],'机器整备')
    text(n['Txt_MachinePreparationSellLabel'],'出售载体')
    panel(n,'MachinePreparationCarrier',(300,290),(0,-48),'载体参数与当前能力')
    panel(n,'MachinePreparationReadiness',(300,304),(0,-354),'部署前检查与限制')
    panel(n,'MachinePreparationAssembly',(536,610),(952,-48),'选择槽位，安装或拆卸组件。')
    placeholder(page,'MachinePreparationDisplay',(620,610),(316,-48),'设备展示')
    # Keep original field paths and slot callbacks; only the presentation changes.
    for key in ['Carrier','Readiness','Assembly']:
        width=512 if key=='Assembly' else 276
        height={'Carrier':104,'Readiness':128,'Assembly':76}[key]
        component(n['Content_MachinePreparation'+key],'VerticalLayoutGroup',childControlWidth=True,childControlHeight=True,childForceExpandWidth=True,childForceExpandHeight=False)
        body=n['Txt_MachinePreparation'+key+'Body']
        rect(body,(width,56),(0,0),(0,1),(1,1),(.5,1))
        component(body,'LayoutElement',minHeight=56,preferredHeight=56,preferredWidth=width,flexibleWidth=1)
        for name,x in index(n['Panel_MachinePreparation'+key]).items():
            if name.startswith('Item_'):
                rect(x,(width,height),(0,0),(0,1),(1,1),(.5,1))
                component(x,'LayoutElement',minHeight=height,preferredHeight=height,preferredWidth=width,flexibleWidth=1)
            if name.endswith('RowLabel') or name.endswith('RowValue'):
                component(x,'TextMeshProUGUI',fontSize=20)
            if name.endswith('RowLabel'):
                rect(x,(-24,26),(0,-6),(0,1),(1,1),(.5,1))
            if name.endswith('RowValue'):
                rect(x,(-24,height-42),(0,8),(0,0),(1,0),(.5,0))

    d=documents['BaseCommandHubForm'];n=index(d['root']);page=n['Panel_PageHubOverview']
    text(n['Txt_HubOverviewTitle'],'基地概况')
    panel(n,'HubOverviewEconomy',(880,340),(0,-48),'基地等级、经济与成长摘要尚未接入。')
    panel(n,'HubOverviewOperation',(880,258),(0,-404),'运行对象与能源详情可从下方入口查看。')
    panel(n,'HubOverviewAttention',(592,614),(896,-48),'待办与日常补给摘要尚未接入。')
    for key in ['Economy','Operation','Attention']:
        rect(n['List_HubOverview'+key],(-32,-120),(0,14),(0,0),(1,1),(.5,.5))
    actions=n['Grp_HubOverviewActions'];undriven(actions)
    actions['lastSibling']=True
    page['children'].remove(actions)
    page['children'].append(actions)
    rect(actions,(0,0),(0,0),(0,0),(1,1),(.5,.5))
    for key,x,y in [('Objects',24,-594),('Energy',220,-594),('Stats',24,-320),('Tasks',920,-542),('Supply',920,-598)]:
        rect(n['Btn_HubOverview'+key],(184,48),(x,y))

    d=documents['FieldHudResidentForm'];n=index(d['root'])
    d.pop('runtimeGeneratedPageRoots',None)
    d['formArrays']['_pageRoots']=[{'node':'FieldHudResidentForm/Grp_PageHost/Panel_PageHud'+k,'kind':'GameObject'} for k in ['Status','Tracker','Alerts','Navigation','Save']]
    # Page roots remain discovered from the pre-authored host by the existing bridge.
    rect(n['FieldHudResidentForm'],(0,0),(0,0),(0,0),(1,1),(.5,.5))
    for x in n.values():
        if x['name'].startswith('Panel_Page'):x['active']=True
    rect(n['Panel_PageHudStatus'],(-48,80),(0,-20),(0,1),(1,1),(.5,1))
    rect(n['Panel_PageHudTracker'],(344,196),(24,-116))
    rect(n['Panel_PageHudAlerts'],(480,108),(-100,-116),(.5,1))
    rect(n['Panel_PageHudNavigation'],(1280,72),(24,24),(0,0))
    rect(n['Panel_PageHudSave'],(320,96),(24,112),(0,0))
    hud_group(n,'HudStatusContent',((800,56),(20,-12)),[
        ('Txt_HudStatusResources',(380,52),(0,0)),('Txt_HudStatusSystems',(400,52),(400,0))])
    # Keep the four status shortcuts left of the persistent machine side panel (starts at ~1440px).
    hud_group(n,'HudStatusActions',((576,48),(840,-16)),[(f'Btn_HudStatus{k}',(136,48),(i*144,0)) for i,k in enumerate(['Growth','Compute','Energy','Resource'])])
    hud_group(n,'HudTrackerContent',((312,112),(16,-12)),[
        ('Txt_HudTrackerObjective',(312,48),(0,0)),('Txt_HudTrackerGuide',(312,52),(0,-54))])
    hud_group(n,'HudTrackerActions',((312,44),(16,-140)),[(f'Btn_HudTracker{k}',(96,44),(i*108,0)) for i,k in enumerate(['Task','Guide','Locate'])])
    hud_group(n,'HudAlertsContent',((280,84),(16,-12)),[
        ('Txt_HudAlertsHighest',(280,36),(0,0)),('Txt_HudAlertsChanges',(280,36),(0,-44))])
    hud_group(n,'HudAlertsActions',((144,84),(320,-12)),[
        ('Btn_HudAlertsOpen',(144,36),(0,0)),('Btn_HudAlertsLocate',(144,36),(0,-44))])
    nav=n['Grp_HudNavigationActions'];undriven(nav);rect(nav,(1248,48),(16,-12))
    for i,k in enumerate(['Hub','Machines','Build','Components','Shop','Tasks','System']):
        rect(n['Btn_HudNavigation'+k],(168,48),(i*180,0))
    # Save is a compact status module, not a permanent large panel.
    undriven(n['Grp_HudSaveContent']);rect(n['Grp_HudSaveContent'],(192,72),(16,-12))
    rect(n['Txt_HudSaveSave'],(192,72),(0,0))
    undriven(n['Grp_HudSaveActions']);rect(n['Grp_HudSaveActions'],(88,48),(216,-24))
    rect(n['Btn_HudSaveDetails'],(88,48),(0,0))
    text(n['Txt_HudStatusResources'],'资源总览\n数据尚未接入')
    text(n['Txt_HudStatusSystems'],'系统状态\n请从中枢查看详情')
    text(n['Txt_HudTrackerObjective'],'当前目标\n任务追踪尚未接入')
    text(n['Txt_HudTrackerGuide'],'引导信息待接入')
    text(n['Txt_HudAlertsHighest'],'警报摘要尚未接入')
    text(n['Txt_HudAlertsChanges'],'状态变化待接入')
    text(n['Txt_HudSaveSave'],'保存状态\n等待正式流程')
    for key,label in [('HudTrackerTask','任务'),('HudTrackerGuide','引导'),('HudTrackerLocate','定位'),('HudAlertsOpen','警报列表'),('HudAlertsLocate','定位来源'),('HudSaveDetails','详情')]:
        text(n['Txt_'+key+'Label'],label)
    # Keep the prototype action strip scannable at 1920x1080; final iconography is a later art pass.
    text(n['Txt_HudStatusGrowthLabel'],'成长')
    text(n['Txt_HudStatusComputeLabel'],'算力对象')
    text(n['Txt_HudStatusEnergyLabel'],'能源')
    text(n['Txt_HudStatusResourceLabel'],'资源详情')
    # Existing working routes only; do not suggest unsupported buttons are operational.
    for name,x in n.items():
        if name.startswith('Btn_'):component(x,'Button',interactable=name in ['Btn_HudNavigationHub','Btn_HudNavigationMachines'])

    d=documents['FieldHudMachineOverviewForm'];n=index(d['root'])
    rect(n['Panel_PageMachineOverview'],(456,832),(-24,-116),(1,1))
    text(n['Txt_MachineOverviewTitle'],'现场机器')
    rect(n['Txt_MachineOverviewTitle'],(340,40),(16,-12))
    component(n['Panel_PageMachineOverview'],'Image',color=[.12549,.141176,.164706,1],raycastTarget=True)
    panel(n,'MachineOverviewIdentity',(456,228),(0,-60),'选中机器的身份与当前状态')
    panel(n,'MachineOverviewCapacity',(456,228),(0,-300),'运行与能力详情')
    # Detail rows are pooled at runtime. Give the label/value pair enough vertical room to remain visible.
    for key in ['Identity','Capacity']:
        width = 416
        body = n['Txt_MachineOverview' + key + 'Body']
        rect(body,(width,56),(0,0),(0,1),(1,1),(.5,1))
        component(body,'LayoutElement',minHeight=56,preferredHeight=56,preferredWidth=width,flexibleWidth=1)
        template = n['Item_MachineOverview' + key + 'Template']
        rect(template,(width,88),(0,0),(0,1),(0,1),(.0,1))
        component(template,'LayoutElement',minHeight=88,preferredHeight=88,preferredWidth=width,flexibleWidth=1)
        rect(n['Txt_MachineOverview' + key + 'RowLabel'],(-24,32),(0,-8),(0,1),(1,1),(.5,1))
        rect(n['Txt_MachineOverview' + key + 'RowValue'],(-24,44),(0,8),(0,0),(1,0),(.5,0))
    actions=n['Grp_MachineOverviewActions'];rect(actions,(-24,264),(0,12),(0,0),(1,0),(.5,0))
    component(actions,'GridLayoutGroup',cellSize=[210,44],spacing=[12,10],constraint=1,constraintCount=2)
    for x in actions['children']:rect(x,(210,44),(0,0))
    rect(n['Btn_FieldClose'],(72,40),(-40,-124),(1,1))
    for state in ['Loading','Empty','Error','Success','Disabled']:
        g=n['Grp_MachineOverview'+state+'State'];rect(g,(456,468),(0,-60))
        rect(n['Panel_MachineOverview'+state+'Message'],(424,120),(0,0),(.5,.5))

    for name,d in documents.items():
        d['layoutMigration']='b51: apply-existing-layout (preserve undeclared component references)'
        (DEST/(name+'.contract.json')).write_text(json.dumps(d,ensure_ascii=False,indent=1)+'\n',encoding='utf-8')
    export_layout(documents)
    verify_unmodified(documents)


def verify_unmodified(documents):
    results=[]
    for name,target in [('MachineLibraryForm','Panel_PageMachinePreparation'),('BaseCommandHubForm','Panel_PageHubOverview')]:
        before=json.loads((CHANGE/'evidence/baseline'/(name+'.contract.json')).read_text(encoding='utf-8-sig'))
        a=index(before['root']);b=index(documents[name]['root'])
        for key,value in a.items():
            if key.startswith('Panel_Page') and key!=target:
                assert value==b[key],key
                results.append({'form':name,'page':key,'unchanged':True,'sha256':hashlib.sha256(json.dumps(value,sort_keys=True).encode()).hexdigest()})
    (CHANGE/'evidence/non-target-pages.json').write_text(json.dumps(results,indent=2),encoding='utf-8')


def export_layout(documents):
    s='# B51 代表界面结构文档\n\n## 全局约定\n\n1920×1080 唯一验收；根 Canvas 与 Scaler 继承 GF，不在 Form 内新增 Canvas。根 Stretch；边缘模块按对应边缘锚定。字体沿用 SIMHEI SDF；Image 占位为中性结构资源，正式美术未制作。旋转为零，缩放为一。动画未来挂展示子容器，不驱动 LayoutGroup 的子节点。\n\n## 关键决策\n\n机器整备：左参数/就绪、中央物件占位、右槽位；HUD：五模块共同常驻、中央世界可操作；中枢总览：概况与运行纵向组织、右侧关注区。未接入领域明确说明，不生成示例经营数据。其它分页完全保持。\n\n## 状态清单\n\n所有工作区保留正常、空、读取失败、不可用与操作反馈；加载成功不以遮挡卡覆盖内容。按钮保留正常/焦点/按下/禁用，未接线入口禁用。列表模板 inactive，运行时走现有对象池。装饰和文本 raycast=false，按钮自身图形 true。\n\n## 跨页复用\n\n复用现有控件、列表池和阅读语义；本批展示占位仅机器整备使用，暂不引入通用美术框架。\n\n'
    for name,d in documents.items():
        s+='## '+name+'\n\n下表是完整节点树；components/text/active 等完整值见同名 contract.json。四项矩形数据单位为参考像素。\n\n| 节点路径 | anchorMin→Max | pivot | sizeDelta | anchoredPosition |\n|---|---|---|---|---|\n'
        def walk(n,path=''):
            nonlocal s
            path=path+'/'+n['name'] if path else n['name']
            s+=f"| {path} | {n.get('anchorMin')} → {n.get('anchorMax')} | {n.get('pivot')} | {n.get('sizeDelta')} | {n.get('anchoredPosition')} |\n"
            for c in n.get('children',[]):walk(c,path)
        walk(d['root'])
        s+='\n'
    s+='## 变更日志\n\n2026-10-09：按用户已确认首批结构范围实施；正式美术后续制作。\n'
    (CHANGE/'art/prefab-layout.md').write_text(s,encoding='utf-8')


if __name__=='__main__':
    main()
