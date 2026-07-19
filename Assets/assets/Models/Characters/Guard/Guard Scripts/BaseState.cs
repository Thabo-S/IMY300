using UnityEngine;
using Assets.Scripts.Guardscripts; // for Guard

public abstract class BaseState
{
    public Guard guard;
    public StateMachine stateMachine;
    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}

// Standing -> (standTime elapses) -> Patrol
// Patrol -> (reaches a waypoint) -> Standing
// Patrol/Standing -> (spotted or heard player) -> Alert
// Alert -> (reached last-known-pos AND can see player) -> Attack
// Alert -> (reached last-known-pos AND cannot see player) -> Patrol
// Attack -> (loses sight, 5s timer expires) -> Alert
// Attack -> (regains sight during the 5s window) -> Attack (reset timer)

//=====================================================================



// Patrol--(detection maxed via sight / sound)-- > Alert

// Alert--(reached last - known - pos AND can see player)-- > Attack

// Alert--(reached last - known - pos AND cannot see player)-- > Patrol

// Attack--(loses sight, 5s timer expires)-- > Alert

// Attack--(regains sight during the 5s window)-- > Attack(reset timer)
