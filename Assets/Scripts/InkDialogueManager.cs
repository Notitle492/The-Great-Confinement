using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class InkDialogueManager : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSONAsset;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image portraitImage;

    [Header("Character Info")] 
    [SerializeField] private Sprite defaultPortrait;
    [SerializeField] private string defaultName = "???";

    private Story currentStory;
    private List<Button> choiceButtons = new List<Button>();

    private void Start()
    {
        if (inkJSONAsset != null)
        {
            StartStory();
        }
    }

    public void StartStory()
    {
        currentStory = new Story(inkJSONAsset.text);
        ContinueStory();
    }

    public void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            string text = currentStory.Continue();
            text = text.Trim();

            dialogueText.text = text;

            HandleTags(currentStory.currentTags);
            DisplayChoices();
        }
        else
        {
            EndStory();
        }
    }

    void HandleTags(List<string> tags)
    {
        string characterName = defaultName;
        Sprite portrait = null;
        bool portraitTagFound = false;

        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogWarning("Tag could not be parsed: " + tag);
                continue;
            }

            string key = splitTag[0].Trim().ToLower();
            string value = splitTag[1].Trim();

            switch (key)
            {
                case "name":
                    characterName = value;
                    break;
                case "portrait":
                    portraitTagFound = true;

                    if (value.ToLower() == "clear")
                    {
                        portrait = null;
                    }
                    else
                    {
                        Sprite loadedSprite = Resources.Load<Sprite>("Portraits/" + value);
                        if (loadedSprite != null)
                        {
                            portrait = loadedSprite;
                        }
                        else
                        {
                            Debug.LogWarning("無法找到對應頭像圖：" + value + "，請檢查 Resources/Portraits/ 裡有沒有這張圖");
                        }
                    }
                    break;
                default:
                    Debug.Log("未處理的 tag：" + tag);
                    break;
            }
        }

        // 更新 UI
        nameText.text = characterName;
        
         if (portraitTagFound)
        {
            portraitImage.sprite = portrait;
            portraitImage.color = portrait == null ? new Color(1, 1, 1, 0) : Color.white;
        }
        // 🟨 若一開始沒標記 portrait，就維持原本預設的 defaultPortrait 圖（第一次顯示用）
        else if (portraitImage.sprite == null && defaultPortrait != null)
        {
            portraitImage.sprite = defaultPortrait;
            portraitImage.color = Color.white;
        }
    }

    void DisplayChoices()
    {
        // Remove old buttons
        foreach (var btn in choiceButtons)
        {
            Destroy(btn.gameObject);
        }
        choiceButtons.Clear();

        List<Choice> choices = currentStory.currentChoices;
        for (int i = 0; i < choices.Count; i++)
        {
            Choice choice = choices[i];
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            /* Text choiceText = choiceButton.GetComponentInChildren<Text>(); */
            TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            choiceText.text = choice.text.Trim();

            int choiceIndex = i; // Cache to avoid closure issue
            choiceButton.onClick.AddListener(() => MakeChoice(choiceIndex));
            choiceButtons.Add(choiceButton);
        }
    }

    void MakeChoice(int index)
    {
        currentStory.ChooseChoiceIndex(index);
        ContinueStory();
    }

    void EndStory()
    {
        dialogueText.text = "(End of dialogue)";
        nameText.text = "";
        portraitImage.sprite = null;
        portraitImage.color = new Color(1, 1, 1, 0); // 設為透明
        foreach (var btn in choiceButtons)
        {
            Destroy(btn.gameObject);
        }
        choiceButtons.Clear();
    }

    // Optional: external functions, save/load can be added later
}
