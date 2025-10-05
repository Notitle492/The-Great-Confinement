using UnityEngine;
using UnityEngine.UI; // 確保引入

public class ItemTrigger : MonoBehaviour
{
    public Item itemToGive;

    private bool hasGivenItem = false;

    [Header("圖示")]

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
        if (hasGivenItem) 
        {
            Debug.Log("ItemTrigger: 已經互動過，跳過");
            return; // 避免重複觸發
        }
        
        Debug.Log($"ItemTrigger.Interact() 開始 - itemToGive: {itemToGive?.ItemName}");
        
        // ✅ 先檢查 IconManager 是否存在
        if (IconManager.Instance == null)
        {
            Debug.LogError("ItemTrigger: IconManager.Instance 是 null！請確保場景中有 IconManager");
            return;
        }
        
        // ✅ 先 生成圖示
        if (iconToShow != null)
        {
            Debug.Log($"ItemTrigger: 準備生成圖示，iconToShow = {iconToShow.name}");
        
            Sprite spriteToUse = null;
            Image img = iconToShow.GetComponent<Image>();
            if (img != null)
            {
                spriteToUse = img.sprite;
                Debug.Log($"ItemTrigger: 找到 Image 組件，sprite = {spriteToUse?.name}");
            }
            else
                Debug.LogWarning("ItemTrigger: iconToShow 沒有 Image 元件");

            if (spriteToUse != null && itemToGive != null)
            {
                IconData icon = new IconData(
                    IconType.Object,
                    spriteToUse,
                    itemToGive.ItemID.ToString(),
                    itemToGive.ItemName
                );

                bool addedIcon = IconManager.Instance.AddIcon(icon);
                Debug.Log($"ItemTrigger: AddIcon 結果 = {addedIcon}");
                /* IconManager.Instance?.AddIcon(icon); */
            }
            else
            {
                Debug.LogWarning($"ItemTrigger: 無法生成圖示 - spriteToUse={spriteToUse}, itemToGive={itemToGive}");
            }
        }
        else
        {
            Debug.LogWarning("ItemTrigger: iconToShow 是 null，跳過圖示生成");
        }


        // ✅ 再給背包
        if (itemToGive != null)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("ItemTrigger: InventoryManager.Instance 是 null！");
                return;
            }
            
            bool added = InventoryManager.Instance.AddItem(itemToGive);
            if (!added)
            {
                Debug.LogWarning("背包已滿，無法加入物品: " + itemToGive.ItemName);
                return;
            }

            Debug.Log($"ItemTrigger: 物品 {itemToGive.ItemName} 已加入背包");

            // ✅ 播放 "Pickup" 音效
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("Pickup");
            }
            else
            {
                Debug.LogWarning("ItemTrigger: SoundManager.Instance 是 null，無法播放音效");
            }
        }
        else
        {
            Debug.LogWarning("ItemTrigger: itemToGive 是 null");
        }

        // ✅ 設定已互動旗標
        hasGivenItem = true;
        Debug.Log("ItemTrigger: 設定 hasGivenItem = true");


        // ✅ 播放音效
        if (appearSound != null)
        {
            if (useOneShotAudio)
            {
                // 用 PlayClipAtPoint 播放一次性音效
                AudioSource.PlayClipAtPoint(appearSound, Camera.main.transform.position, 1f);
                Debug.Log("ItemTrigger: 播放 appearSound (PlayClipAtPoint)");
            }
            else if (audioSource != null)
            {
                // 原本 Inspector 指派的 AudioSource 播放
                audioSource?.PlayOneShot(appearSound);
                Debug.Log("ItemTrigger: 播放 appearSound (AudioSource)");
            }
        }

        // ✅ 成功交互後，摧毀父物件（那個場景物件會消失）
        if (transform.parent != null)
        {
            Debug.Log($"ItemTrigger: 摧毀父物件 {transform.parent.name}");
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Debug.LogWarning("ItemTrigger: transform.parent 是 null，無法摧毀");
        }
    }         
}

