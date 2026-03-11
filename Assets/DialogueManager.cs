using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;


public class DialogueManager : MonoBehaviour
{

    [Header("Params")] // [新增] 控制打字速度
    [SerializeField] private float typingSpeed = 0.07f;

    [SerializeField] private AudioClip typingSound; // [新增] 讓你可以自行選擇音效檔案
    [Range(1, 5)]
    [SerializeField] private int soundFrequency = 1; // [新增] 控制音效頻率（每隔幾個字響一次，避免太吵）


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

    // [新增] 用於控制打字狀態與跳過動畫
    private Coroutine displayLineCoroutine;
    private bool canContinueToNextLine = false;


    private List<Button> choiceButtons = new List<Button>();

    private static DialogueManager instance;

    private GameObject currentSpeaker;

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

        // [修改] 只有在「文字打完了」且「沒有選項」時，按下 Submit 才會進入下一句
        if (canContinueToNextLine
            && currentStory.currentChoices.Count == 0
            && InputManager.GetInstance().GetSubmitPressed())
        {
            ContinueStory();
        }
    }

    /// 開始對話（支援指定 Knot）
    public void StartDialogue(TextAsset inkJSON, GameObject speaker, string knotName = null)
    {
        currentSpeaker = speaker;
        currentStory = new Story(inkJSON.text);

        // 如果指定了 knot，跳轉到該 knot
        if (!string.IsNullOrEmpty(knotName))
        {
            try
            {
                currentStory.ChoosePathString(knotName);
                Debug.Log($"[DialogueManager] 跳轉到 Knot: {knotName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] 找不到 Knot: {knotName}\n錯誤: {e.Message}");
                // 如果找不到 knot，就從頭開始
            }
        }

        dialogueIsPlaying = true;
        dialogueCanvas.SetActive(true);

        // 確保 CanvasGroup 可以互動
        if (dialogueCanvas.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        ContinueStory();
    }

    /// 進入對話模式（舊方法，保留相容性）
    public void EnterDialogueMode(TextAsset inkJSON)
    {
        StartDialogue(inkJSON, null, null);
        /* currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialogueCanvas.SetActive(true);
        ContinueStory(); */
    }

    private void ExitDialogueMode()
    {
        // 如果還在打字就強行結束，先停止協程
        if (displayLineCoroutine != null) StopCoroutine(displayLineCoroutine);

        dialogueIsPlaying = false;
        dialogueCanvas.SetActive(false);
        dialogueText.text = "";
        nameText.text = "";
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0);
        ClearChoices();

        // 在對話結束後觸發圖示淡入
        if (iconFader != null)
        {
            iconFader.FadeIn(); // 淡入圖示
        }


        // 對話結束後，呼叫 NPC 的 OnDialogueEnded()
        if (currentSpeaker != null)
        {
            
            DialogueTrigger dialogueTrigger = currentSpeaker.GetComponent<DialogueTrigger>();
            if (dialogueTrigger != null)
            {
                dialogueTrigger.OnDialogueEnded();

                // 如果需要對話結束後整個父物件消失
                if (dialogueTrigger.disappearAfterDialogue)
                {
                    Transform parent = dialogueTrigger.transform.parent;
                    if (parent != null)
                        parent.gameObject.SetActive(false);

                }
            }   
        }

        // 清除 speaker
        currentSpeaker = null;
        
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            // [新增] 如果舊的協程還在跑，先把它停掉
            if (displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }

            string text = currentStory.Continue().Trim();
            dialogueText.text = text;
            HandleTags(currentStory.currentTags);

            // [修改] 改為啟動打字機協程，而不是直接賦值
            displayLineCoroutine = StartCoroutine(DisplayLine(text));
        }
        else
        {
            ExitDialogueMode();
        }
    }

    // [新增] 打字機動畫核心協程
    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = line; // 先填入完整文字以便計算
        dialogueText.maxVisibleCharacters = 0; // 但先不顯示任何字

        canContinueToNextLine = false;
        choicesPanel.SetActive(false); // 打字時隱藏選項

        bool isAddingRichTextTag = false;

        foreach (char letter in line.ToCharArray())
        {
            // 如果玩家在打字期間按下 Submit，直接顯示整行 (Skip 功能)
            if (InputManager.GetInstance().GetSubmitPressed())
            {
                dialogueText.maxVisibleCharacters = line.Length;
                break;
            }

            // 處理 Rich Text (如 <b></b>)，避免標籤被逐字拆開顯示
            if (letter == '<' || isAddingRichTextTag)
            {
                isAddingRichTextTag = true;
                if (letter == '>') isAddingRichTextTag = false;
            }
            else
            {
                // [新增] 播放音效邏輯
                PlayTypingSound(dialogueText.maxVisibleCharacters);

                dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 打字結束後的處理
        DisplayChoices();
        canContinueToNextLine = true;
    }

    // [新增] 播放文字音效的方法
    private void PlayTypingSound(int currentDisplayedCharacterCount)
    {
        // 只有在設定了音效，且達到播放頻率時才播放
        if (typingSound != null && currentDisplayedCharacterCount % soundFrequency == 0)
        {
            // 這裡直接調用你原本 SoundManager 的 3D 播放功能（物件位置設在相機或 Manager 物件上）
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound3D(typingSound, transform.position);
            }
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
