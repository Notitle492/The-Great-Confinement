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

        // 建議避免音效自動播放
        audioSource.playOnAwake = false;
    }

    public void OnDialogueEnded()
    {
        // 加入物品邏輯（如有）
        if (!hasGivenItem && itemToGive != null)
        {
            InventoryManager.Instance.AddItem(itemToGive);
            hasGivenItem = true;
        }

        // 顯示圖示（若尚未顯示）
        bool iconJustShown = false;

        if (iconToShow != null)
        {
            if (!iconToShow.activeSelf)
            {
                iconToShow.SetActive(true);
                iconJustShown = true; // 圖示是現在才顯示的
            }
        }

        // 如果圖示剛剛出現，播放音效
        if (iconJustShown && appearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(appearSound);
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
