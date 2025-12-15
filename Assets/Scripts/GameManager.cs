using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    private bool isInitialized = false; // 新增：防止重複初始化

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
