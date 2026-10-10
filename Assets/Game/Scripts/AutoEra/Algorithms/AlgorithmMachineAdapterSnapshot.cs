using System;
using System.Collections.Generic;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.Algorithms
{
    public sealed class AlgorithmAdapterTaskSnapshot
    {
        public ulong Task, Instance;
    }
    public sealed class AlgorithmAdapterNavigationSnapshot
    {
        public AlgorithmTrigger Trigger;
        public ulong Node, Task;
        public Vector3 Position;
        public float? FacingYaw;
        public bool TracksTransport;
    }
    public sealed class AlgorithmAdapterEffectorSnapshot
    {
        public AlgorithmTrigger Trigger;
        public ulong Node, Task, Behavior, Component, ComponentGeneration;
    }
    public sealed class AlgorithmAdapterResultSnapshot
    {
        public AlgorithmTrigger Trigger;
        public ulong Task;
    }
    public sealed class AlgorithmMachineAdapterSnapshot
    {
        public ulong Machine;
        public long WorldMilliseconds;
        public string LastUnavailableReason;
        public AlgorithmAdapterTaskSnapshot[] OwnedTasks;
        public AlgorithmAdapterNavigationSnapshot ActiveNavigation;
        public AlgorithmAdapterNavigationSnapshot[] WaitingNavigation;
        public AlgorithmAdapterEffectorSnapshot[] Effectors;
        public AlgorithmAdapterResultSnapshot[] Results;
    }

    public sealed partial class AlgorithmMachineAdapter
    {
        public bool TryCapturePersistent(long now, out AlgorithmMachineAdapterSnapshot snapshot)
        {
            snapshot = null;
            if (_disposed || !_region.IsActive || now < 0 || now < _now) return false;
            var tasks = new List<AlgorithmAdapterTaskSnapshot>(_ownedTasks.Count);
            foreach (var id in _ownedTasks)
            {
                if (!_taskOwners.TryGetValue(id,out var instance) || instance == 0) return false;
                tasks.Add(new AlgorithmAdapterTaskSnapshot { Task=id.Value, Instance=instance });
            }
            tasks.Sort((a,b)=>a.Task.CompareTo(b.Task));
            AlgorithmAdapterNavigationSnapshot Navigation(Pending value) => value == null ? null : new AlgorithmAdapterNavigationSnapshot
            { Trigger=value.Trigger.Copy(), Node=value.NodeId, Task=value.Task.Value, Position=value.Position, FacingYaw=value.FacingYaw, TracksTransport=value.Transport!=null };
            var waiting = new AlgorithmAdapterNavigationSnapshot[_waiting.Count];
            for(int i=0;i<waiting.Length;i++) waiting[i]=Navigation(_waiting[i]);
            var effectors = new AlgorithmAdapterEffectorSnapshot[_effectorPending.Count];
            for(int i=0;i<effectors.Length;i++)
            {
                var value=_effectorPending[i]; PersistentId component=PersistentId.Invalid;
                foreach(var pair in _effectors)
                    if(pair.Value.TryFindPersistentBehavior(value.Behavior,out _))
                    { if(component.IsValid) return false; component=pair.Key; }
                if(!component.IsValid || !_effectorGenerations.TryGetValue(component,out var generation)) return false;
                effectors[i]=new AlgorithmAdapterEffectorSnapshot { Trigger=value.Trigger.Copy(), Node=value.NodeId, Task=value.Task.Value,
                    Behavior=value.Behavior.Value, Component=component.Value, ComponentGeneration=generation };
            }
            var results = new List<AlgorithmAdapterResultSnapshot>(_results.Count);
            foreach(var value in _results) results.Add(new AlgorithmAdapterResultSnapshot { Trigger=value.Trigger.Copy(), Task=value.Task.Value });
            snapshot=new AlgorithmMachineAdapterSnapshot { Machine=_context.Machine.Id.Value, WorldMilliseconds=now,
                LastUnavailableReason=LastCommandUnavailableReason, OwnedTasks=tasks.ToArray(), ActiveNavigation=Navigation(_active),
                WaitingNavigation=waiting, Effectors=effectors, Results=results.ToArray() };
            return true;
        }

        /// <summary>Restore after tasks, runtime instances, navigation and effector queues. No result is delivered until the next world step.</summary>
        public bool RestorePersistent(AlgorithmMachineAdapterSnapshot snapshot, long now, TransportResponsibilityLedger transport = null)
        {
            if(_disposed || _active!=null || _waiting.Count!=0 || _effectorPending.Count!=0 || _results.Count!=0 || _ownedTasks.Count!=0 || _taskOwners.Count!=0 ||
                snapshot?.OwnedTasks==null || snapshot.WaitingNavigation==null || snapshot.Effectors==null || snapshot.Results==null ||
                snapshot.Machine!=_context.Machine.Id.Value || snapshot.WorldMilliseconds<0 || snapshot.WorldMilliseconds>now) return false;
            var owned=new Dictionary<PersistentId,ulong>();
            bool KnownTask(PersistentId id, bool live)
            {
                if(_context.Tasks.TryGet(id,out _)) return true;
                if(!live) foreach(var value in _context.Tasks.History) if(value.Id==id) return true;
                return false;
            }
            foreach(var row in snapshot.OwnedTasks)
            {
                if(row==null || row.Task==0 || owned.ContainsKey(new PersistentId(row.Task)) || !_runtimes.ContainsKey(row.Instance) ||
                    !KnownTask(new PersistentId(row.Task),false)) return false;
                owned.Add(new PersistentId(row.Task),row.Instance);
            }
            AlgorithmTrigger Trigger(AlgorithmTrigger original, ulong node)
            {
                if(original==null || !_runtimes.TryGetValue(original.InstanceId,out var runtime) || runtime.Generation<=1 ||
                    original.Generation!=runtime.Generation-1 || original.Revision!=runtime.Revision || original.Time<0 || original.Time>snapshot.WorldMilliseconds ||
                    !AlgorithmPersistentValidation.TriggerValues(original)) return null;
                var graph=runtime.CopyApplied();
                if(!graph.Nodes.Exists(n=>!n.Deleted && n.Id==node) || !graph.Nodes.Exists(n=>!n.Deleted && n.Id==original.NodeId) ||
                    original.RootNodeId!=0 && !graph.Nodes.Exists(n=>!n.Deleted && n.Id==original.RootNodeId)) return null;
                var copy=original.Copy(); copy.Generation=runtime.Generation; return copy;
            }
            Pending Navigation(AlgorithmAdapterNavigationSnapshot row, bool active)
            {
                if(row==null || row.Task==0 || !KnownTask(new PersistentId(row.Task),true) ||
                    row.Trigger?.TaskId!=0 && row.Trigger?.TaskId!=row.Task || !Finite(row.Position) ||
                    row.FacingYaw.HasValue && (float.IsNaN(row.FacingYaw.Value)||float.IsInfinity(row.FacingYaw.Value)) ||
                    row.TracksTransport && transport==null || _navigation==null ||
                    active && (!_navigation.IsActive || _navigation.CurrentTaskId.Value!=row.Task)) return null;
                var trigger=Trigger(row.Trigger,row.Node);
                if(trigger==null || !_runtimes[trigger.InstanceId].CopyApplied().Nodes.Exists(n=>n.Id==row.Node && n.Kind==AlgorithmNodeKind.Navigate)) return null;
                return new Pending { Trigger=trigger, NodeId=row.Node, Task=new PersistentId(row.Task), Position=row.Position,
                    FacingYaw=row.FacingYaw, Transport=row.TracksTransport ? transport : null };
            }
            var active=snapshot.ActiveNavigation==null ? null : Navigation(snapshot.ActiveNavigation,true);
            if(snapshot.ActiveNavigation!=null && active==null) return false;
            var waiting=new List<Pending>();
            foreach(var row in snapshot.WaitingNavigation) { var value=Navigation(row,false); if(value==null) return false; waiting.Add(value); }
            var effectors=new List<EffectorPending>(); var behaviorIds=new HashSet<PersistentId>();
            foreach(var row in snapshot.Effectors)
            {
                if(row==null || row.Task==0 || row.Behavior==0 || !KnownTask(new PersistentId(row.Task),true) ||
                    !behaviorIds.Add(new PersistentId(row.Behavior)) || !_effectors.TryGetValue(new PersistentId(row.Component),out var queue) ||
                    !_effectorGenerations.TryGetValue(new PersistentId(row.Component),out var generation) || generation!=row.ComponentGeneration ||
                    !queue.TryFindPersistentBehavior(new PersistentId(row.Behavior),out var request) || request.TaskId.Value!=row.Task ||
                    request.AlgorithmId.Value!=row.Trigger?.InstanceId || request.NodeId.Value!=row.Node ||
                    row.Trigger?.TaskId!=0 && row.Trigger?.TaskId!=row.Task) return false;
                var trigger=Trigger(row.Trigger,row.Node);
                if(trigger==null || !_runtimes[trigger.InstanceId].CopyApplied().Nodes.Exists(n=>n.Id==row.Node && n.Kind==AlgorithmNodeKind.Effector)) return false;
                effectors.Add(new EffectorPending { Trigger=trigger, NodeId=row.Node, Task=new PersistentId(row.Task), Behavior=request.Id });
            }
            foreach(var queue in _effectors.Values)
            {
                if(!queue.TryCapturePersistent(EffectorParameterSnapshot.Copy,out var state)) return false;
                if(state.Current!=null && state.Current.AlgorithmId.IsValid && !behaviorIds.Contains(state.Current.Id)) return false;
                foreach(var request in state.Waiting) if(request.AlgorithmId.IsValid && !behaviorIds.Contains(request.Id)) return false;
            }
            var results=new List<PendingResult>();
            var ports=new HashSet<string>(StringComparer.Ordinal) { "accepted","started","completed","cancelled","partial","preempted","targetInvalid","failed","rejected" };
            foreach(var row in snapshot.Results)
            {
                if(row==null || row.Trigger==null || row.Trigger.TaskId!=row.Task || row.Task!=0 && !KnownTask(new PersistentId(row.Task),false) ||
                    !ports.Contains(row.Trigger.Port)) return false;
                var trigger=Trigger(row.Trigger,row.Trigger.NodeId); if(trigger==null) return false;
                results.Add(new PendingResult { Runtime=_runtimes[trigger.InstanceId], Trigger=trigger, Task=new PersistentId(row.Task) });
            }
            _active=active; _waiting.AddRange(waiting); _effectorPending.AddRange(effectors);
            foreach(var value in results) _results.Enqueue(value);
            foreach(var pair in owned) { _ownedTasks.Add(pair.Key); _taskOwners.Add(pair.Key,pair.Value); }
            _now=now; LastCommandUnavailableReason=snapshot.LastUnavailableReason;
            return true;
        }

        private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }
}
