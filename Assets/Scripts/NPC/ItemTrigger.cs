using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    public Item itemToGive;

    private bool hasGivenItem = false;

    [Header("圖示與音效")]

    public GameObject iconToShow;      // 更通用：要出現的任意圖示
    public AudioClip appearSound;      // 出現時的音效
    private AudioSource audioSource;

    private void Awake()
    {
        // 確保這個物件有 AudioSource 組件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnDialogueEnded()
    {
        if (!hasGivenItem && itemToGive != null)
        {
            InventoryManager.Instance.AddItem(itemToGive);
            hasGivenItem = true;
        }

        if (iconToShow != null && !iconToShow.activeSelf)
        {
            iconToShow.SetActive(true);

            // 播放出現音效
            if (appearSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(appearSound);
            }
        }
    }
}

/* public class MonsterIconTrigger : MonoBehaviour
{
    public GameObject PenAndQuestionIcon; // 要出現的圖示
    private bool hasTalked = false;

    // DialogueManager 呼叫這個方法，代表對話結束
    public void OnDialogueEnded()
    {
        if (!hasTalked)
        {
            PenAndQuestionIcon.SetActive(true);
            hasTalked = true;
        }
    }

    
} */
