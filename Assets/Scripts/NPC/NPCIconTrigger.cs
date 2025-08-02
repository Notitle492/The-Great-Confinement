using UnityEngine;

public class NPCIconTrigger : MonoBehaviour
{
    public GameObject iconObject; // 要出現的圖示
    private bool hasTalked = false;

    // DialogueManager 呼叫這個方法，代表對話結束
    public void OnDialogueEnded()
    {
        if (!hasTalked)
        {
            iconObject.SetActive(true);
            hasTalked = true;
        }
    }
}
