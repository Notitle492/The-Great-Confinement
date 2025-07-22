using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;

    private void Awake()
    {
        playerInRange = false;
        if (visualCue != null)
            visualCue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (visualCue != null)
                visualCue.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E)) // 你可以改成自己的輸入方式
            {
                DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
                // 之後你可以呼叫 InkDialogueManager 來啟動對話
            }
        }
        else
        {
            if (visualCue != null)
                visualCue.SetActive(false);
        }
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
}
