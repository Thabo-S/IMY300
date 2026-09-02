using System.Collections;
using UnityEngine;

public class AlertState : BaseState
{
    public Vector3 lastKnownPosition;
    public float alertSpeed = 3f;
    public float waitAtLocationTime = 10f;
    public float pauseAtEachPoint = 1f;

    [Header("Alert / Search")]

    private bool hasArrived = false;
    private bool isPausedAtPoint = false;

    // Exposed for gizmo drawing
    public Vector3 SearchOrigin { get; private set; }
    public Vector3 CurrentWanderTarget { get; private set; }
    public bool HasArrived => hasArrived;

    public override void Enter()
    {
        guard.Agent.speed = alertSpeed;
        guard.Agent.SetDestination(lastKnownPosition);
        hasArrived = false;
    }

    public override void Perform()
    {
        if (guard.CanSeePlayer())
        {
            stateMachine.ChangeState(new AttackState());
            Debug.Log("[ALERT] Spotted player, changing to ATTACK State");
            return;
        }

        if (!hasArrived)
        {
            if (guard.Agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
            {
                Debug.Log("[ALERT] Pathing failed, skipping to Patrol");
                stateMachine.ChangeState(new PatrolState());
                return;
            }

            bool arrived = !guard.Agent.pathPending
                && guard.Agent.remainingDistance <= guard.Agent.stoppingDistance;

            if (arrived)
            {
                hasArrived = true;
                SearchOrigin = guard.transform.position;
                stateMachine.StartCoroutine(SearchAtLocation());
            }
        }

        bool isMoving = guard.Agent.velocity.magnitude > 0.1f && !isPausedAtPoint;
        guard.UpdateAnimationParameters(isMoving, !isMoving);
    }

    private IEnumerator SearchAtLocation()
    {
        float elapsed = 0f;

        while (elapsed < waitAtLocationTime)
        {
            if (stateMachine.activeState != this) yield break;

            if (guard.CanSeePlayer())
            {
                stateMachine.ChangeState(new AttackState());
                Debug.Log("[ALERT] Spotted player while searching, changing to ATTACK State");
                yield break;
            }

            if (!isPausedAtPoint && !guard.Agent.pathPending && guard.Agent.remainingDistance <= guard.Agent.stoppingDistance)
            {
                // Just arrived at a wander point - pause briefly before picking the next one
                isPausedAtPoint = true;
                float pauseTimer = 0f;

                while (pauseTimer < pauseAtEachPoint)
                {
                    if (stateMachine.activeState != this) yield break;

                    if (guard.CanSeePlayer())
                    {
                        stateMachine.ChangeState(new AttackState());
                        Debug.Log("[ALERT] Spotted player during pause, changing to ATTACK State");
                        yield break;
                    }

                    pauseTimer += Time.deltaTime;
                    elapsed += Time.deltaTime;

                    if (elapsed >= waitAtLocationTime) break;

                    yield return null;
                }

                isPausedAtPoint = false;

                Vector3 randomOffset = Random.insideUnitSphere * guard.wanderRadius;
                randomOffset.y = 0f;
                CurrentWanderTarget = SearchOrigin + randomOffset;
                guard.Agent.SetDestination(CurrentWanderTarget);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (stateMachine.activeState != this) yield break;

        stateMachine.ChangeState(new PatrolState());
        Debug.Log("[ALERT] Search finished, didn't find player, returning to Patrol");
    }

    public override void Exit()
    {
        guard.detection = 0f;
    }
}