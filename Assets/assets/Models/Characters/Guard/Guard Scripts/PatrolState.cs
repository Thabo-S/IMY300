using System.Collections;
using UnityEngine;

public class PatrolState : BaseState
{
    public float wayPointInterval = 4f;
    private bool isWaiting = false;

    public override void Enter()
    {
        guard.Agent.isStopped = false;

        if (guard.path != null && guard.path.waypoints.Count > 0)
        {
            guard.Agent.SetDestination(guard.path.waypoints[guard.currentWaypointIndex].position);
        }

    }

    public override void Perform()
    {
        PatrolCycle();

        bool isMoving = guard.Agent.velocity.magnitude > 0.1f && !isWaiting;
        guard.UpdateAnimationParameters(isMoving, isWaiting);

        if (guard.CanSeePlayer())
        {
            stateMachine.ChangeState(new AttackState());
            Debug.Log("[PATROL] Spotted player, changing to ATTACK State");
            return;
        }

        if (guard.TickDetection())
        {

            if (PlayerPrefs.GetInt("LevelIndex", 0) == 0 && !Object.FindObjectOfType<Step4Trigger>().isTriggered)
            {
                TutorialManager tutorial = Object.FindObjectOfType<TutorialManager>();
                if (tutorial != null)
                {
                    tutorial.PlayerFailedStep3();
                    Debug.Log("[PATROL] Heard player during tutorial, calling PlayerFailedStep3");
                }
                return;
            }

            AlertState alert = new AlertState();
            alert.lastKnownPosition = guard.LastKnownPlayerPosition;
            stateMachine.ChangeState(alert);
            Debug.Log("[PATROL] Heard player, changing to ALERT State");
        }
    }

    public override void Exit() { }

    public void PatrolCycle()
    {
        if (guard.path == null || guard.path.waypoints.Count == 0) return;

        if (!isWaiting && guard.Agent.hasPath && guard.Agent.remainingDistance < 2f)
        {
            stateMachine.StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        guard.currentWaypointIndex = (guard.currentWaypointIndex + 1) % guard.path.waypoints.Count;
        yield return new WaitForSeconds(wayPointInterval);
        guard.Agent.SetDestination(guard.path.waypoints[guard.currentWaypointIndex].position);
        isWaiting = false;
    }
}