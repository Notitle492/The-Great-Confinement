using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconUIBinder : MonoBehaviour
{
    [Header("PuzzleUI 場景內的容器與 Slot Prefab")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    private void Start()
    {
        if (IconManager.Instance != null)
        {
            IconManager.Instance.BindUI(slotContainer, slotPrefab);
        }
        else
        {
            Debug.LogWarning("IconUIBinder: 找不到 IconManager。請在啟動場景放一個 IconManager 並勾 DontDestroyOnLoad。");
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
