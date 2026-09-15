from __future__ import annotations

import importlib.util
import json
import sys
import tempfile
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "audit_framework_purity.py"
SPEC = importlib.util.spec_from_file_location("audit_framework_purity", MODULE_PATH)
audit_module = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
sys.modules[SPEC.name] = audit_module
SPEC.loader.exec_module(audit_module)


class FrameworkPurityAuditTests(unittest.TestCase):
    def setUp(self):
        original_skills = audit_module.ALLOWED_SKILLS
        original_agents = audit_module.ALLOWED_AGENTS
        audit_module.ALLOWED_SKILLS = {"core-skill"}
        audit_module.ALLOWED_AGENTS = set()
        self.addCleanup(setattr, audit_module, "ALLOWED_SKILLS", original_skills)
        self.addCleanup(setattr, audit_module, "ALLOWED_AGENTS", original_agents)

    def make_root(self) -> Path:
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        root = Path(temp.name)
        (root / ".claude/skills/core-skill").mkdir(parents=True)
        (root / ".claude/skills/core-skill/SKILL.md").write_text(
            "---\nname: core-skill\ndescription: Generic workflow.\n---\n\n# Core\n",
            encoding="utf-8",
        )
        (root / ".claude/skills/SKILLS_INDEX.md").write_text(
            "# Index\n\n## 当前 SKILL\n\n`core-skill`\n",
            encoding="utf-8",
        )
        (root / ".claude/agents").mkdir(parents=True)
        (root / ".codex/agents").mkdir(parents=True)
        (root / "ProjectSettings").mkdir(parents=True)
        (root / "ProjectSettings/EditorBuildSettings.asset").write_text(
            "EditorBuildSettings:\n  m_Scenes: []\n",
            encoding="utf-8",
        )
        return root

    def test_clean_minimal_framework_passes(self):
        self.assertEqual([], audit_module.audit(self.make_root()))

    def test_ignored_ai_usage_events_are_allowed(self):
        root = self.make_root()
        events = root / ".ai/usage/events.jsonl"
        events.parent.mkdir(parents=True)
        events.write_text('{"schema_version": 1}\n', encoding="utf-8")

        self.assertEqual([], audit_module.audit(root))

    def test_openspec_archives_are_allowed(self):
        root = self.make_root()
        archive = root / "openspec/changes/archive/2026-08-13-completed-change"
        archive.mkdir(parents=True)
        (archive / "proposal.md").write_text("# Completed change\n", encoding="utf-8")

        self.assertEqual([], audit_module.audit(root))

    def test_unapproved_skill_is_rejected(self):
        root = self.make_root()
        path = root / ".claude/skills/domain-specific-skill"
        path.mkdir()
        (path / "SKILL.md").write_text(
            "---\nname: domain-specific-skill\ndescription: Not approved.\n---\n",
            encoding="utf-8",
        )
        self.assertTrue(any(item.rule == "skill-allowlist" for item in audit_module.audit(root)))

    def test_agent_unknown_skill_is_rejected(self):
        root = self.make_root()
        audit_module.ALLOWED_AGENTS = {"tool"}
        (root / ".claude/agents/tool.md").write_text(
            "---\nname: tool\nskills:\n  - missing-skill\n---\n",
            encoding="utf-8",
        )
        (root / ".codex/agents/tool.toml").write_text('name = "tool"\n', encoding="utf-8")
        self.assertTrue(any(item.rule == "agent-skill" for item in audit_module.audit(root)))

    def test_sample_directory_is_rejected(self):
        root = self.make_root()
        (root / "Assets/Game/Samples").mkdir(parents=True)
        self.assertTrue(any(item.rule == "sample-directory" for item in audit_module.audit(root)))

    def test_legacy_singular_material_directory_is_rejected(self):
        root = self.make_root()
        (root / "Assets/Game/Material").mkdir(parents=True)
        findings = audit_module.audit(root)
        self.assertTrue(
            any(
                item.rule == "forbidden-path" and item.path == "Assets/Game/Material"
                for item in findings
            )
        )

    def test_legacy_singular_material_path_reference_is_rejected(self):
        root = self.make_root()
        path = root / "Assets/Game/Scripts/LegacyPath.cs"
        path.parent.mkdir(parents=True)
        path.write_text(
            'const string Path = "Assets/Game/Material/Test.mat";\n',
            encoding="utf-8",
        )
        self.assertTrue(
            any(
                item.rule == "legacy-content"
                and item.detail == "legacy singular material path"
                for item in audit_module.audit(root)
            )
        )

    def test_business_identifier_is_rejected(self):
        root = self.make_root()
        (root / ".claude/CLAUDE.md").write_text(
            "Business runtime: BusinessEntity\n",
            encoding="utf-8",
        )
        self.assertTrue(any(item.rule == "legacy-content" for item in audit_module.audit(root)))

    def product_fixture(self):
        root = self.make_root()
        code = "Assets/Game/Scripts/Product/Entry.cs"
        scene = "Assets/Game/Scene/MainMenu.unity"
        for name, content in ((code, "class MainMenu {}"), (scene, "scene"), ("approval.md", "Approved")):
            path = root / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(content, encoding="utf-8")
        (root / "ProjectSettings/EditorBuildSettings.asset").write_text(
            "m_Scenes:\n  - enabled: 1\n    path: " + scene + "\n", encoding="utf-8")
        profile = {"schemaVersion": 1, "source": "approval.md", "rule": "sample launch identifier",
                   "mainMenuFiles": [code, "ProjectSettings/EditorBuildSettings.asset"], "enabledScenes": [scene]}
        (root / "profile.json").write_text(json.dumps(profile), encoding="utf-8")
        return root, profile, code

    def test_product_profile_is_explicit_and_strict_mode_stays_strict(self):
        root, _, _ = self.product_fixture()
        self.assertTrue(any(f.rule == "build-settings" for f in audit_module.audit(root)))
        self.assertTrue(any(f.rule == "legacy-content" for f in audit_module.audit(root)))
        profile = audit_module.load_product_profile(root, "profile.json")
        self.assertEqual([], audit_module.audit(root, profile))

    def test_exception_does_not_hide_samplescene_or_other_patterns(self):
        root, profile, code = self.product_fixture()
        (root / code).write_text("MainMenu SampleScene BusinessEntity", encoding="utf-8")
        findings = audit_module.audit(root, profile)
        self.assertTrue(any(f.detail == "sample launch identifier" for f in findings))
        self.assertTrue(any(f.detail == "business runtime type" for f in findings))

    def test_unlisted_file_and_scene_still_fail(self):
        root, profile, _ = self.product_fixture()
        (root / "Assets/Game/Scripts/Other.cs").write_text("MainMenu", encoding="utf-8")
        with (root / "ProjectSettings/EditorBuildSettings.asset").open("a", encoding="utf-8") as stream:
            stream.write("  - enabled: 1\n    path: Assets/Game/Scene/Other.unity\n")
        findings = audit_module.audit(root, profile)
        self.assertTrue(any(f.path.endswith("Other.cs") for f in findings))
        self.assertTrue(any(f.rule == "build-settings" for f in findings))

    def test_framework_core_cannot_be_allowlisted(self):
        root, profile, _ = self.product_fixture()
        core = root / "Assets/Game/ScriptsBuiltin/Core.cs"
        core.parent.mkdir(parents=True)
        core.write_text("MainMenu", encoding="utf-8")
        self.assertTrue(any(f.path.endswith("Core.cs") for f in audit_module.audit(root, profile)))
        profile["mainMenuFiles"].append("Assets/Game/ScriptsBuiltin/Core.cs")
        (root / "profile.json").write_text(json.dumps(profile), encoding="utf-8")
        with self.assertRaises(ValueError): audit_module.load_product_profile(root, "profile.json")

    def test_invalid_and_escaping_profiles_fail_closed(self):
        root, profile, _ = self.product_fixture()
        for path in ("../profile.json", "/profile.json", "missing.json", "./profile.json"):
            with self.assertRaises(ValueError): audit_module.load_product_profile(root, path)
        for bad in ({}, {**profile, "rule": "all"}, {**profile, "mainMenuFiles": ["../outside.cs"]},
                    {**profile, "enabledScenes": ["Assets/Game/Scene/Missing.unity"]}):
            (root / "profile.json").write_text(json.dumps(bad), encoding="utf-8")
            with self.assertRaises(ValueError): audit_module.load_product_profile(root, "profile.json")


if __name__ == "__main__":
    unittest.main()
