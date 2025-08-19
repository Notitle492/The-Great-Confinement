using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    public Item itemToGive;

    private bool hasGivenItem = false;

    [Header("圖示與音效")]

    public GameObject iconToShow;      // 更通用：要出現的任意圖示
    public AudioClip appearSound;      // 出現時的音效
    public AudioSource audioSource;    // 直接在 Inspector 指派的 AudioSource
    public bool useOneShotAudio = false; // 是否用 PlayClipAtPoint 播放一次性音效


    private void Awake()
    {
        /// 如果沒有在 Inspector 指派，就自動取得
        if (audioSource == null && !useOneShotAudio)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            /* audioSource = gameObject.AddComponent<AudioSource>(); */

            audioSource.spatialBlend = 0f; // 0 是 2D 音效，1 是 3D 音效
            // 建議避免音效自動播放
            audioSource.playOnAwake = false;
            audioSource.volume = 1f; // 確保音量正常
            audioSource.enabled = true; // 確保啟用
        }
        
    }

    public void Interact()
    {
        if (hasGivenItem) return; // 避免重複觸發

        bool added = false;

        if (itemToGive != null)
        {
            added = InventoryManager.Instance.AddItem(itemToGive);
            if (!added)
            {
                Debug.LogWarning("背包已滿，無法加入物品: " + itemToGive.ItemName);
                return;
            }
            hasGivenItem = true;
        }

        if (iconToShow != null && !iconToShow.activeSelf)
        {
            iconToShow.SetActive(true);
        }

        if (appearSound != null)
        {
            if (useOneShotAudio)
                // 用 PlayClipAtPoint 播放一次性音效
                AudioSource.PlayClipAtPoint(appearSound, Camera.main.transform.position, 1f);
                
            else if (audioSource != null)
                // 原本 Inspector 指派的 AudioSource 播放
                audioSource.PlayOneShot(appearSound);
        }
        // ✅ 成功交互後，摧毀父物件（那個場景物件會消失）
        Destroy(transform.parent.gameObject);
    }         
}



/* public void OnDialogueEnded()
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
    } */

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
