using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;


public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Character Info")]
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private string defaultName = "???";


    [SerializeField] private IconFader iconFader;


    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }

    private List<Button> choiceButtons = new List<Button>();

    private static DialogueManager instance;

    private GameObject currentSpeaker;
    public void StartDialogue(TextAsset inkJSON, GameObject speaker)
{
    currentSpeaker = speaker;
    currentStory = new Story(inkJSON.text);
    dialogueIsPlaying = true;
    dialogueCanvas.SetActive(true);
    ContinueStory();
}


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    public bool IsDialoguePlaying()
    {
        return dialogueIsPlaying;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialogueCanvas.SetActive(false);
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
        dialogueCanvas.SetActive(true);
        ContinueStory();
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialogueCanvas.SetActive(false);
        dialogueText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0);
        ClearChoices();


        // 🔽 嘗試取得 CanvasGroup 並停用互動（這不是必要，但可強化穩定性）
        if (dialogueCanvas.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 🔽 在對話結束後觸發圖示淡入
        if (iconFader != null)
        {
            iconFader.FadeIn(); // ✅ 淡入圖示
        }
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

        List<Choice> currentChoices = currentStory.currentChoices;
        if (currentChoices.Count > 0)
        {
            choicesPanel.SetActive(true);
            foreach (Choice choice in currentChoices)
            {
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                choiceText.text = choice.text.Trim();

                int choiceIndex = choice.index;
                choiceButton.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                choiceButtons.Add(choiceButton);
            }

            /* // ✅ 自動選中第一個選項
            if (choiceButtons.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(null); // 清除之前的選擇
                EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
            } */
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

    IEnumerator ShowIconWithDelay()
    {
        yield return new WaitForSeconds(1f);
        iconObject.SetActive(true);
    }


    if (!story.canContinue && !story.currentChoices.Any())
    {
        // 通知 NPC
        currentTalker?.GetComponent<NPCIconTrigger>()?.OnDialogueEnded();
    }

    // 你從外部呼叫 DialogueManager 的時候
    DialogueManager.Instance.StartDialogue(Test_monsterInkJSON, Monster); // 這裡的 gameObject 是某個 NPC



}
