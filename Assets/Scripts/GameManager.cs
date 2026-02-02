using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    [Header("InventoryManager 控制")]
    [Tooltip("在這些場景中會隱藏 InventoryManager")]
    public List<string> scenesWithoutInventory = new List<string>
    {
        "CutScene",
        "MainMenu",
        "FriendACutScene",
        "FriendAExitCutScene",
        "TestPrepClassroom",
        "EnglishListeningTest"
        // 在這裡添加其他不需要 InventoryManager 的場景名稱
    };

    private bool isInitialized = false; //防止重複初始化

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Found duplicate GameManager. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!isInitialized)
        {
            MarkPersistentObjects();
            isInitialized = true;

            // 訂閱場景載入事件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null && obj.scene.name != null)
            {
                DontDestroyOnLoad(obj);
                Debug.Log($"標記為跨場景保留：{obj.name}");
            }
        }
        // 確認關鍵 Manager 已初始化
        StartCoroutine(VerifyManagers());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] 場景已載入：{scene.name}");
        UpdateInventoryManagerVisibility(scene.name);
    }

    private void UpdateInventoryManagerVisibility(string sceneName)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[GameManager] InventoryManager.Instance 不存在");
            return;
        }

        GameObject inventoryManagerObj = InventoryManager.Instance.gameObject;

        // 檢查當前場景是否在「不需要 InventoryManager」的列表中
        if (scenesWithoutInventory.Contains(sceneName))
        {
            inventoryManagerObj.SetActive(false);
            Debug.Log($"[GameManager] 在場景 {sceneName} 中隱藏 InventoryManager");
        }
        else
        {
            inventoryManagerObj.SetActive(true);
            Debug.Log($"[GameManager] 在場景 {sceneName} 中顯示 InventoryManager");
        }
    }


    public void ShowInventoryManager()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.gameObject.SetActive(true);
            Debug.Log("[GameManager] 手動顯示 InventoryManager");
        }
    }

    public void HideInventoryManager()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.gameObject.SetActive(false);
            Debug.Log("[GameManager] 手動隱藏 InventoryManager");
        }
    }


    public void HidePersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"[GameManager] 隱藏：{obj.name}");
            }
        }
    }

    public void ShowPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"[GameManager] 顯示：{obj.name}");
            }
        }
    }


    private System.Collections.IEnumerator VerifyManagers()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("=== Manager 初始化檢查 ===");
        Debug.Log($"IconManager: {(IconManager.Instance != null ? "OK" : "NO")}");
        Debug.Log($"InventoryManager: {(InventoryManager.Instance != null ? "OK" : "NO")}");
        Debug.Log($"InteractionStateManager: {(InteractionStateManager.Instance != null ? "OK" : "NO")}");
        Debug.Log($"DialogueManager: {(DialogueManager.GetInstance() != null ? "OK" : "NO")}");
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // 取消訂閱場景載入事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    
}
