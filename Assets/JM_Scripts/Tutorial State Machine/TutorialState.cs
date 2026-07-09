using UnityEngine;

public abstract class TutorialState
{
    protected TutorialController controller;

    public TutorialState(TutorialController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter(){}

    public virtual void Tick(){}

    public virtual void Exit(){}

}
