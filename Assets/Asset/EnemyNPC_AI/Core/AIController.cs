using UnityEngine;
using UnityEngine.AI;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class AIController : MonoBehaviour
    {
        public enum AIState { Patrol, Chase, Search, Alert, Follow, Idle, Flee }

        [Header("References")]
        public EnemyAIConfigSO config;
        public Perception perception;
        public Transform player;
        public PatrolPath patrolPath;
        public Transform[] fleePoints;
        public float followdistance;

        private NavMeshAgent agent;
        private Animator animator;

        private int currentWaypointIndex;
        private float reachedThreshold = 1.0f;

        private AIState currentState = AIState.Patrol;
        private float stateTimer = 0f;

        private Vector3 lastKnownPlayerPosition;
        private bool hasLastKnownPosition = false;

        private bool waitingAtWaypoint = false;
        private float waitTimer = 0f;
        private bool isPatrolForward = true;

        private Vector3 searchPosition;
        private Vector3 noiseHeardPosition;
        private bool hasHeardNoise = false;

        private Vector3 currentDestination = Vector3.positiveInfinity;

        [Header("Hit Reaction")]
        public float hitReactionDuration = 0.4f;
        private bool isHitReacting = false;
        private float hitReactionTimer = 0f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            SwitchToConfiguredMode();
        }

        private void Update()
        {
            if (!agent.isOnNavMesh || player == null || config == null || perception == null)
                return;

            if (isHitReacting)
            {
                hitReactionTimer -= Time.deltaTime;
                if (hitReactionTimer <= 0f)
                {
                    isHitReacting = false;
                    agent.isStopped = false;
                }
                return;
            }

            bool playerVisible = perception.CanSeeTarget(player);

            if (IsPathBlocked())
                AttemptReposition();

            switch (currentState)
            {
                case AIState.Patrol:
                    Patrol();
                    if (playerVisible && config.mode == EnemyAIConfigSO.BehaviorMode.Hostile)
                        SwitchState(AIState.Chase);
                    break;

                case AIState.Chase:
                    if (playerVisible)
                    {
                        hasLastKnownPosition = true;
                        lastKnownPlayerPosition = player.position;
                        SetDestinationSmooth(player.position);
                    }
                    else if (hasLastKnownPosition)
                    {
                        SwitchState(AIState.Search);
                    }
                    break;

                case AIState.Search:
                    stateTimer -= Time.deltaTime;

                    if (hasHeardNoise)
                    {
                        HandleSearchNoise();
                        hasHeardNoise = false; // Reset after handling
                    }
                    else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        Vector3 roamPoint = lastKnownPlayerPosition + Random.insideUnitSphere * 5f;
                        roamPoint.y = 0;
                        if (NavMesh.SamplePosition(roamPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                            SetDestinationSmooth(hit.position);
                    }

                    if (playerVisible)
                        SwitchState(AIState.Chase);
                    else if (stateTimer <= 0f)
                        SwitchState(AIState.Alert);
                    break;

                case AIState.Alert:
                    stateTimer -= Time.deltaTime;
                    Patrol();
                    if (playerVisible)
                        SwitchState(AIState.Chase);
                    else if (stateTimer <= 0f)
                        SwitchState(AIState.Patrol);
                    break;

                case AIState.Follow:
                    if (player != null)
                        SetDestinationSmooth(player.position);
                    break;

                case AIState.Idle:
                    agent.isStopped = true;
                    break;

                case AIState.Flee:
                    if (fleePoints != null && fleePoints.Length > 0)
                    {
                        Transform point = fleePoints[Random.Range(0, fleePoints.Length)];
                        SetDestinationSmooth(point.position);
                    }
                    break;
            }

            UpdateAnimation();
        }

        public void OnHit()
        {
            isHitReacting = true;
            hitReactionTimer = hitReactionDuration;
            agent.isStopped = true;
            animator?.SetTrigger("Hit");
        }

        public void SwitchToConfiguredMode()
        {
            switch (config.mode)
            {
                case EnemyAIConfigSO.BehaviorMode.Friendly:
                case EnemyAIConfigSO.BehaviorMode.Follow:
                    SwitchState(AIState.Follow);
                    break;
                case EnemyAIConfigSO.BehaviorMode.Idle:
                    SwitchState(AIState.Idle);
                    break;
                case EnemyAIConfigSO.BehaviorMode.Flee:
                    SwitchState(AIState.Flee);
                    break;
                default:
                    SwitchState(AIState.Patrol);
                    break;
            }
        }

        private void SwitchState(AIState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"🧠 [AIController] Switched to: {newState}", this);

            switch (newState)
            {
                case AIState.Patrol:
                    ApplyVision(config.viewRadius);
                    agent.speed = config.moveSpeed;
                    agent.isStopped = false;
                    break;
                case AIState.Chase:
                    ApplyVision(config.viewRadius);
                    agent.speed = config.chaseSpeed;
                    agent.isStopped = false;
                    break;
                case AIState.Search:
                    ApplyVision(config.searchViewRadius, config.searchViewAngle);
                    stateTimer = config.searchDuration;
                    agent.speed = config.moveSpeed;
                    agent.isStopped = false;
                    break;
                case AIState.Alert:
                    ApplyVision(config.alertViewRadius);
                    stateTimer = config.alertDuration;
                    agent.speed = config.moveSpeed;
                    agent.isStopped = false;
                    break;
                case AIState.Follow:
                    agent.speed = config.moveSpeed;
                    agent.isStopped = false;
                    break;
                case AIState.Idle:
                    agent.isStopped = true;
                    break;
                case AIState.Flee:
                    agent.speed = config.chaseSpeed;
                    agent.isStopped = false;
                    break;
            }

            currentDestination = Vector3.positiveInfinity;
        }

        private void SetDestinationSmooth(Vector3 target)
        {
            if (Vector3.Distance(agent.destination, target) > 0.5f)
            {
                agent.isStopped = false;
                agent.SetDestination(target);
                currentDestination = target;
            }
        }

        private void HandleSearchNoise()
        {
            if (NavMesh.SamplePosition(noiseHeardPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                SetDestinationSmooth(hit.position);
            }
        }
        public void OnNoiseHeard(Vector3 position)
        {
            noiseHeardPosition = position;
            hasHeardNoise = true;

            if (currentState != AIState.Chase) // don't override if already chasing
            {
                lastKnownPlayerPosition = position;
                hasLastKnownPosition = true;
                SwitchState(AIState.Search);
            }
        }

        private void Patrol()
        {
            if (patrolPath == null || patrolPath.WaypointCount == 0 || agent.pathPending) return;

            Transform targetPoint = patrolPath.GetWaypoint(currentWaypointIndex);
            if (targetPoint == null) return;

            float distance = Vector3.Distance(transform.position, targetPoint.position);

            if (distance > reachedThreshold)
            {
                if (!waitingAtWaypoint)
                    SetDestinationSmooth(targetPoint.position);
            }
            else
            {
                if (!waitingAtWaypoint)
                {
                    waitingAtWaypoint = true;
                    waitTimer = config.waitTimeAtWaypoint;
                    agent.isStopped = true;
                }
                else
                {
                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0f)
                    {
                        agent.isStopped = false;
                        waitingAtWaypoint = false;
                        AdvanceToNextWaypoint();
                    }
                }
            }
        }

        private void AdvanceToNextWaypoint()
        {
            switch (config.patrolMode)
            {
                case EnemyAIConfigSO.PatrolMode.Loop:
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolPath.WaypointCount;
                    break;
                case EnemyAIConfigSO.PatrolMode.PingPong:
                    if (isPatrolForward)
                    {
                        currentWaypointIndex++;
                        if (currentWaypointIndex >= patrolPath.WaypointCount - 1)
                        {
                            currentWaypointIndex = patrolPath.WaypointCount - 1;
                            isPatrolForward = false;
                        }
                    }
                    else
                    {
                        currentWaypointIndex--;
                        if (currentWaypointIndex <= 0)
                        {
                            currentWaypointIndex = 0;
                            isPatrolForward = true;
                        }
                    }
                    break;
                case EnemyAIConfigSO.PatrolMode.Random:
                    currentWaypointIndex = Random.Range(0, patrolPath.WaypointCount);
                    break;
            }
        }

        private bool IsPathBlocked()
        {
            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
                return true;

            if (agent.remainingDistance < agent.stoppingDistance)
                return false;

            Vector3 direction = agent.steeringTarget - transform.position;
            if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out RaycastHit hit, 2f))
            {
                if (!hit.collider.isTrigger && hit.collider.gameObject != player.gameObject)
                    return true;
            }
            return false;
        }

        private void AttemptReposition()
        {
            Vector3 offset = Random.insideUnitSphere * 3f;
            offset.y = 0;
            Vector3 candidate = transform.position + offset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log("🔄 [AIController] Path blocked – repositioning...");
            }
        }

        public void ResetToDefaultState()
        {
            if (agent != null)
            {
                agent.ResetPath();
                agent.isStopped = false;
            }

            hasLastKnownPosition = false;
            stateTimer = 0f;
            currentDestination = Vector3.positiveInfinity;
            SwitchToConfiguredMode();
        }

        private void ApplyVision(float radius) => ApplyVision(radius, config.viewAngle);

        private void ApplyVision(float radius, float angle)
        {
            if (perception != null)
            {
                perception.viewRadius = radius;
                perception.viewAngle = angle;
            }
        }

        public void SetState(AIState newState)
        {
            currentState = newState;
        }

        public void SetSearchLocation(Vector3 position)
        {
            searchPosition = position;
        }

        private void UpdateAnimation()
        {
            if (animator != null && agent != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
        }

        public string GetCurrentState() => currentState.ToString();
    }
}