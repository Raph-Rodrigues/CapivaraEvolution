using UnityEngine;
using DialogueSystem;

public class TutorialStep1 : TutorialState
{
    public TutorialStep1(TutorialController controller) : base(controller)
    {
        
    }
    public override void Enter()
    {
        DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 0 //texto inicial abordando q existem capivaras (?)
        controller.dropScript.enabled = false; //trava o drop de capivaras momentaneamente
    }

    public override void Tick()
    {
        if(DialogueTemplateInk.Instance.IsDialogueActive) return;
        if(!controller.dropScript.enabled) controller.dropScript.enabled = true; //ativa o drop para a captura da capivara 1

        if(controller.capybara1 == null) return;
        if(controller.dropScript.enabled) controller.dropScript.enabled = false; //desativa apos a coleta da capivara 1
        
        if(!controller.stepInAction) 
        {
            controller.stepInAction = true;
            TutorialCursor.Instance.OutsideCanvasPointer(controller.capybara1.transform); //cursor aponta para a capivara 1
            DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 1 //texto explicando sobre as capivaras e poder arrastar elas
        }
        else
        {
            if(!DialogueTemplateInk.Instance.IsDialogueActive)
            {
                controller.stepInAction = false;
                controller.ChangeState(new TutorialStep2(controller)); //proximo passo -> laranjas
            }
        }
    }   

    public override void Exit()
    {
        
    }
}
