using UnityEngine;

public abstract  class BaseState
{
    public Guard guard;
    public StateMachine stateMachine;
    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}

// Patrol--(detection maxed via sight / sound)-- > Alert

// Alert--(reached last - known - pos AND can see player)-- > Attack

// Alert--(reached last - known - pos AND cannot see player)-- > Patrol

// Attack--(loses sight, 5s timer expires)-- > Alert

// Attack--(regains sight during the 5s window)-- > Attack(reset timer)




//=====================================================================



// Patrol--(detection maxed via sight / sound)-- > Alert

// Alert--(reached last - known - pos AND can see player)-- > Attack

// Alert--(reached last - known - pos AND cannot see player)-- > Patrol

// Attack--(loses sight, 5s timer expires)-- > Alert

// Attack--(regains sight during the 5s window)-- > Attack(reset timer)
