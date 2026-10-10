using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Save
{
    internal static class WorldSnapshotIdentityValidator
    {
        internal static bool Validate(WorldSessionSnapshot session,WorldRegionSnapshot region,RegionExecutionSnapshot execution,
            ulong allocated,ulong[] domainIdentities,out string reason)
        {
            reason="世界身份或跨领域引用无效";
            if(session?.Roster?.Machines==null || session.Roster.Components==null || session.Roster.PendingHardware==null || session.Templates==null || session.Events==null ||
                region?.State?.Objects==null || region.Queues==null || region.Presentation==null || execution?.Machines==null ||
                execution.PublicProviders==null || domainIdentities==null || session.Roster.AllocatedThrough!=allocated || region.State.AllocatedThrough!=allocated ||
                double.IsNaN(session.FractionalMilliseconds) || double.IsInfinity(session.FractionalMilliseconds) || session.FractionalMilliseconds<0 || session.FractionalMilliseconds>=1)return false;
            var ids=new HashSet<ulong>();var machines=new HashSet<ulong>();var deployed=new HashSet<ulong>();var objects=new HashSet<ulong>();
            bool Own(ulong id) => id!=0 && id<=allocated && ids.Add(id);
            foreach(var row in session.Roster.Machines)
            { if(row==null || !Own(row.Id))return false;machines.Add(row.Id);if(row.Deployed)deployed.Add(row.Id); }
            foreach(var row in session.Roster.Components)if(row==null || !Own(row.Id) || row.Owner!=0 && !machines.Contains(row.Owner))return false;
            foreach(var row in session.Templates)if(row==null || !Own(row.Id))return false;
            var regionMachines=new HashSet<ulong>();
            foreach(var row in region.State.Objects)
            {
                if(row==null || !objects.Add(row.Id))return false;
                if(row.Kind==PersistentObjectKind.Machine) { if(!deployed.Contains(row.Id) || !regionMachines.Add(row.Id))return false; }
                else if(!Own(row.Id))return false;
            }
            if(!regionMachines.SetEquals(deployed))return false;
            foreach(var id in domainIdentities)if(!Own(id))return false;
            var hosts=new HashSet<ulong>();
            foreach(var host in execution.Machines)
            {
                if(host==null || !deployed.Contains(host.Machine.Value) || !hosts.Add(host.Machine.Value) || host.Tasks?.Live==null || host.Tasks.History==null ||
                    host.Compute?.Running==null || host.Compute.Waiting==null || host.Hardware?.Effectors==null || host.Algorithms==null)return false;
                foreach(var task in host.Tasks.Live)if(task==null || !Own(task.Id))return false;
                foreach(var task in host.Tasks.History)if(task==null || !Own(task.Id))return false;
                foreach(var lease in host.Compute.Running)if(lease==null || !Own(lease.Id.Value))return false;
                foreach(var lease in host.Compute.Waiting)if(lease==null || !Own(lease.Id.Value))return false;
                foreach(var effector in host.Hardware.Effectors)
                {
                    if(effector?.Queue?.Waiting==null)return false;
                    if(effector.Queue.Current!=null && !Own(effector.Queue.Current.Id.Value))return false;
                    foreach(var request in effector.Queue.Waiting)if(request==null || !Own(request.Id.Value))return false;
                }
                foreach(var algorithm in host.Algorithms)
                {
                    if(algorithm==null || !Own(algorithm.InstanceId))return false;
                    if(algorithm.Request!=null && !Own(algorithm.Request.RequestId))return false;
                    // Draft/saved/applied/history are versions of the same node identities within this instance.
                    var nodes=new HashSet<ulong>();
                    bool Document(AlgorithmDocument doc)
                    {
                        if(doc==null)return true;
                        if(doc.DocumentId!=algorithm.InstanceId || doc.Nodes==null)return false;
                        foreach(var node in doc.Nodes)if(node==null || node.Id==0 || node.Id>allocated || nodes.Add(node.Id) && !Own(node.Id))return false;
                        return true;
                    }
                    if(!Document(algorithm.Draft) || !Document(algorithm.Saved) || !Document(algorithm.Request?.Document) || !Document(algorithm.Runtime?.Applied))return false;
                    if(algorithm.Runtime?.History!=null)foreach(var history in algorithm.Runtime.History)if(history==null || !Document(history.ExecutedDocument))return false;
                }
            }
            if(!hosts.SetEquals(deployed))return false;
            var channels=new HashSet<string>(StringComparer.Ordinal);
            foreach(var queue in region.Queues)
            {
                if(queue==null || !objects.Contains(queue.Target) || string.IsNullOrWhiteSpace(queue.Channel) ||
                    !channels.Add(queue.Target+":"+queue.Channel) || queue.Waiting==null || queue.Owner!=0 && !deployed.Contains(queue.Owner))return false;
                foreach(var waiter in queue.Waiting)if(waiter==null || !deployed.Contains(waiter.Machine))return false;
            }
            var bound=new HashSet<ulong>();var seeds=new HashSet<int>();
            foreach(var binding in region.Presentation)
                if(binding==null || !objects.Contains(binding.Object) || !bound.Add(binding.Object) || binding.ContentVersion!=1 ||
                    string.IsNullOrWhiteSpace(binding.Asset) || binding.SeedIndex< -1 || binding.SeedIndex>=0 && !seeds.Add(binding.SeedIndex) ||
                    binding.SeedIndex== -1 && !deployed.Contains(binding.Object))return false;
            if(!bound.SetEquals(objects))return false;
            reason=null;return true;
        }
    }
}
