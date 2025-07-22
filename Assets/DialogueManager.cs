using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Character Info")]
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private string defaultName = "???";

    private Story currentStory;
    private bool dialogueIsPlaying;
    private List<Button> choiceButtons = new List<Button>();

    private static DialogueManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }

    public bool IsDialoguePlaying()
    {
        return dialogueIsPlaying;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueIsPlaying) return;
        if (InputManager.GetInstance().GetSubmitPressed() && currentStory.currentChoices.Count == 0)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0);
        ClearChoices();
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            string text = currentStory.Continue().Trim();
            dialogueText.text = text;
            HandleTags(currentStory.currentTags);
            DisplayChoices();
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void HandleTags(List<string> tags)
    {
        string characterName = defaultName;
        Sprite portrait = null;
        bool portraitFound = false;

        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2) continue;

            string key = splitTag[0].Trim().ToLower();
            string value = splitTag[1].Trim();

            if (key == "name")
            {
                characterName = value;
            }
            else if (key == "portrait")
            {
                portraitFound = true;
                if (value.ToLower() == "clear")
                {
                    portrait = null;
                }
                else
                {
                    portrait = Resources.Load<Sprite>("Portraits/" + value);
                    if (portrait == null)
                        Debug.LogWarning("找不到圖片：" + value);
                }
            }
        }

        nameText.text = characterName;

        if (portraitFound)
        {
            portraitImage.sprite = portrait;
            portraitImage.color = portrait == null ? new Color(1, 1, 1, 0) : Color.white;
        }
        else if (portraitImage.sprite == null && defaultPortrait != null)
        {
            portraitImage.sprite = defaultPortrait;
            portraitImage.color = Color.white;
        }
    }

    private void DisplayChoices()
    {
        ClearChoices();

        List<Choice> choices = currentStory.currentChoices;
        if (choices.Count > 0)
        {
            choicesPanel.SetActive(true);
            foreach (Choice choice in choices)
            {
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                choiceText.text = choice.text.Trim();

                choiceButton.onClick.AddListener(() => OnChoiceSelected(choice.index));
                choiceButtons.Add(choiceButton);
            }
        }
        else
        {
            choicesPanel.SetActive(false);
        }
    }

    private void ClearChoices()
    {
        foreach (Button button in choiceButtons)
        {
            Destroy(button.gameObject);
        }
        choiceButtons.Clear();
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
}
