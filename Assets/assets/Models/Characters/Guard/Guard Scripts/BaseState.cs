public abstract  class BaseState
{
    public Guard guard;
    public StateMachine stateMachine;
    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}