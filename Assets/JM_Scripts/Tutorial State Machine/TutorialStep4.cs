using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DialogueSystem;

public class TutorialStep4 : TutorialState
{
    private bool isFading = false;
    public TutorialStep4(TutorialController controller) : base(controller){}

    public override void Enter()
    {
        
    }

    public override void Tick()
    {
        if(DialogueTemplateInk.Instance.IsDialogueActive) return;
        if(controller.capybara2 != null) return;

        if(!DialogueTemplateInk.Instance.IsDialogueActive && controller.textNumber > 5)
        {
            Image spotlightImage = controller.spotlightScreen.GetComponent<Image>();
            Color initialColor = spotlightImage.color;
            RectTransform buttonTransform = controller.buttonContainer.GetComponent<RectTransform>();

            if(controller.textNumber > 6)
            {
                DOTween.Kill(controller.moneyBar);
                if(isFading) return;

                isFading = true;
                spotlightImage.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    DOTween.Kill(buttonTransform);
                    controller.spotlightScreen.SetActive(false);

                    controller.stepInAction = false;
                    controller.ChangeState(new TutorialEnd(controller));
                });
            }
            else
            {
                initialColor.a = 0f; 
                spotlightImage.color = initialColor;

                //destaque pra barra de laranjas
                controller.spotlightScreen.transform.SetAsLastSibling();
                controller.buttonContainer.transform.SetAsLastSibling();
                controller.moneyBar.transform.SetAsFirstSibling();

                controller.shopButton.SetActive(true);

                spotlightImage.DOFade(0.8f, 0.5f);
                
                buttonTransform.DOScale(1.3f, 0.8f).SetLoops(-1, LoopType.Yoyo);

                controller.spotlightScreen.SetActive(true);

                DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]);// 6 //explica sobre a loja
            }
        }


        DialogueTemplateInk.Instance.StartDialogue(controller.inkFiles[controller.textNumber++]); // 5 // comenta sobre a fusao

    }

    public override void Exit()
    {
        
    }

}
