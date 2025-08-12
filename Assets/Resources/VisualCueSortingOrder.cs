using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisualCueSortingOrder : MonoBehaviour
{
    public SpriteRenderer visualCueSpriteRenderer;
    public SpriteRenderer playerSpriteRenderer;

    void Update()
    {
        if (visualCueSpriteRenderer != null && playerSpriteRenderer != null)
        {
            visualCueSpriteRenderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            visualCueSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1;
        }
    }
}

