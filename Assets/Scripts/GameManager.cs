using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

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
    }

    //private void CleanUpAndDestory()
    //{
    //    foreach (GameObject obj in persistentObjects)
    //    {

    //        Destroy(obj);

    //    }
    //    Destroy(gameObject);
    //}
}
