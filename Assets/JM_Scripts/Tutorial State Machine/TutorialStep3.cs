using UnityEngine;
using DialogueSystem;

public class TutorialStep3 : TutorialState
{
    public TutorialStep3(TutorialController controller) : base(controller) {}

    public override void Enter()
    {
        controller.dropScript.enabled = true;
    }

    public override void Tick()
    {
        if(controller.capybara2 == null) return;
        else if(controller.dropScript.enabled) controller.dropScript.enabled = false;

        if(DialogueTemplateInk.Instance.IsDialogueActive) return;
        
        DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]); // 4 // texto falando sobre as fusoes

        TutorialCursor.Instance.DragCanvasPointer(controller.capybara1.transform, controller.capybara2.transform);

        controller.ChangeState(new TutorialStep4(controller));
    }

    public override void Exit()
    {
        
    }
}
