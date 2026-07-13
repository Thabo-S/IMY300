using System.Collections;
using UnityEngine;

public class AlertState : BaseState
{
    public Vector3 lastKnownPosition;
    public float alertSpeed = 16f;
    public float waitAtLocationTime = 8f;
    public float wanderRadius = 15f;

    private bool hasArrived = false;

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
                stateMachine.StartCoroutine(SearchAtLocation());
            }
        }

        bool isMoving = guard.Agent.velocity.magnitude > 0.1f;
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

            // Wander to a new nearby point every so often, but stay within the total search window
            if (!guard.Agent.pathPending && guard.Agent.remainingDistance <= guard.Agent.stoppingDistance)
            {
                Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
                randomOffset.y = 0f;
                guard.Agent.SetDestination(guard.transform.position + randomOffset);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (stateMachine.activeState != this) yield break;

        stateMachine.ChangeState(new PatrolState());
        Debug.Log("[ALERT] Search finished, didn't find player, returning to Patrol");
    }

    public override void Exit() { }
}