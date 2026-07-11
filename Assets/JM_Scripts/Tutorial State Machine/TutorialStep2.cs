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

        if(!controller.stepInAction)
        {
            controller.stepInAction = true;
            DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 2 //texto explicando sobre as capivaras droparem laranjas
        }
        else
        {
            if(!DialogueTemplateInk.Instance.IsDialogueActive)
            {
                Image spotlightImage = controller.spotlightScreen.GetComponent<Image>();
                Color initialColor = spotlightImage.color;
                

                if(controller.textNumber > 3)
                {
                    DOTween.Kill(controller.moneyBar);
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
                    TutorialCursor.Instance.CursorState(false);
                    initialColor.a = 0f; 
                    spotlightImage.color = initialColor;

                    //destaque pra barra de laranjas
                    controller.spotlightScreen.transform.SetAsLastSibling();
                    controller.moneyBar.transform.SetAsLastSibling();
                    controller.shopButton.transform.SetAsFirstSibling();

                    controller.moneyBar.DOScale(1.3f, 0.8f).SetLoops(-1, LoopType.Yoyo);

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
