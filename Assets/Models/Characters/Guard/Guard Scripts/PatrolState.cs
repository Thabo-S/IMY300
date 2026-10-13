using System.Collections;
using UnityEngine;

public class PatrolState : BaseState
{
    public float wayPointInterval = 4f;
    private bool isWaiting = false;

    // Cached tutorial references.
    // These replace FindAnyObjectByType calls from Perform().
    private Step3Trigger step3Trigger;
    private Step3Trigger_2 step3Trigger2;
    private TutorialManager tutorial;

    // Cache the current level once instead of reading PlayerPrefs
    // repeatedly during Perform().
    private bool isTutorialLevel;

    public override void Enter()
    {
        guard.SetVocalState(Guard.GuardVocalState.Patrolling);

        // Cache tutorial references once when entering PatrolState.
        isTutorialLevel = PlayerPrefs.GetInt("currentLevelPrefKey", 0) == 0;

        if (isTutorialLevel)
        {
            step3Trigger = Object.FindAnyObjectByType<Step3Trigger>();
            step3Trigger2 = Object.FindAnyObjectByType<Step3Trigger_2>();
            tutorial = Object.FindAnyObjectByType<TutorialManager>();
        }

        if (!guard.Agent.isOnNavMesh)
        {
            Debug.LogWarning(
                $"[PATROL] {guard.gameObject.name}'s NavMeshAgent is not on a baked NavMesh " +
                "at Enter() - skipping movement setup. Check the guard's spawn position and " +
                "that the NavMesh is baked under it."
            );

            return;
        }

        guard.Agent.isStopped = false;

        if (guard.path != null &&
            guard.path.waypoints.Count > 0)
        {
            guard.Agent.SetDestination(
                guard.path.waypoints[guard.currentWaypointIndex].position
            );
        }
    }

    public override void Perform()
    {
        PatrolCycle();

        bool isMoving =
            guard.Agent.velocity.magnitude > 0.1f &&
            !isWaiting;

        guard.UpdateAnimationParameters(
            isMoving,
            isWaiting
        );

        if (guard.CanSeePlayer())
        {
            stateMachine.ChangeState(new AttackState());

            Debug.Log(
                "[PATROL] Spotted player, changing to ATTACK State"
            );

            return;
        }

        if (guard.TickDetection())
        {
            // Tutorial detection logic.
            // No scene-wide searches happen here anymore.
            if (isTutorialLevel)
            {
                if (step3Trigger != null &&
                    !step3Trigger.isTriggered)
                {
                    if (tutorial != null)
                    {
                        // tutorial.PlayerWarning.SetActive(true);

                        tutorial.PlayerFailedStep2();

                        Debug.Log(
                            "[PATROL] Heard player during tutorial, " +
                            "failed Sound Awareness Test!"
                        );
                    }

                    return;
                }

                if (step3Trigger2 != null &&
                    !step3Trigger2.isTriggered)
                {
                    return;
                }
            }

            AlertState alert = new AlertState();
            alert.lastKnownPosition =
                guard.LastKnownPlayerPosition;

            stateMachine.ChangeState(alert);

            Debug.Log(
                "[PATROL] Heard player, changing to ALERT State"
            );
        }
    }

    public override void Exit()
    {
    }

    public void PatrolCycle()
    {
        if (guard.path == null ||
            guard.path.waypoints.Count == 0)
        {
            return;
        }

        if (!isWaiting &&
            guard.Agent.hasPath &&
            guard.Agent.remainingDistance < 2f)
        {
            stateMachine.StartCoroutine(
                WaitAtWaypoint()
            );
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        guard.currentWaypointIndex =
            (guard.currentWaypointIndex + 1) %
            guard.path.waypoints.Count;

        yield return new WaitForSeconds(
            wayPointInterval
        );

        guard.Agent.SetDestination(
            guard.path.waypoints[
                guard.currentWaypointIndex
            ].position
        );

        isWaiting = false;
    }
}
