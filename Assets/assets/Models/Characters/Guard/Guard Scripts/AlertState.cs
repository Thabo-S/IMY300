using System.Collections;
using UnityEngine;

public class AlertState : BaseState
{
    public Vector3 lastKnownPosition;
    public float alertSpeed = 16f;
    public float waitAtLocationTime = 2f;

    private bool hasArrived = false;
    private bool isWaiting = false;

    public override void Enter()
    {
        guard.Agent.speed = alertSpeed;
        guard.Agent.SetDestination(lastKnownPosition);
        hasArrived = false;
        isWaiting = false;
    }

    public override void Perform()
    {

        //Debug.Log($"[ALERT] pos:{guard.Agent.transform.position} dest:{guard.Agent.destination} remainingDist:{guard.Agent.remainingDistance} hasPath:{guard.Agent.hasPath} pathPending:{guard.Agent.pathPending} pathStatus:{guard.Agent.pathStatus} hasArrived:{hasArrived}");

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
                stateMachine.StartCoroutine(WaitAtLocation());
            }
        }

        bool isMoving = guard.Agent.velocity.magnitude > 0.1f && !isWaiting;
        guard.UpdateAnimationParameters(isMoving, isWaiting);
    }

    private IEnumerator WaitAtLocation()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtLocationTime);

        if (stateMachine.activeState != this)
        {
            yield break;
        }

        if (guard.CanSeePlayer())
        {
            stateMachine.ChangeState(new AttackState());
            Debug.Log("[ALERT] Spotted player after waiting, changing to ATTACK State");
        }
        else
        {
            stateMachine.ChangeState(new PatrolState());
            Debug.Log("[ALERT] Didn't find player after waiting, changing back to Patrol State");
        }

        isWaiting = false;
    }

    public override void Exit() { }
}