from __future__ import annotations

import importlib.util
import sys
import tempfile
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "audit_project_boundaries.py"
SPEC = importlib.util.spec_from_file_location("audit_project_boundaries", MODULE_PATH)
audit_module = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
sys.modules[SPEC.name] = audit_module
SPEC.loader.exec_module(audit_module)


class ProjectBoundaryAuditTests(unittest.TestCase):
    def make_root(self) -> Path:
        temp = tempfile.TemporaryDirectory()
        self.addCleanup(temp.cleanup)
        root = Path(temp.name)

        (root / "Docs/Development").mkdir(parents=True)
        (root / "Docs/Development/ProjectBaseline.md").write_text(
            "# Project baseline\n",
            encoding="utf-8",
        )
        (root / ".claude").mkdir(parents=True)
        (root / "AGENTS.md").write_text(
            "[baseline](Docs/Development/ProjectBaseline.md)\n",
            encoding="utf-8",
        )
        (root / "README.md").write_text(
            "[baseline](Docs/Development/ProjectBaseline.md)\n",
            encoding="utf-8",
        )
        (root / ".claude/CLAUDE.md").write_text(
            "[baseline](../Docs/Development/ProjectBaseline.md)\n",
            encoding="utf-8",
        )
        (root / "Assets/Game/Scripts/AutoEra").mkdir(parents=True)
        (root / "Assets/Game/Scripts/AutoEra/README.md").write_text(
            "# AutoEra\n",
            encoding="utf-8",
        )
        (root / "Assets/Game/ScriptsBuiltin").mkdir(parents=True)
        return root

    @staticmethod
    def rules(root: Path) -> set[str]:
        return {finding.rule for finding in audit_module.audit(root)}

    def test_clean_project_boundaries_pass(self):
        root = self.make_root()
        path = root / "Assets/Game/Scripts/AutoEra/World/WorldClock.cs"
        path.parent.mkdir(parents=True)
        path.write_text(
            "namespace AutoEra.World;\n\npublic sealed class WorldClock {}\n",
            encoding="utf-8",
        )

        self.assertEqual([], audit_module.audit(root))

    def test_product_file_without_product_namespace_is_rejected(self):
        root = self.make_root()
        path = root / "Assets/Game/Scripts/AutoEra/WorldClock.cs"
        path.write_text("public sealed class WorldClock {}\n", encoding="utf-8")

        self.assertIn("product-namespace", self.rules(root))

    def test_product_namespace_outside_product_root_is_rejected(self):
        root = self.make_root()
        path = root / "Assets/Game/Scripts/World/WorldClock.cs"
        path.parent.mkdir(parents=True)
        path.write_text(
            "namespace AutoEra.World { public sealed class WorldClock {} }\n",
            encoding="utf-8",
        )

        self.assertIn("product-path", self.rules(root))

    def test_builtin_product_dependency_is_rejected(self):
        root = self.make_root()
        path = root / "Assets/Game/ScriptsBuiltin/Bootstrap.cs"
        path.write_text(
            "using AutoEra.World;\n\npublic sealed class Bootstrap {}\n",
            encoding="utf-8",
        )

        self.assertIn("framework-dependency", self.rules(root))

    def test_comments_do_not_create_builtin_dependency(self):
        root = self.make_root()
        path = root / "Assets/Game/ScriptsBuiltin/Bootstrap.cs"
        path.write_text(
            "// AutoEra is discussed here only.\npublic sealed class Bootstrap {}\n",
            encoding="utf-8",
        )

        self.assertEqual([], audit_module.audit(root))

    def test_redundant_product_resource_directory_is_rejected(self):
        root = self.make_root()
        (root / "Assets/Game/Models/AutoEra/Machines").mkdir(parents=True)

        self.assertIn("redundant-product-directory", self.rules(root))

    def test_missing_baseline_entry_link_is_rejected(self):
        root = self.make_root()
        (root / "README.md").write_text("# No baseline link\n", encoding="utf-8")

        self.assertIn("baseline-entry", self.rules(root))


if __name__ == "__main__":
    unittest.main()
