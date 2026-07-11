using UnityEngine;
using DialogueSystem;

public class TutorialEnd : TutorialState
{
    public TutorialEnd(TutorialController controller) : base(controller) {}

    public override void Enter()
    {
        
    }

    public override void Tick()
    {
        if(DialogueTemplateInk.Instance.IsDialogueActive) return;

        PlayerPrefs.SetInt("TutorialVisto", 1);
        PlayerPrefs.Save();

        controller.dropScript.enabled = true;
        controller.controllerItself.enabled = false;
    }

    public override void Exit()
    {
        
    }
}
