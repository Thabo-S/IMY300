public class PatrolState : BaseState
{
    public int wayPointIndex; 
    public override void Enter()
    {

    }
    public override void Perform()
    {
        PatrolCycle();
    }
    public override void Exit()
    {

    }

    public void PatrolCycle()
    {
        if (guard.Agent.remainingDistance < 2f)
        {
            if (wayPointIndex < guard.path.waypoints.Count - 1)
            {
                wayPointIndex++;
            }
            else
            {
                wayPointIndex = 0;
            }

            guard.Agent.SetDestination(guard.path.waypoints[wayPointIndex].position);
        }
    }
}