using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DialogueSystem;

public class TutorialController : MonoBehaviour
{
    [Header("Objetos a serem sinalizados")]
    public CapivaraBoxDrops dropScript; //para desativar e evitar capivaras a mais no tutorial
    public FusionManager fusionScript; //para ficar de olho da primeira fusao
    public GameObject moneyBar;
    public Button shopButton;
    public GameObject spotlightScreen;
    public GameObject capybara1;
    public GameObject capybara2;
    public GameObject orange;
    
    [Header("Textos do tutorial")]
    public List<TextAsset> inkFiles;
    private TutorialState currentState;

    [Header("Ignorar - Controladores do Tutorial")]
    public int textNumber = 0;
    public bool stepInAction = false;

    private void Start()
    {
        //dialogo de introducao
        ChangeState(new TutorialStep1(this));
    }

    private void LateUpdate()
    {
        //verifica o se o estado existe antes de chamar o tick
        currentState?.Tick();
    }

    void OnEnable()
    {
        CapybaraEvent.OnCapybaraCreation += CapybaraFulfiller;
        OrangeEvent.OnOrangeCreation += OrangeFulfiller;
    }

    void OnDisable()
    {
        CapybaraEvent.OnCapybaraCreation -= CapybaraFulfiller;
        OrangeEvent.OnOrangeCreation -= OrangeFulfiller;
    }

    //mudanca de estado comum
    public void ChangeState(TutorialState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    //preenche as capivaras usadas no tutorial 
    private void CapybaraFulfiller(GameObject cap)
    {
        if(capybara1 == null)
        {
            capybara1 = cap;
        }
        else if(capybara2 == null)
        {
            capybara2 = cap;
        }
    }

    //laranja usada no tutorial
    private void OrangeFulfiller(GameObject org)
    {
        if(orange == null)
        {
            orange = org;
        }
    }
}
