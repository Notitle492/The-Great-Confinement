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
    [Tooltip("對話結束後是否讓物件消失")]
    public bool disappearAfterDialogue = false;   

    [Header("多次對話設定")]
    [Tooltip("是否支援多次對話（不同內容）")]
    public bool supportMultipleDialogues = false;
    
    [Tooltip("第二次對話的 Knot 名稱")]
    public string secondDialogueKnot = "";
    
    [Tooltip("第一次對話的 Knot 名稱（預設為 Chapter1）")]
    public string firstDialogueKnot = "Chapter1";

    private bool hasTalked = false;
    private bool playerInRange;

    private int interactCount = 0;

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

            /* if (InputManager.GetInstance() != null && InputManager.GetInstance().GetInteractPressed()) // 你可以改成自己的輸入方式
            {   
                DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject);
            } */
                /* DialogueManager.GetInstance().EnterDialogueMode(inkJSON); */
                // 之後你可以呼叫 InkDialogueManager 來啟動對話
            if (playerInRange && InputManager.GetInstance().GetInteractPressed())
            {
                if (!DialogueManager.GetInstance().dialogueIsPlaying)
                {
                    Interact();
                }
            }
            
        }
        else
        {
            if (visualCue != null)
                visualCue.SetActive(false);
        }
    }

    private void Interact()
    {
        if (!playerInRange || DialogueManager.GetInstance().dialogueIsPlaying)
            return;

        interactCount++; // 每次互動+1

        string knotToPlay = firstDialogueKnot; 

        // 如果支援多次對話

        if (supportMultipleDialogues && !string.IsNullOrEmpty(secondDialogueKnot))
        {
            if (interactCount == 1)
            {
                knotToPlay = firstDialogueKnot;
                Debug.Log($"[{gameObject.name}] 第 {interactCount} 次互動，播放: {knotToPlay}");
            }
            else
            {
                knotToPlay = secondDialogueKnot;
                Debug.Log($"[{gameObject.name}] 第 {interactCount} 次互動，播放: {knotToPlay}");
            }
        }

        DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject, knotToPlay);
        
        /* // 如果是 NPC2，根據互動次數決定播放哪個 knot
        if (gameObject.name == "npc2") // 或者用一個 public string npcID 來判斷
        {

            if (interactCount == 1)
                knotToPlay = "Chapter1"; // 第一次互動
            else
                knotToPlay = "npc2_second_time"; // 第二次以後都播放第二句
        } */


        /* if (!playerInRange)
        return;

        if (DialogueManager.GetInstance().dialogueIsPlaying)
            return;

        interactCount++;

        string knotToPlay = "Chapter1"; // 預設

        // NPC2 特殊處理
        if (gameObject.name == "npc2")
        {
            knotToPlay = interactCount == 1 ? "Chapter1" : "npc2_second_time";
            Debug.Log("npc2 interactCount: " + interactCount + ", knotToPlay: " + knotToPlay, this);
        } */

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
        if (!giveIconAfterDialogue) return; // 只管圖示生成

        // 每個對話只生成一次圖示
        if (!hasTalked)
        {
            IconData icon = new IconData(
                IconType.Dialogue,
                ItemImage,
                ItemID,
                ItemName
            );
            IconManager.Instance?.AddIcon(icon);
            hasTalked = true;
        }
    }
}
