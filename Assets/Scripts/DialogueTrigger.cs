using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 確保引入

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    [Header("圖示相關")]

    public Sprite ItemImage; // 對話圖示圖
    public string ItemID; // 唯一ID
    public string ItemName;

    [Tooltip("是否在對話結束後給圖示")]
    public bool giveIconAfterDialogue = false;  // ✅ 新增開關

    private bool hasTalked = false;
    private bool playerInRange;

    private void Awake()
    {
        playerInRange = false;
        if (visualCue != null)
            visualCue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            if (visualCue != null)
                visualCue.SetActive(true);

            if (InputManager.GetInstance() != null && InputManager.GetInstance().GetInteractPressed()) // 你可以改成自己的輸入方式
            {   
                DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject);
                /* DialogueManager.GetInstance().EnterDialogueMode(inkJSON); */
                // 之後你可以呼叫 InkDialogueManager 來啟動對話
            }
            
        }
        else
        {
            if (visualCue != null)
                visualCue.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void OnDialogueEnded()
    {
        if (hasTalked || !giveIconAfterDialogue) return; // ✅ 只處理指定的 NPC

        IconData icon = new IconData(
            IconType.Dialogue,
            ItemImage,
            ItemID,
            ItemName
        );
        
        if (IconManager.Instance != null)
            IconManager.Instance.AddIcon(icon);
        else
            Debug.LogWarning("DialogueTrigger: 找不到 IconManager");

        hasTalked = true;  
    }
}
