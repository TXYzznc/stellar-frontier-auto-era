using System;
using System.Collections.Generic;
using AutoEra.Motion;

namespace AutoEra.Editor.Motion
{
    public static class MotionStaticValidator
    {
        public static string[] Validate(MotionRig rig, MotionGraphAsset graph)
        {
            var errors = new List<string>();
            if (rig == null) errors.Add("MotionRig is required.");
            else if (!rig.TryValidate(out string rigError)) errors.Add(rigError);
            if (graph == null) errors.Add("MotionGraphAsset is required.");
            else if (!graph.TryValidate(out string graphError)) errors.Add(graphError);
            if (rig != null && graph != null && graph.TryValidate(out _))
            {
                foreach (MotionNodeDefinition node in graph.Nodes)
                {
                    if (!string.IsNullOrEmpty(node.TargetJointId) && !rig.TryGetBinding(node.TargetJointId, out _)) errors.Add("Graph node references missing rig joint: " + node.StableId);
                }
            }
            return errors.ToArray();
        }
    }
}
