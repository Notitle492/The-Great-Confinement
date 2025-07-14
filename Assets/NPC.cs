using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class NPC : MonoBehaviour, IInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            //interact
            Interact();
        }
    }

    public abstract void Interact();
}
