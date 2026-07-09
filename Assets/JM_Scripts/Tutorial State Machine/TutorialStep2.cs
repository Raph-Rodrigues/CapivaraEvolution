using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DialogueSystem;
using Unity.VisualScripting;

public class TutorialStep2 : TutorialState
{
    private bool isFading = false;
    public TutorialStep2(TutorialController controller) : base(controller) {}

    public override void Enter()
    {
        
    }

    public override void Tick()
    {
        if(DialogueTemplateInk.Instance.IsDialogueActive) return;
        Debug.Log("Para aqui - Sem laranjas ainda");
        //if(controller.orange == null) return; //espera o spawn da primeira laranja

        if(!controller.stepInAction)
        {
            controller.stepInAction = true;
            DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 2 //texto explicando sobre as capivaras droparem laranjas
            //TutorialCursor.Instance.OutsideCanvasPointer(controller.orange.transform); //aponta pra laranja
        }
        else
        {
            if(!DialogueTemplateInk.Instance.IsDialogueActive)
            {
                Image spotlightImage = controller.spotlightScreen.GetComponent<Image>();
                Color initialColor = spotlightImage.color;
                

                if(controller.textNumber > 3)
                {
                    if(isFading) return;

                    isFading = true;
                    spotlightImage.DOFade(0f, 0.5f).OnComplete(() =>
                    {
                        controller.spotlightScreen.SetActive(false);

                        controller.stepInAction = false;
                        controller.ChangeState(new TutorialStep3(controller));
                    });
                }
                else
                {
                    initialColor.a = 0f; 
                    spotlightImage.color = initialColor;

                    //destaque pra barra de laranjas
                    controller.spotlightScreen.transform.SetAsLastSibling();
                    controller.moneyBar.transform.SetAsLastSibling();
                    controller.shopButton.transform.SetAsFirstSibling();

                    controller.spotlightScreen.SetActive(true);

                    spotlightImage.DOFade(0.8f, 0.5f);

                    DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 3 //texto explicando sobre a barra de laranjas
                }
            }
        }
    }

    public override void Exit()
    {
        
    }
}
