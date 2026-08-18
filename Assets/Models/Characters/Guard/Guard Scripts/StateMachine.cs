//using UnityEngine;

//public class StateMachine : MonoBehaviour
//{
//    public BaseState activeState;
//    //public PatrolState patrolState;

//    public void Initialise()
//    {
//        ChangeState(new PatrolState());
//    }
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (activeState != null)
//        {
//            activeState.Perform();
//        }
//    }

//    public void ChangeState(BaseState newState)
//    {
//        if (activeState != null)
//        {
//            activeState.Exit();
//        }

//        activeState = newState;

//        if(activeState != null)
//        {
//            activeState.stateMachine = this;
//            activeState.guard = GetComponent<Guard>();
//            activeState.Enter();
//        }

//    }
//}

using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    //public PatrolState patrolState;

    public void Initialise()
    {
        ChangeState(new PatrolState());
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (activeState != null)
        {
            activeState.Perform();
        }
    }

    public void ChangeState(BaseState newState)
    {
        if (activeState != null)
        {
            activeState.Exit();
        }

        activeState = newState;

        if (activeState != null)
        {
            activeState.stateMachine = this;
            activeState.guard = GetComponent<Guard>();
            activeState.Enter();
        }

        // Any guard entering Alert or Attack means the player was detected
        // at some point this run - covers sight, sound, laser, and camera
        // triggers alike, since they all route through ChangeState().
        if (newState is AlertState || newState is AttackState)
        {
            MissionStats.WasDetected = true;
        }
    }
}