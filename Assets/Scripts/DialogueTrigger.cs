using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 確保引入

public class DialogueTrigger : MonoBehaviour
{

    [Header("=== 物件識別 ===")]
    [Tooltip("唯一識別ID（用於跨場景記錄互動狀態）")]
    public string uniqueObjectID;

    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    [Header("第一次對話圖示")]
    public Sprite ItemImage; // 對話圖示圖
    public string ItemID; // 唯一ID
    public string ItemName;

    [Header("第二次對話圖示")]
    public Sprite SecondItemImage;
    public string SecondItemID;
    public string SecondItemName;


    [Tooltip("是否在對話結束後給圖示")]
    public bool giveIconAfterDialogue = false;  // 新增開關
    [Tooltip("對話結束後是否讓物件消失")]
    public bool disappearAfterDialogue = false;   

    [Header("多次對話設定")]
    [Tooltip("是否支援多次對話（不同內容）")]
    public bool supportMultipleDialogues = false;
    
    [Tooltip("第二次對話的 Knot 名稱")]
    public string secondDialogueKnot = "";
    
    [Tooltip("第一次對話的 Knot 名稱（預設為 Chapter1）")]
    public string firstDialogueKnot = "Chapter1";

    [Header("條件對話設定")]
    [Tooltip("是否啟用條件對話（依據擁有的圖示觸發不同對話）")]
    public bool useConditionalDialogue = false;

    [System.Serializable]
    public class ConditionalDialogue
    {
        [Tooltip("需要擁有的圖示ID（例如：6）")]
        public string requiredIconID;

        [Tooltip("擁有該圖示時要播放的 Knot 名稱")]
        public string dialogueKnot;

        [Tooltip("條件描述（方便識別）")]
        public string description;
    }

    [Tooltip("條件對話列表（按順序檢查）")]
    public List<ConditionalDialogue> conditionalDialogues = new List<ConditionalDialogue>();

    [Header("對話結束後條件獎勵")]
    [Tooltip("是否在對話結束後檢查條件並給予獎勵")]
    public bool giveRewardAfterDialogue = false;

    [System.Serializable]
    public class DialogueEndReward
    {
        [Header("觸發條件")]
        [Tooltip("需要播放的對話 Knot（例如：player_has_icon8）")]
        public string requiredDialogueKnot;

        [Header("獎勵圖示")]
        [Tooltip("滿足條件後給予的獎勵圖示")]
        public Sprite rewardSprite;
        public string rewardIconID;
        public string rewardIconName;

        [Tooltip("給予獎勵時要移除的圖示ID列表")]
        public List<string> iconsToRemoveOnReward = new List<string>();

        [Tooltip("條件描述（方便識別）")]
        public string description;
    }

    [Tooltip("對話結束後的獎勵列表（按順序檢查）")]
    public List<DialogueEndReward> dialogueEndRewards = new List<DialogueEndReward>();


    [Header("第二次互動獎勵設定")]
    [Tooltip("是否在第二次互動時檢查特殊條件並給予獎勵圖示")]
    public bool checkSecondInteractionReward = false;

    [System.Serializable]
    public class RewardCondition
    {
        [Header("條件類型")]
        [Tooltip("檢查單一圖示 (true) 或檢查配方組合 (false)")]
        public bool checkSingleIcon = true;

        [Header("單一圖示條件")]
        [Tooltip("需要擁有的圖示ID（例如：5）")]
        public string requiredIconID;

        [Header("配方組合條件")]
        [Tooltip("需要擁有的配方列表（任一配方符合即可）")]
        public List<SynthesisRecipe> requiredRecipes = new List<SynthesisRecipe>();

        [Header("獎勵圖示")]
        [Tooltip("滿足條件後給予的獎勵圖示")]
        public Sprite rewardSprite;
        public string rewardIconID;
        public string rewardIconName;

        [Tooltip("給予獎勵時要移除的圖示ID列表（例如：給圖示6時移除圖示5）")]
        public List<string> iconsToRemoveOnReward = new List<string>();

        [Tooltip("條件描述（方便識別）")]
        public string description;
    }

    [Tooltip("第二次互動的獎勵條件列表（按順序檢查,只會觸發第一個符合的）")]
    public List<RewardCondition> secondInteractionRewards = new List<RewardCondition>();

    ////private bool hasTalked = false;
    private bool playerInRange;
    private int interactCount = 0;
    private bool hasGivenSecondInteractionReward = false; // 防止重複給予獎勵
    private string lastPlayedKnot = "";

    // 用於跨場景記錄的 Key
    private string InteractCountKey => $"{uniqueObjectID}_InteractCount";
    private string HasGivenRewardKey => $"{uniqueObjectID}_HasGivenReward";

    private void Awake()
    {
        playerInRange = false;
        if (visualCue != null)
            visualCue.SetActive(false);
    }

    private void Start()
    {
        // 從 InteractionStateManager 載入跨場景狀態
        LoadInteractionState();

        // 如果物件已經設定為「對話後消失」且已經互動過，直接隱藏
        if (disappearAfterDialogue && interactCount > 0)
        {
            gameObject.SetActive(false);
            Debug.Log($"[DialogueTrigger] {uniqueObjectID} 已互動過且設定為消失，物件已隱藏");
        }
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            if (visualCue != null)
                visualCue.SetActive(true);

            if (InputManager.GetInstance().GetInteractPressed())
            {
                Interact();
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
        SaveInteractionState(); // 每次互動後儲存狀態

        Debug.Log($"[DialogueTrigger] {uniqueObjectID} 互動開始 interactCount={interactCount}, hasGivenSecondInteractionReward={hasGivenSecondInteractionReward}");

        // 關鍵修改：第2次互動時，先檢查是否滿足獎勵條件
        if (interactCount == 2 && checkSecondInteractionReward && !hasGivenSecondInteractionReward)
        {
            Debug.Log($"[DialogueTrigger] {uniqueObjectID} 進入第2次互動獎勵檢查");
            bool rewardGiven = CheckAndGiveSecondInteractionReward();

            if (rewardGiven)
            {
                Debug.Log($"[DialogueTrigger] {uniqueObjectID} 第2次互動：滿足條件，已給予獎勵圖示，不播放對話");
                return;
            }
        }

        // 如果沒有給予獎勵，正常播放對話
        string knotToPlay = DetermineDialogueKnot();
        lastPlayedKnot = knotToPlay; // 記錄播放的 Knot
        Debug.Log($"[DialogueTrigger] {uniqueObjectID} 第 {interactCount} 次互動，播放: {knotToPlay}");
        DialogueManager.GetInstance().StartDialogue(inkJSON, this.gameObject, knotToPlay);

    }

    /// 決定要播放哪個對話 Knot
    /// 優先順序：條件對話 > 多次對話 > 預設對話

    private string DetermineDialogueKnot()
    {
        if (useConditionalDialogue && conditionalDialogues.Count > 0)
        {
            foreach (var condition in conditionalDialogues)
            {
                if (HasIcon(condition.requiredIconID))
                {
                    Debug.Log($"[DialogueTrigger] 滿足條件：擁有圖示 {condition.requiredIconID}，播放 {condition.dialogueKnot}");
                    return condition.dialogueKnot;
                }
            }
        }

        if (supportMultipleDialogues && !string.IsNullOrEmpty(secondDialogueKnot))
        {
            if (interactCount == 1)
            {
                return firstDialogueKnot;
            }
            else
            {
                return secondDialogueKnot;
            }
        }

        return firstDialogueKnot;
    }



    /// 檢查玩家是否擁有指定ID的圖示
    private bool HasIcon(string iconID)
    {
        if (string.IsNullOrEmpty(iconID))
            return false;

        if (IconManager.Instance == null)
        {
            Debug.LogWarning("[DialogueTrigger] IconManager.Instance 是 null，無法檢查圖示");
            return false;
        }

        var unlockedIcons = IconManager.Instance.GetUnlockedIcons();
        foreach (var icon in unlockedIcons)
        {
            if (icon.id == iconID)
            {
                return true;
            }
        }

        return false;
    }

    /// 檢查玩家是否擁有配方所需的所有圖示
    private bool HasRecipeMaterials(SynthesisRecipe recipe)
    {
        if (recipe == null || IconManager.Instance == null)
            return false;

        var unlockedIcons = IconManager.Instance.GetUnlockedIcons();

        foreach (var requiredID in recipe.requiredIconIDs)
        {
            bool found = false;
            foreach (var icon in unlockedIcons)
            {
                if (icon.id == requiredID)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log($"[DialogueTrigger] 缺少配方材料：{requiredID}");
                return false;
            }
        }

        Debug.Log($"[DialogueTrigger] 擁有配方 {recipe.recipeName} 的所有材料");
        return true;
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
        // 優先檢查「對話結束後條件獎勵」
        if (giveRewardAfterDialogue && dialogueEndRewards.Count > 0)
        {
            CheckAndGiveDialogueEndReward();
        }

        // 原有的「對話結束後給圖示」邏輯
        if (!giveIconAfterDialogue)
        {
            // 如果設定為消失，執行消失邏輯
            if (disappearAfterDialogue)
            {
                gameObject.SetActive(false);
                Debug.Log($"[DialogueTrigger] 對話結束後物件已隱藏：{gameObject.name}");
            }
            return;
        }

        // 第一次對話 → 第一次圖示
        if (interactCount == 1)
        {
            if (ItemImage == null || string.IsNullOrEmpty(ItemID))
            {
                Debug.LogWarning($"[DialogueTrigger] 第一次對話圖示資料不完整！ItemImage={ItemImage}, ItemID={ItemID}");
                return;
            }

            IconData icon = new IconData(
                IconType.Dialogue,
                ItemImage,
                ItemID,
                ItemName
            );
            IconManager.Instance?.AddIcon(icon);
            Debug.Log($"[DialogueTrigger] 第一次對話結束，已加入圖示：{ItemName}");
        }

        // 第二次對話 → 第二次圖示（如果有設定的話）
        else if (interactCount == 2 && SecondItemImage != null && !string.IsNullOrEmpty(SecondItemID))
        {
            IconData icon = new IconData(
                IconType.Dialogue,
                SecondItemImage,
                SecondItemID,
                SecondItemName
            );

            IconManager.Instance?.AddIcon(icon);
            Debug.Log($"[DialogueTrigger] 第二次對話結束，已加入圖示：{SecondItemName}");
        }


        // 對話後消失（如果有設定）
        if (disappearAfterDialogue)
        {
            gameObject.SetActive(false);
            Debug.Log($"[DialogueTrigger] 對話結束後物件已隱藏：{gameObject.name}");
        }
    }

    /// 檢查並給予對話結束後的條件獎勵
    private void CheckAndGiveDialogueEndReward()
    {
        foreach (var reward in dialogueEndRewards)
        {
            // 檢查是否播放了指定的對話
            if (lastPlayedKnot == reward.requiredDialogueKnot)
            {
                Debug.Log($"[DialogueTrigger] ✅ 播放了 {lastPlayedKnot}，給予獎勵：{reward.rewardIconName}");
                GiveDialogueEndReward(reward);
                return; // 只給予第一個符合的獎勵
            }
        }
    }

    ///給予對話結束後的獎勵
    private void GiveDialogueEndReward(DialogueEndReward reward)
    {
        if (reward.rewardSprite == null || string.IsNullOrEmpty(reward.rewardIconID))
        {
            Debug.LogWarning("[DialogueTrigger] 獎勵圖示資料不完整！");
            return;
        }

        // 1. 先移除舊圖示
        if (reward.iconsToRemoveOnReward != null && reward.iconsToRemoveOnReward.Count > 0)
        {
            foreach (var iconIDToRemove in reward.iconsToRemoveOnReward)
            {
                if (string.IsNullOrEmpty(iconIDToRemove))
                    continue;

                if (IconManager.Instance != null)
                {
                    bool removed = IconManager.Instance.RemoveIconByID(iconIDToRemove);
                    if (removed)
                    {
                        Debug.Log($"[DialogueTrigger] 已移除舊圖示：{iconIDToRemove}");
                    }
                }
            }
        }

        // 2. 給予新的獎勵圖示
        IconData rewardIcon = new IconData(
            IconType.Dialogue,
            reward.rewardSprite,
            reward.rewardIconID,
            reward.rewardIconName
        );

        bool added = IconManager.Instance?.AddIcon(rewardIcon) ?? false;
        if (added)
        {
            Debug.Log($"[DialogueTrigger] 對話結束後獎勵已給予：{reward.rewardIconName}");
        }
    }

    /// 檢查並給予第二次互動的特殊獎勵
    private bool CheckAndGiveSecondInteractionReward()
    {
        foreach (var condition in secondInteractionRewards)
        {
            bool conditionMet = false;

            // 檢查單一圖示條件
            if (condition.checkSingleIcon)
            {
                conditionMet = HasIcon(condition.requiredIconID);
                if (conditionMet)
                {
                    Debug.Log($"[DialogueTrigger] 滿足單一圖示條件：擁有圖示 {condition.requiredIconID}");
                }
            }
            // 檢查配方組合條件
            else
            {
                foreach (var recipe in condition.requiredRecipes)
                {
                    if (HasRecipeMaterials(recipe))
                    {
                        conditionMet = true;
                        Debug.Log($"[DialogueTrigger] 滿足配方條件：擁有配方 {recipe.recipeName} 的材料");
                        break; // 只要有一個配方符合就通過
                    }
                }
            }

            // 如果條件滿足，給予獎勵
            if (conditionMet)
            {
                GiveRewardIcon(condition);
                hasGivenSecondInteractionReward = true; // 標記已給予獎勵
                SaveInteractionState(); // 給予獎勵後儲存狀態
                return true; // 返回 true 表示已給予獎勵
            }
        }

        Debug.Log($"[DialogueTrigger] 沒有滿足任何第二次互動的獎勵條件");
        return false; // 返回 false 表示未給予獎勵
    }

    /// 給予獎勵圖示
    private void GiveRewardIcon(RewardCondition condition)
    {
        if (condition.rewardSprite == null || string.IsNullOrEmpty(condition.rewardIconID))
        {
            Debug.LogWarning("[DialogueTrigger] 獎勵圖示資料不完整！");
            return;
        }

        // 先移除舊圖示（如果有設定）
        if (condition.iconsToRemoveOnReward != null && condition.iconsToRemoveOnReward.Count > 0)
        {
            foreach (var iconIDToRemove in condition.iconsToRemoveOnReward)
            {
                if (string.IsNullOrEmpty(iconIDToRemove))
                    continue;

                if (IconManager.Instance != null)
                {
                    bool removed = IconManager.Instance.RemoveIconByID(iconIDToRemove);
                    if (removed)
                    {
                        Debug.Log($"[DialogueTrigger] 已移除舊圖示：{iconIDToRemove}（因為獲得了 {condition.rewardIconName}）");
                    }
                    else
                    {
                        Debug.LogWarning($"[DialogueTrigger] 無法移除圖示：{iconIDToRemove}（可能不存在）");
                    }
                }
            }
        }

        // 給予新的獎勵圖示
        IconData rewardIcon = new IconData(
            IconType.Dialogue,
            condition.rewardSprite,
            condition.rewardIconID,
            condition.rewardIconName
        );

        bool added = IconManager.Instance?.AddIcon(rewardIcon) ?? false;
        if (added)
        {
            Debug.Log($"[DialogueTrigger] 第二次互動獎勵已給予：{condition.rewardIconName} (條件: {condition.description})");
        }
        else
        {
            Debug.Log($"[DialogueTrigger] 獎勵圖示已存在：{condition.rewardIconName}");
        }
    }

    // 載入跨場景互動狀態
    private void LoadInteractionState()
    {
        if (InteractionStateManager.Instance == null)
        {
            Debug.LogWarning($"[DialogueTrigger] InteractionStateManager.Instance 是 null，無法載入狀態");
            return;
        }

        // 載入互動次數
        string countData = InteractionStateManager.Instance.GetInteractionData(InteractCountKey);
        if (!string.IsNullOrEmpty(countData) && int.TryParse(countData, out int savedCount))
        {
            interactCount = savedCount;
            Debug.Log($"[DialogueTrigger] {uniqueObjectID} 載入互動次數：{interactCount}");
        }

        // 載入是否已給予獎勵
        string rewardData = InteractionStateManager.Instance.GetInteractionData(HasGivenRewardKey);
        if (!string.IsNullOrEmpty(rewardData) && bool.TryParse(rewardData, out bool savedReward))
        {
            hasGivenSecondInteractionReward = savedReward;
            Debug.Log($"[DialogueTrigger] {uniqueObjectID} 載入獎勵狀態：{hasGivenSecondInteractionReward}");
        }
    }


    // 儲存跨場景互動狀態
    private void SaveInteractionState()
    {
        if (InteractionStateManager.Instance == null)
        {
            Debug.LogWarning($"[DialogueTrigger] InteractionStateManager.Instance 是 null，無法儲存狀態");
            return;
        }

        // 儲存互動次數
        InteractionStateManager.Instance.SetInteractionData(InteractCountKey, interactCount.ToString());

        // 儲存是否已給予獎勵
        InteractionStateManager.Instance.SetInteractionData(HasGivenRewardKey, hasGivenSecondInteractionReward.ToString());

        Debug.Log($"[DialogueTrigger] {uniqueObjectID} 已儲存互動狀態：interactCount={interactCount}, hasGivenReward={hasGivenSecondInteractionReward}");
    }


    //在 Inspector 中自動生成 ID
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueObjectID))
        {
            string sceneName = gameObject.scene.name;
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "Prefab";
            }

            uniqueObjectID = $"{sceneName}_{gameObject.name}";
            Debug.Log($"[DialogueTrigger] 自動生成ID：{uniqueObjectID}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        if (string.IsNullOrEmpty(uniqueObjectID))
        {
            Debug.LogError($"[DialogueTrigger] {gameObject.name}: uniqueObjectID 不能為空！");
        }
    }


}


