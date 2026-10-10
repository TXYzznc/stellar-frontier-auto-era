"""Generate B52 family-specific structure contracts from the existing authored contracts."""
import json
from pathlib import Path

CHANGE = Path(__file__).resolve().parents[1]
ROOT = CHANGE.parents[2]
CONTRACTS = ROOT / "Docs/Development/UI-PrefabLayouts"
AUTHORED = CHANGE / "evidence/authored"
ITEMS = CHANGE / "art/items"

FORMS = {
    2: ("system", ["MainMenuForm", "SaveSlotsForm", "SaveRecoveryForm", "SystemMenuForm", "SettingsForm", "ExitFlowForm", "OperationDialogForm", "OperationFeedbackForm"]),
    3: ("directory", ["MachineLibraryForm", "ComponentLibraryForm", "ComponentPickerForm", "NodeComponentPickerForm", "WorldObjectPickerForm", "AlgorithmBindingForm", "AlgorithmLibraryForm", "BuildCatalogForm", "ShopForm", "UpgradeForm"]),
    4: ("production", ["WorkshopForm", "WarehouseForm", "FieldHudForm", "FieldHudResidentForm", "FieldHudMachineOverviewForm", "FieldHudDetailForm", "WorldPlacementForm"]),
    5: ("algorithm", ["AlgorithmEditorForm", "AlgorithmPublicParametersForm"]),
    6: ("reading", ["BaseCommandHubForm", "BaseCommandEnergyForm", "AlertForm", "QuestForm", "RecordReaderForm", "ProgressReportForm", "TutorialForm", "RuleHelpForm", "HelpForm", "FeatureHelpForm", "CropKnowledgeForm"]),
}
RETAINED = {"FieldHudForm", "FieldHudResidentForm", "FieldHudMachineOverviewForm"}


def rect(node, size, pos=(0, 0), amin=(0, 1), amax=None, pivot=None):
    node.update(sizeDelta=list(size), anchoredPosition=list(pos), anchorMin=list(amin),
                anchorMax=list(amax if amax is not None else amin), pivot=list(pivot if pivot is not None else amin))


def index(node):
    result = {node["name"]: node}
    for child in node.get("children", []):
        result.update(index(child))
    return result


def pages(root):
    return [n for n in index(root).values() if n["name"].startswith("Panel_Page")]


def direct_content(page):
    return [n for n in page.get("children", []) if n["name"].startswith("Panel_") and not n["name"].endswith("State")]


def image(node, color):
    for component in node.setdefault("components", []):
        if component.get("type") == "Image":
            component.update(color=list(color), raycastTarget=True)
            return


def family_color(family, index):
    bases = {
        "system": (.13, .12, .16),
        "directory": (.08, .14, .17),
        "production": (.08, .15, .12),
        "algorithm": (.14, .10, .17),
        "reading": (.16, .13, .09),
    }
    base = bases[family]
    lift = min(index * .012, .036)
    return [base[0] + lift, base[1] + lift, base[2] + lift, 1]


def set_page(page, family):
    rect(page, (0, 0), (0, 0), (0, 0), (1, 1), (.5, .5))
    content = direct_content(page)
    if not content:
        return
    if family == "algorithm" and page["name"] == "Panel_PageAlgorithmEditor":
        positions = {"Panel_AlgorithmEditorNodes": ((320, 480), (0, -112)), "Panel_AlgorithmEditorCanvas": ((1000, 480), (336, -112)), "Panel_AlgorithmEditorInspector": ((376, 480), (1352, -112)), "Panel_AlgorithmEditorProblems": ((1728, 80), (0, -608)), "Panel_AlgorithmEditorToolbar": ((1728, 96), (0, -704))}
        for node in content:
            if node["name"] in positions:
                size, pos = positions[node["name"]]
                rect(node, size, pos)
    elif family == "system":
        if len(content) == 1:
            rect(content[0], (960, 570), (264, -42))
        elif len(content) == 2:
            rect(content[0], (620, 570), (0, -42))
            rect(content[1], (800, 570), (648, -42))
        else:
            width = 440
            for i, node in enumerate(content):
                rect(node, (width, 570), (i * 468, -42))
    elif family == "directory":
        if len(content) == 1:
            rect(content[0], (1120, 570), (0, -42))
        elif len(content) == 2:
            rect(content[0], (500, 570), (0, -42))
            rect(content[1], (960, 570), (528, -42))
        else:
            width = 440
            for i, node in enumerate(content):
                rect(node, (width, 570), (i * 468, -42))
    elif family == "production":
        if len(content) == 1:
            rect(content[0], (1180, 570), (0, -42))
        elif len(content) == 2:
            rect(content[0], (430, 570), (0, -42))
            rect(content[1], (1030, 570), (454, -42))
        else:
            widths = [400, 620, 400]
            x = 0
            for i, node in enumerate(content):
                width = widths[i] if i < len(widths) else 400
                rect(node, (width, 570), (x, -42))
                x += width + 24
    elif family == "reading":
        if len(content) == 1:
            rect(content[0], (1280, 570), (0, -42))
        elif len(content) == 2:
            rect(content[0], (460, 570), (0, -42))
            rect(content[1], (1000, 570), (484, -42))
        else:
            width = 460
            for i, node in enumerate(content):
                rect(node, (width, 570), (i * 484, -42))
    for i, node in enumerate(content):
        image(node, family_color(family, i))
        # Keep lists visually distinct from their detail/readout panel.
        for child in node.get("children", []):
            if child["name"].startswith("Content_"):
                for component in child.setdefault("components", []):
                    if component.get("type") in ("VerticalLayoutGroup", "HorizontalLayoutGroup"):
                        component.update(spacing=10, childForceExpandWidth=True, childForceExpandHeight=False)


def transform(doc, family):
    root = index(doc["root"])
    frame = root.get("Panel_Frame")
    if frame is not None and doc.get("form") not in RETAINED:
        frame_sizes = {"system": (1520, 840), "directory": (1680, 900), "production": (1760, 900), "algorithm": (1840, 980), "reading": (1720, 900)}
        rect(frame, frame_sizes[family], (0, 0), (.5, .5), (.5, .5), (.5, .5))
    for page in pages(doc["root"]):
        # b51 representative pages are already approved; their contracts are retained verbatim.
        if doc.get("form") in RETAINED and page["name"] in {"Panel_PageMachinePreparation", "Panel_PageHubOverview", "Panel_PageHudStatus", "Panel_PageHudTracker", "Panel_PageHudAlerts", "Panel_PageHudNavigation", "Panel_PageHudSave", "Panel_PageMachineOverview"}:
            continue
        set_page(page, family)
    if doc.get("form") == "AlgorithmEditorForm":
        # The editor has two action bands: editing commands live below the tabs,
        # diagnosis commands stay in the lower work area and must not overlap.
        if "Grp_AlgorithmEditorActions" in root:
            rect(root["Grp_AlgorithmEditorActions"], (-16, 48), (0, -48), (0, 1), (1, 1), (.5, 1))
        if "Grp_AlgorithmDiagnosisActions" in root:
            rect(root["Grp_AlgorithmDiagnosisActions"], (-16, 48), (0, -48), (0, 1), (1, 1), (.5, 1))
            root["Grp_AlgorithmDiagnosisActions"]["active"] = False
        rect(root["Txt_AlgorithmEditorToolbarHeading"], (240, 32), (12, -8))
        rect(root["Txt_AlgorithmToolbarStatus"], (-24, 44), (0, -44), (0, 1), (1, 1), (.5, 1))
        rect(root["Grp_AlgorithmDraftTools"], (360, 32), (-12, -8), (1, 1), (1, 1), (1, 1))
    doc["layoutMigration"] = "b52: family-specific structure rollout; preserve serialized bindings and functional page routes"


def main():
    manifest = {"schemaVersion": 1, "change": "b52-ui-all-pages-visual-optimization", "referenceResolution": [1920, 1080], "forms": []}
    for batch, (family, names) in FORMS.items():
        for name in names:
            path = CONTRACTS / (name + ".contract.json")
            if not path.exists():
                continue
            doc = json.loads(path.read_text(encoding="utf-8-sig"))
            transform(doc, family)
            path.write_text(json.dumps(doc, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
            manifest["forms"].append({"batch": batch, "family": family, "form": name, "contract": path.as_posix(), "pages": [p["name"] for p in pages(doc["root"])], "retained": name in RETAINED})
    ITEMS.mkdir(parents=True, exist_ok=True)
    for name in ("AlgorithmNodeItem", "AlgorithmEdgeItem"):
        source = AUTHORED / (name + ".json")
        if source.exists():
            doc = json.loads(source.read_text(encoding="utf-8-sig"))
            # Future Item batches can be applied without touching runtime scripts.
            rect(doc["root"], (320, 108), (0, 0), (0, 1), (0, 1), (0, 1))
            (ITEMS / (name + ".json")).write_text(json.dumps(doc, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
    (CHANGE / "art/rollout-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Generated", len(manifest["forms"]), "form entries")


if __name__ == "__main__":
    main()
