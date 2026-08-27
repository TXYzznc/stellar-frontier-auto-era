import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


SCRIPT = Path(__file__).resolve().parents[1] / "window_task_queue.py"


class WindowTaskQueueTests(unittest.TestCase):
    def setUp(self):
        self.temp_dir = tempfile.TemporaryDirectory()
        self.queue_file = Path(self.temp_dir.name) / "queue.json"

    def tearDown(self):
        self.temp_dir.cleanup()

    def run_queue(self, *args, ok=True):
        command = [sys.executable, str(SCRIPT), "--queue-file", str(self.queue_file), *args]
        result = subprocess.run(command, text=True, capture_output=True, check=False)
        if ok and result.returncode != 0:
            self.fail(result.stderr)
        if not ok:
            self.assertNotEqual(result.returncode, 0)
            return json.loads(result.stderr)
        return json.loads(result.stdout)

    def enqueue(self, task_id, priority=50):
        return self.run_queue(
            "enqueue", "--role", "art-concept-3d", "--task-id", task_id,
            "--title", task_id, "--priority", str(priority),
        )

    def test_new_task_never_replaces_active_task(self):
        self.enqueue("G01", 10)
        self.run_queue("claim", "--role", "art-concept-3d")
        result = self.enqueue("G02", 0)
        self.assertTrue(result["activeUnchanged"])
        status = self.run_queue("status", "--role", "art-concept-3d")
        self.assertEqual(status["state"]["active"]["taskId"], "G01")

    def test_pending_uses_priority_then_fifo(self):
        self.enqueue("A", 10)
        self.enqueue("B", 0)
        self.enqueue("C", 10)
        first = self.run_queue("claim", "--role", "art-concept-3d")
        self.assertEqual(first["active"]["taskId"], "B")
        second = self.run_queue(
            "complete", "--role", "art-concept-3d", "--task-id", "B", "--claim-next"
        )
        self.assertEqual(second["nextActive"]["taskId"], "A")

    def test_preemption_requires_checkpoint_and_resumes_original(self):
        self.enqueue("NORMAL", 10)
        self.run_queue("claim", "--role", "art-concept-3d")
        self.enqueue("URGENT", 0)
        rejected = self.run_queue(
            "preempt", "--role", "art-concept-3d", "--task-id", "URGENT",
            "--reason-kind", "s1", "--checkpoint", "", "--approved-by", "producer", ok=False,
        )
        self.assertIn("checkpoint", rejected["error"])
        accepted = self.run_queue(
            "preempt", "--role", "art-concept-3d", "--task-id", "URGENT",
            "--reason-kind", "s1", "--checkpoint", "saved G01", "--approved-by", "producer",
        )
        self.assertEqual(accepted["active"]["taskId"], "URGENT")
        resumed = self.run_queue(
            "complete", "--role", "art-concept-3d", "--task-id", "URGENT", "--claim-next"
        )
        self.assertEqual(resumed["nextActive"]["taskId"], "NORMAL")


if __name__ == "__main__":
    unittest.main()
