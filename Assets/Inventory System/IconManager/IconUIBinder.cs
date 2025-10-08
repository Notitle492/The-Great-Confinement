using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconUIBinder : MonoBehaviour
{
    [Header("PuzzleUI 場景內的容器與 Slot Prefab")]
    public Transform synthesisContainer;
    public Transform slotContainer;
    public GameObject slotPrefab;
    [SerializeField] private GameObject puzzleUI;

    private void Start()
    {
        // 若 IconManager 尚未存在，先建立一個（方便測試 / 不需手動把 IconManager 放在起始場景）
        if (IconManager.Instance == null)
        {
            GameObject go = new GameObject("IconManager");
            go.AddComponent<IconManager>();
            Debug.Log("IconUIBinder: 自動建立 IconManager（因為找不到 Instance）");
        }

        if (IconManager.Instance != null)
        {
            IconManager.Instance.BindUI(slotContainer, slotPrefab,
                    synthesisContainer, puzzleUI);
        }
        else
        {
            Debug.LogWarning("IconUIBinder: 找不到 IconManager。請手動在起始場景放置 IconManager。");
        }
    }

    private void OnDestroy()
    {
        if (IconManager.Instance != null)
        {
            IconManager.Instance.UnbindUI();
        }
    }
}
