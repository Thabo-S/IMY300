using Assets.Scripts.Guardscripts;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    //public PatrolState patrolState;

    public void Initialise()
    {
        ChangeState(new PatrolState ());
    }
    void Start()
    {
        //nothing
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

        if(activeState != null)
        {
            activeState.stateMachine = this;
            activeState.guard = GetComponent<Guard>();
            activeState.Enter();
        }
 
    }
}
