using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;

namespace DialogueSystem
{
    public class DialogueTemplateInk : MonoBehaviour
    {
        private TextAsset currentInk = null;
        private Story currentStory;
        
        public static DialogueTemplateInk Instance { get; private set; }

        [SerializeField] bool useSimpleDialogue = true;
        [SerializeField] bool useChoiceDialogue = true;

        [Header("Simple Dialogue")]
        [SerializeField] GameObject simpleTextBox;
        [SerializeField] TextMeshProUGUI simpleText;
        [SerializeField] Button continueButton;

        [Header ("Choice Dialogue")]
        [SerializeField] GameObject choiceTextBox;
        [SerializeField] GameObject buttonPrefab;
        [SerializeField] TextMeshProUGUI choiceText;
        [SerializeField] Transform buttonContainer;
        [SerializeField] Button choicecontinuebutton;

        [Header ("TypeWriter")]
        private int _currentVisibleCharacterIndex;
        private Coroutine _typewriterCoroutine;
        private WaitForSeconds _simpleDelay;
        private WaitForSeconds _interpunctuationDelay;
        [SerializeField] float characteresPerSecond = 20;
        [SerializeField] float interpunctuationDelay = 0.5f;

        [Header("Skip Dialogue")]
        private bool skip = false;
        private TextMeshProUGUI activeBox = null;
        private List<GameObject> buttonList = new List<GameObject>();
        public static event Action<Story, TextAsset> DialogueStarted;    
        public bool IsDialogueActive { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _simpleDelay = new WaitForSeconds(1f/characteresPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
        }
        void Start()
        {
            if(useSimpleDialogue) continueButton.onClick.AddListener(ContinueButton);
            if(useChoiceDialogue) choicecontinuebutton.onClick.AddListener(SkipDialogue);
        }

        public void SkipDialogue()
        {
            if(activeBox != null && activeBox.maxVisibleCharacters != activeBox.textInfo.characterCount)
            {
                skip = true;
                Debug.Log("Aqui");
            }
        }

        public void ContinueButton()
        {
            if(activeBox != null && activeBox.maxVisibleCharacters != activeBox.textInfo.characterCount)
            {
                skip = true;
                Debug.Log("Aqui");
            }
            else 
            {
                skip = false;
                AdvanceStory();
            }
            
        }

        public void StartDialogue(TextAsset inkJSON)
        {
            skip = false;
            activeBox = null;
            currentInk = inkJSON;
            currentStory = new Story(inkJSON.text);
            IsDialogueActive = true;

            DialogueStarted?.Invoke(currentStory, currentInk);

            AdvanceStory();
        }

        private void AdvanceStory()
        {
            skip = false;

            DialogueCleaner(simpleTextBox, simpleText);
            DialogueCleaner(choiceTextBox, choiceText, true);

            if (currentStory.canContinue)
            {
                string nextLine = currentStory.Continue();
                
                if (useChoiceDialogue && currentStory.currentChoices.Count > 0) ChoiceDialogue(nextLine, currentStory.currentChoices);
                
                else if (useSimpleDialogue) SimpleDialogue(nextLine);

                else Debug.LogWarning("Sem módulo de diálogo ativo");
            }

            else if (useChoiceDialogue && currentStory.currentChoices.Count > 0) ChoiceDialogue("", currentStory.currentChoices);

            else EndDialogue();
        }

        void Update()
        {
            
        }

        public void SimpleDialogue(string dialogue)
        {
            if (!useSimpleDialogue || simpleTextBox == null) return;

            continueButton.interactable = true;
            simpleTextBox.SetActive(true);
            SetText(simpleText, dialogue);
        }

        public void ChoiceDialogue(string dialogue, List<Choice> choices)
        {
            if (!useChoiceDialogue || choiceTextBox == null) return;

            choicecontinuebutton.interactable = true;
            choiceTextBox.SetActive(true);
            SetText(choiceText, dialogue);
            
            for(int i = 0; i < choices.Count; i++)
            {
                int aux = choices[i].index;
                GameObject button = Instantiate(buttonPrefab, buttonContainer);
                buttonList.Add(button);
                Button buttonCmp = button.GetComponent<Button>();

                button.GetComponentInChildren<TextMeshProUGUI>().text = choices[i].text;
                
                buttonCmp.onClick.AddListener(() =>
                {
                    currentStory.ChooseChoiceIndex(aux);
                    AdvanceStory();
                });
            }
        }

        private void EndDialogue()
        {
            IsDialogueActive = false;
            DialogueCleaner(simpleTextBox, simpleText);
            DialogueCleaner(choiceTextBox, choiceText, true);
            if(useChoiceDialogue) choicecontinuebutton.interactable = false;
            if(useSimpleDialogue) continueButton.interactable = false;
            currentInk = null;
            currentStory = null;
            Debug.Log("Dialogo acabou");
        }

        void DialogueCleaner(GameObject box, TextMeshProUGUI text, bool cleanButtons = false)
        {
            if (box == null || text == null) return;

            activeBox = null;
            text.text = "";
            box.SetActive(false);

            if(cleanButtons)
            {
                foreach(GameObject bttn in buttonList) Destroy(bttn);  
                buttonList.Clear();
            }
                
        }

        void OnDestroy()
        {
            if(useSimpleDialogue) continueButton.onClick.RemoveAllListeners();
            if(useChoiceDialogue) choicecontinuebutton.onClick.RemoveAllListeners();
        }

        private void SetText(TextMeshProUGUI dialBox, string text)
        {
            if(_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);

            dialBox.maxVisibleCharacters = 0;
            dialBox.text = text;
            skip = false;
            _currentVisibleCharacterIndex = 0;

            _typewriterCoroutine = StartCoroutine(routine:Typewriter(dialBox));
        }

        private IEnumerator Typewriter(TextMeshProUGUI dialBox)
        {
            activeBox = dialBox;
            dialBox.ForceMeshUpdate();
            int totalCharacters = dialBox.textInfo.characterCount;

            TMP_TextInfo textInfo = dialBox.textInfo;
            while(_currentVisibleCharacterIndex < totalCharacters && !skip)
            {
                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

                _currentVisibleCharacterIndex++;

                dialBox.maxVisibleCharacters = _currentVisibleCharacterIndex;

                if(character == '?'|| character == ',' || character == ':' || character == ';' || character == '!' || character ==  '.')
                {
                    yield return _interpunctuationDelay;
                }
                else
                {
                    yield return _simpleDelay;
                }
            }

            dialBox.maxVisibleCharacters = totalCharacters;
        }

        public Ink.Runtime.Story GetCurrentStory()
        {
            return currentStory;
        }

        public TextAsset GetCurrentAsset()
        {
            return currentInk;
        }


    }
}
