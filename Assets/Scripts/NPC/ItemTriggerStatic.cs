using UnityEngine;
using UnityEngine.UI; // 確保引入


public class ItemTriggerStatic : MonoBehaviour
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
            Debug.Log("ItemTriggerStatic: 已經互動過，跳過");
            return; // 避免重複觸發
        }
        
        Debug.Log($"ItemTriggerStatic.Interact() 開始 - itemToGive: {itemToGive?.ItemName}");
        
        // ✅ 先檢查 IconManager 是否存在
        if (IconManager.Instance == null)
        {
            Debug.LogError("ItemTriggerStatic: IconManager.Instance 是 null！請確保場景中有 IconManager");
            return;
        }
        
        // ✅ 先 生成圖示
        if (iconToShow != null)
        {
            Debug.Log($"ItemTriggerStatic: 準備生成圖示，iconToShow = {iconToShow.name}");
        
            Sprite spriteToUse = null;
            Image img = iconToShow.GetComponent<Image>();
            if (img != null)
            {
                spriteToUse = img.sprite;
                Debug.Log($"ItemTriggerStatic: 找到 Image 組件，sprite = {spriteToUse?.name}");
            }
            else
                Debug.LogWarning("ItemTriggerStatic: iconToShow 沒有 Image 元件");

            if (spriteToUse != null && itemToGive != null)
            {
                IconData icon = new IconData(
                    IconType.Object,
                    spriteToUse,
                    itemToGive.ItemID.ToString(),
                    itemToGive.ItemName,
                    "",  // description
                    itemToGive.ItemID  // ✅ 關聯背包物品 ID
                );

                bool addedIcon = IconManager.Instance.AddIcon(icon);
                Debug.Log($"ItemTriggerStatic: AddIcon 結果 = {addedIcon}");
                /* IconManager.Instance?.AddIcon(icon); */
            }
            else
            {
                Debug.LogWarning($"ItemTriggerStatic: 無法生成圖示 - spriteToUse={spriteToUse}, itemToGive={itemToGive}");
            }
        }
        else
        {
            Debug.LogWarning("ItemTriggerStatic: iconToShow 是 null，跳過圖示生成");
        }


        // ✅ 再給背包
        if (itemToGive != null)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("ItemTriggerStatic: InventoryManager.Instance 是 null！");
                return;
            }
            
            bool added = InventoryManager.Instance.AddItem(itemToGive);
            if (!added)
            {
                Debug.LogWarning("背包已滿，無法加入物品: " + itemToGive.ItemName);
                return;
            }

            Debug.Log($"ItemTriggerStatic: 物品 {itemToGive.ItemName} 已加入背包");

            // ✅ 播放 "Pickup" 音效
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D("Pickup");
            }
            else
            {
                Debug.LogWarning("ItemTriggerStatic: SoundManager.Instance 是 null，無法播放音效");
            }
        }
        else
        {
            Debug.LogWarning("ItemTriggerStatic: itemToGive 是 null");
        }

        // ✅ 設定已互動旗標
        hasGivenItem = true;
        Debug.Log("ItemTriggerStatic: 設定 hasGivenItem = true");


        // ✅ 播放音效
        if (appearSound != null)
        {
            if (useOneShotAudio)
            {
                // 用 PlayClipAtPoint 播放一次性音效
                AudioSource.PlayClipAtPoint(appearSound, Camera.main.transform.position, 1f);
                Debug.Log("ItemTriggerStatic: 播放 appearSound (PlayClipAtPoint)");
            }
            else if (audioSource != null)
            {
                // 原本 Inspector 指派的 AudioSource 播放
                audioSource?.PlayOneShot(appearSound);
                Debug.Log("ItemTriggerStatic: 播放 appearSound (AudioSource)");
            }
        }

        
        Debug.Log("ItemTriggerStatic: 互動完成，物件保留在場景中");
        
    } 
}
