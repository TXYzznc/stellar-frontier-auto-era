using System;
using UnityEngine;
using UnityEngine.AI;

namespace AutoEra.Machines
{
    /// <summary>Uses the installed native AI module; owns neither task state nor compute authority.</summary>
    public sealed class UnityMachineNavigationDriver : IMachineNavigationDriver
    {
        private readonly NavMeshAgent _agent;
        private readonly NavMeshPath _path = new NavMeshPath();
        private readonly Vector3[] _corners = new Vector3[256];
        private readonly float _sampleDistance;
        public Vector3 Position => _agent.transform.position;
        public float Yaw => _agent.transform.eulerAngles.y;
        public float Speed => _agent.velocity.magnitude;
        public bool PathValid => _agent != null && _agent.enabled && _agent.isOnNavMesh &&
            !_agent.isPathStale && !_agent.pathPending && _agent.hasPath && _agent.pathStatus == NavMeshPathStatus.PathComplete;

        public UnityMachineNavigationDriver(NavMeshAgent agent, MachineNavigationSettings settings, float radius,
            float height, int avoidancePriority, float sampleDistance = 0.25f)
        {
            _agent = agent != null ? agent : throw new ArgumentNullException(nameof(agent));
            if (settings == null) throw new ArgumentNullException(nameof(settings)); settings.Validate();
            if (!MachineNavigationSettings.Positive(radius) || !MachineNavigationSettings.Positive(height) ||
                !MachineNavigationSettings.Positive(sampleDistance) || avoidancePriority < 0 || avoidancePriority > 99)
                throw new ArgumentOutOfRangeException(nameof(radius));
            _sampleDistance = sampleDistance;
            agent.speed = settings.Speed; agent.acceleration = settings.Acceleration; agent.angularSpeed = settings.AngularSpeed;
            agent.radius = radius; agent.height = height; agent.stoppingDistance = settings.ArrivalDistance * 0.5f;
            agent.autoRepath = false; agent.autoBraking = true; agent.autoTraverseOffMeshLink = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = avoidancePriority; Stop();
        }
        public bool TryPlan(MachineNavigationTarget target, MachineInstance machine, out Vector3 destination)
        {
            destination = default;
            if (!_agent.enabled || !_agent.isOnNavMesh) return false;
            Stop();
            var filter = new NavMeshQueryFilter { agentTypeID = _agent.agentTypeID, areaMask = _agent.areaMask };
            float best = float.PositiveInfinity;
            foreach (var candidate in target.Candidates)
            {
                if (!NavMesh.SamplePosition(candidate, out var hit, _sampleDistance, filter) || !target.Allows(machine, hit.position) ||
                    !_agent.CalculatePath(hit.position, _path) || _path.status != NavMeshPathStatus.PathComplete) continue;
                int count = _path.GetCornersNonAlloc(_corners);
                if (count < 1 || count >= _corners.Length) continue;
                float length = 0;
                for (int i = 1; i < count; i++) length += Vector3.Distance(_corners[i - 1], _corners[i]);
                if (length >= best) continue;
                best = length; destination = hit.position;
            }
            return !float.IsPositiveInfinity(best) && _agent.CalculatePath(destination, _path) &&
                _path.status == NavMeshPathStatus.PathComplete && _agent.SetPath(_path);
        }
        public void Resume() { if (_agent.enabled && _agent.isOnNavMesh) _agent.isStopped = false; }
        public void Stop()
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
            // Emergency stop is immediate (within the 0.5 s cancellation ceiling).
            _agent.isStopped = true; _agent.velocity = Vector3.zero;
        }
        public void Align(float yaw, float degreesPerSecond, float deltaSeconds)
        {
            _agent.transform.rotation = Quaternion.RotateTowards(_agent.transform.rotation,
                Quaternion.Euler(0, yaw, 0), degreesPerSecond * deltaSeconds);
        }
    }
}
