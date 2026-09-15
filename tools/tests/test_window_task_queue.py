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

    def test_block_yields_and_is_not_automatically_resumed(self):
        self.enqueue("WAIT", 0)
        self.run_queue("claim", "--role", "art-concept-3d")
        self.enqueue("READY", 10)
        result = self.run_queue("block", "--role", "art-concept-3d", "--task-id", "WAIT",
                                "--reason", "external decision", "--checkpoint", "saved",
                                "--resume-when", "decision received", "--claim-next")
        self.assertEqual(result["nextActive"]["taskId"], "READY")
        result = self.run_queue("complete", "--role", "art-concept-3d", "--task-id", "READY", "--claim-next")
        self.assertIsNone(result["nextActive"])
        self.assertIsNone(self.run_queue("claim", "--role", "art-concept-3d")["active"])
        result = self.run_queue("unblock", "--role", "art-concept-3d", "--task-id", "WAIT",
                                "--evidence", "decision recorded", "--claim-next")
        self.assertEqual(result["active"]["taskId"], "WAIT")

    def test_unblock_never_preempts_active(self):
        self.enqueue("WAIT")
        self.run_queue("claim", "--role", "art-concept-3d")
        self.enqueue("READY")
        self.run_queue("block", "--role", "art-concept-3d", "--task-id", "WAIT",
                       "--reason", "dependency", "--checkpoint", "saved", "--resume-when", "ready", "--claim-next")
        result = self.run_queue("unblock", "--role", "art-concept-3d", "--task-id", "WAIT",
                                "--evidence", "ready", "--claim-next")
        self.assertEqual(result["active"]["taskId"], "READY")

    def test_block_requires_checkpoint_and_correct_active(self):
        self.enqueue("A")
        self.run_queue("claim", "--role", "art-concept-3d")
        for task_id, checkpoint in [("A", ""), ("OTHER", "saved")]:
            self.run_queue("block", "--role", "art-concept-3d", "--task-id", task_id,
                           "--reason", "dependency", "--checkpoint", checkpoint, "--resume-when", "ready", ok=False)
        self.assertEqual(self.run_queue("status", "--role", "art-concept-3d")["state"]["active"]["taskId"], "A")


if __name__ == "__main__":
    unittest.main()
