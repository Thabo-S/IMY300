using UnityEngine;

/// <summary>
/// Tiny static tracker for whole-run mission stats that don't belong to any
/// single object - currently just whether the player was ever detected.
/// Set WasDetected = true from StateMachine.ChangeState() whenever a guard
/// enters AlertState or AttackState (covers sight, sound, laser, and camera
/// detection alike, since all of them funnel through ChangeState).
/// </summary>
public static class MissionStats
{
    public static bool WasDetected = false;

    /// <summary>Call at the start of a level/scene to clear the previous run's state.</summary>
    public static void ResetForNewLevel()
    {
        WasDetected = false;
    }
}