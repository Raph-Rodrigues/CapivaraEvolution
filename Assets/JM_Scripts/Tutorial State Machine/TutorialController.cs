using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DialogueSystem;
using Unity.VisualScripting;

public class TutorialController : MonoBehaviour
{
    [Header("Objetos a serem sinalizados")]
    public TutorialController controllerItself;
    public CapivaraBoxDrops dropScript; //para desativar e evitar capivaras a mais no tutorial
    public RectTransform moneyBar;
    public GameObject buttonContainer;
    public GameObject shopButton;
    public GameObject spotlightScreen;
    public GameObject capybara1;
    public GameObject capybara2;
    
    [Header("Textos do tutorial")]
    public List<TextAsset> inkFiles;
    private TutorialState currentState;

    [Header("Ignorar - Controladores do Tutorial")]
    public int textNumber = 0;
    public bool stepInAction = false;

    private void Start()
    {
        if(PlayerPrefs.GetInt("TutorialVisto", 0) != 0) 
        {
            controllerItself.enabled = false;
            return;
        }
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
    }

    void OnDisable()
    {
        CapybaraEvent.OnCapybaraCreation -= CapybaraFulfiller;
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

}
